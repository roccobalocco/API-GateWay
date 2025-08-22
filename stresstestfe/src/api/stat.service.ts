import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {getAuthHeaders} from './auth.service';
import {Observable} from 'rxjs';

export interface LoadTestConfig {
  endpoint: string;
  requests: number;
  concurrency: number;
  duration: number;
  method: string;
}
export interface ServiceStats {
  serviceName: string;
  totalRequests: number;
  totalElapsedTime: number;
  lastCalled: string;
  methodCounts: Record<string, number>;
  users: Record<string, number>;
  paths: Record<string, number>;
  avgResponseTime: number;
}

export interface SummaryStatus {
  [serviceName: string]: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class StatsService {
  private apiUrl: string = 'http://192.168.49.2/api/Gateway'; // Mock API
  constructor(private httpClient: HttpClient) {
  }

  /** Fetch boolean health status for each service */
  getSummary(): Observable<SummaryStatus> {
    const headers = getAuthHeaders();
    return this.httpClient.get<SummaryStatus>(`${this.apiUrl}/health/summary`, {headers});
  }

  /** Fetch detailed service stats */
  getServiceStats(): Observable<ServiceStats[]> {
    const headers = getAuthHeaders();
    return this.httpClient.get <ServiceStats[]>(`${this.apiUrl}/stats`, {headers});
  }
}
