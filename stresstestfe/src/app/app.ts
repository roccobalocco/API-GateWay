import {Component, OnInit, OnDestroy, ChangeDetectorRef} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {HttpClient} from '@angular/common/http';
import {Observable, interval, Subscription, finalize} from 'rxjs';
import {Book, BookService} from '../api/book.service';
import {AuthService} from '../api/auth.service';
import {User, UserService} from '../api/user.service';
import {Room, RoomService} from '../api/room.service';
import {Loan, LoanService} from '../api/loan.service';
import {RateLimitingInfoComponent} from './info/app.info.component';
import {JwtHelperService} from '@auth0/angular-jwt';
import {LoadTestConfig, ServiceStats, StatsService, SummaryStatus} from '../api/stat.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  imports: [CommonModule, FormsModule, RateLimitingInfoComponent],
  providers: [JwtHelperService]
})
export class App implements OnInit, OnDestroy {
  // Authentication
  isAuthenticated = false;
  credentials = {username: '', password: ''};

  // Loading states
  isLoading = false;
  isLoadTesting = false;

  // Health monitoring
  autoRefresh = true;
  refreshSubscription?: Subscription;

  // Load testing
  loadTestConfig!: LoadTestConfig;

  loadTestResults: any[] = [];
  testProgress = 0;
  loadTestSubscription?: Subscription;

  // stats
  servicesStats: ServiceStats[] = [];
  summaryStatus: SummaryStatus = {};

  // base entities
  room: Room = {
    id: 0,
    name: 'Test Room',
    description: 'Test Description'
  };
  book: Book = {
    id: 0,
    name: 'Test Book',
    author: 'Test Author',
    publisher: 'Test Publisher',
    year: 2024,
    room: this.room
  };
  user: User = {
    id: 0,
    name: 'Test User',
    email: 'test@example.com'
  };
  loan: Loan = {
    id: 0,
    loanDate: new Date(),
    returnDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000),
    isReturned: false,
    user: this.user,
    book: this.book,
    status: 'active',
    comments: 'Test loan'
  };
  private baseUrl = 'http://192.168.49.2/api/Gateway';

  constructor(
    private authService: AuthService,
    private bookService: BookService,
    private userService: UserService,
    private roomService: RoomService,
    private loanService: LoanService,
    private statsService: StatsService,
    private jwtHelper: JwtHelperService,
    private cd: ChangeDetectorRef) {
  }

  ngOnInit() {
    this.checkToken();
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


  checkToken() {
    setTimeout(() => {
      if (typeof sessionStorage === 'undefined') {
        // We're on server side, return
        return;
      }
      const token = sessionStorage.getItem('token');
      if (token == null) {
        this.isAuthenticated = false;
        return;
      }
      this.isAuthenticated = !this.jwtHelper.isTokenExpired(token);
      if (!this.isAuthenticated)
        sessionStorage.removeItem('token');
      this.checkToken();
    }, 1000);
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
    ).subscribe(success => {
      if (success) {
        this.isAuthenticated = true;
        this.refreshHealth();
      }
      this.isLoading = false;
      this.cd.markForCheck();
    });
  }

  // Health Monitoring Methods
  refreshHealth() {
    this.isLoading = true;

    // Fetch boolean health status for each service
    this.statsService.getSummary().pipe(
      finalize(() => this.cd.markForCheck())
    ).subscribe({
      next: (data) => {
        this.summaryStatus = data;
      },
      error: (error) => {
        console.error('Health summary fetch failed:', error);
        this.summaryStatus = {};
      }
    });

    // Fetch detailed service stats
    this.statsService.getServiceStats().pipe(
      finalize(() => {
          this.isLoading = false;
          this.cd.markForCheck();
        }
      )).subscribe({
      next: (data) => {
        this.servicesStats = data;
      },
      error: (error) => {
        console.error('Stats fetch failed:', error);
        this.servicesStats = [];
      }
    });
  }

  getServiceHealthClass(serviceName
                        :
                        string
  ):
    string {
    const healthy = this.summaryStatus[serviceName];
    if (healthy === true) return 'healthy';
    if (healthy === false) return 'unhealthy';
    return 'unknown';
  }

  formatAvgResponseTime(ms
                        :
                        number
  ):
    string {
    return ms >= 1000 ? `${(ms / 1000).toFixed(2)}s` : `${ms.toFixed(2)}ms`;
  }

  objectKeys = Object.keys; // for template iteration

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

      observable
        .pipe(
          finalize(() => {
            completedRequests++;
            this.updateTestProgress(completedRequests, totalRequests, testResult, totalTime)
          })
        )
        .subscribe({
          next: () => {
            testResult.successRequests++;
            const endTime = performance.now();
            totalTime += (endTime - startTime);
          },
          error: () => testResult.errorRequests++
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

  getServiceByEndpoint(endpoint
                       :
                       string
  ):
    any {
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

  getMockData()
    :
    any {
    switch (this.loadTestConfig.endpoint) {
      case 'Book':
        return {
          id: 0,
          name: 'Test Book',
          author: 'Test Author',
          publisher: 'Test Publisher',
          year: 2024,
          room: this.room
        };
      case 'User':
        return this.user;
      case 'Room':
        return this.room;
      case 'Loan':
        return this.loan;
      default:
        return {};
    }
  }

  updateTestProgress(completed
                     :
                     number, total
                     :
                     number, testResult
                     :
                     any, totalTime
                     :
                     number
  ) {
    this.testProgress = Math.round((completed / total) * 100);

    if (completed === total) {
      testResult.averageTime = Math.round(totalTime / completed);
      testResult.successRate = Math.round((testResult.successRequests / total) * 100);
      this.loadTestResults.unshift(testResult);
      this.isLoadTesting = false;
    }

    this.cd.markForCheck();
    this.cd.detectChanges();
  }
}
