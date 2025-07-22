import {Component, OnInit, OnDestroy} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Observable, interval, Subscription} from 'rxjs';
import {BookService} from '../api/book.service';
import {AuthService} from '../api/auth.service';
import {UserService} from '../api/user.service';
import {RoomService} from '../api/room.service';
import {LoanService} from '../api/loan.service';

interface HealthSummary {
  status: string;
  checks: any[];
  totalDuration: string;
}

interface Stats {
  uptime: number;
  memoryUsage: number;
  cpuUsage: number;
  requestCount: number;
  errorRate: number;
}

interface LoadTestConfig {
  endpoint: string;
  requests: number;
  concurrency: number;
  duration: number;
  method: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  imports: [CommonModule, FormsModule],
})
export class App implements OnInit, OnDestroy {
  // Authentication
  isAuthenticated = false;
  credentials = {username: '', password: ''};

  // Loading states
  isLoading = false;
  isLoadTesting = false;

  // Health monitoring
  healthSummary: HealthSummary | null = null;
  stats: Stats | null = null;
  autoRefresh = false;
  refreshSubscription?: Subscription;

  // Load testing
  loadTestConfig!: LoadTestConfig;

  loadTestResults: any[] = [];
  testProgress = 0;
  loadTestSubscription?: Subscription;

  private baseUrl = 'http://192.168.49.2/api/Gateway';

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private bookService: BookService,
    private userService: UserService,
    private roomService: RoomService,
    private loanService: LoanService
  ) {
  }

  ngOnInit() {
    this.loadTestConfig = {
      endpoint: 'Book',
      requests: 100,
      concurrency: 10,
      duration: 60,
      method: 'GET'
    };
    if (typeof sessionStorage === 'undefined') {
      // We're on server side, return
      return;
    }
    // Check if already authenticated
    const token = sessionStorage.getItem('token');
    if (token) {
      this.isAuthenticated = true;
      this.refreshHealth();
    }
  }

  ngOnDestroy() {
    this.refreshSubscription?.unsubscribe();
    this.loadTestSubscription?.unsubscribe();
  }

  // Authentication Methods
  login() {
    if (!this.credentials.username || !this.credentials.password) {
      alert('Inserisci username e password');
      return;
    }

    this.isLoading = true;

    this.authService.login(
      this.credentials.username,
      this.credentials.password
    );

    let counter = 5;
    while (!this.isAuthenticated && counter > 0) {
      setTimeout(() => {
        if (typeof sessionStorage === 'undefined') {
          // We're on server side, return headers without Authorization
          counter = 0;
        }else if (sessionStorage.getItem('token')) {
          this.isAuthenticated = true;
        }
      }, 500);
      counter--;
    }

    this.isLoading = false;
    this.refreshHealth();
  }

  // Health Monitoring Methods
  refreshHealth() {
    this.isLoading = true;

    const headers = this.getAuthHeaders();

    // Fetch health summary
    this.http.get<HealthSummary>(`${this.baseUrl}/health/summary`, {headers}).subscribe({
      next: (data) => {
        this.healthSummary = data;
      },
      error: (error) => {
        console.error('Health summary fetch failed:', error);
        this.healthSummary = null;
      }
    });

    // Fetch stats
    this.http.get<Stats>(`${this.baseUrl}/stats`, {headers}).subscribe({
      next: (data) => {
        this.stats = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Stats fetch failed:', error);
        this.stats = null;
        this.isLoading = false;
      }
    });
  }

  toggleAutoRefresh() {
    if (this.autoRefresh) {
      this.refreshSubscription = interval(30000).subscribe(() => {
        this.refreshHealth();
      });
    } else {
      this.refreshSubscription?.unsubscribe();
    }
  }

  // Load Testing Methods
  startLoadTest() {
    this.isLoadTesting = true;
    this.testProgress = 0;

    const testResult = {
      endpoint: this.loadTestConfig.endpoint,
      totalRequests: this.loadTestConfig.requests,
      successRequests: 0,
      errorRequests: 0,
      averageTime: 0,
      successRate: 0,
      startTime: new Date()
    };

    const service = this.getServiceByEndpoint(this.loadTestConfig.endpoint);
    const requestsPerSecond = Math.ceil(this.loadTestConfig.requests / this.loadTestConfig.duration);
    const totalRequests = this.loadTestConfig.requests;
    let completedRequests = 0;
    let totalTime = 0;

    // Simulate concurrent requests
    const executeRequest = () => {
      const startTime = performance.now();

      let observable: Observable<any>;
      switch (this.loadTestConfig.method) {
        case 'GET':
          observable = service.getProducts();
          break;
        case 'POST':
          observable = service.createProduct(this.getMockData());
          break;
        default:
          observable = service.getProducts();
      }

      observable.subscribe({
        next: () => {
          testResult.successRequests++;
          const endTime = performance.now();
          totalTime += (endTime - startTime);
          completedRequests++;
          this.updateTestProgress(completedRequests, totalRequests, testResult, totalTime);
        },
        error: () => {
          testResult.errorRequests++;
          completedRequests++;
          this.updateTestProgress(completedRequests, totalRequests, testResult, totalTime);
        }
      });
    };

    // Execute requests with concurrency control
    const executeWithConcurrency = (remainingRequests: number) => {
      const batchSize = Math.min(this.loadTestConfig.concurrency, remainingRequests);

      for (let i = 0; i < batchSize; i++) {
        executeRequest();
      }

      if (remainingRequests > batchSize) {
        setTimeout(() => {
          executeWithConcurrency(remainingRequests - batchSize);
        }, 1000 / requestsPerSecond);
      }
    };

    executeWithConcurrency(totalRequests);
  }

  stopLoadTest() {
    this.isLoadTesting = false;
    this.loadTestSubscription?.unsubscribe();
  }

  // Helper Methods
  private getAuthHeaders(): HttpHeaders {
    if (typeof sessionStorage === 'undefined') {
      return new HttpHeaders();
    }
    const token = sessionStorage.getItem('token');
    return new HttpHeaders({
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }

  private getServiceByEndpoint(endpoint: string): any {
    switch (endpoint) {
      case 'Book':
        return this.bookService;
      case 'User':
        return this.userService;
      case 'Room':
        return this.roomService;
      case 'Loan':
        return this.loanService;
      default:
        return this.bookService;
    }
  }

  private getMockData(): any {
    switch (this.loadTestConfig.endpoint) {
      case 'Book':
        return {
          name: 'Test Book',
          author: 'Test Author',
          publisher: 'Test Publisher',
          year: 2024,
          room: null
        };
      case 'User':
        return {
          name: 'Test User',
          email: 'test@example.com'
        };
      case 'Room':
        return {
          name: 'Test Room',
          description: 'Test Description'
        };
      case 'Loan':
        return {
          loanDate: new Date(),
          returnDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000),
          isReturned: false,
          user: null,
          book: null,
          status: 'active',
          comments: 'Test loan'
        };
      default:
        return {};
    }
  }

  private updateTestProgress(completed: number, total: number, testResult: any, totalTime: number) {
    this.testProgress = Math.round((completed / total) * 100);

    if (completed === total) {
      testResult.averageTime = Math.round(totalTime / completed);
      testResult.successRate = Math.round((testResult.successRequests / total) * 100);
      this.loadTestResults.unshift(testResult);
      this.isLoadTesting = false;
    }
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'healthy':
        return 'healthy';
      case 'unhealthy':
        return 'unhealthy';
      case 'degraded':
        return 'degraded';
      default:
        return 'degraded';
    }
  }

  formatUptime(seconds: number): string {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    return `${hours}h ${minutes}m ${secs}s`;
  }
}
