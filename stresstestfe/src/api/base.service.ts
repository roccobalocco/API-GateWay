import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BaseService<T> {
  private apiUrl: string = 'http://192.168.49.2/api/Gateway'; // Mock API

  constructor(private http: HttpClient, endpoint: String | null = null) {
    if (endpoint) {
      this.apiUrl += '/' + endpoint;
    }
  }

  private getAuthHeaders(): HttpHeaders {
    if (typeof sessionStorage === 'undefined') {
      // We're on server side, return
      return new HttpHeaders();
    }
    const token = sessionStorage.getItem('token');
    return new HttpHeaders({
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }

  getProducts(): Observable<T[] | null> {
    return this.http.get<T[] | null>(this.apiUrl, {
      headers: this.getAuthHeaders()
    });
  }

  getProduct(id: number): Observable<T | null> {
    return this.http.get<T | null>(`${this.apiUrl}/${id}`, {
      headers: this.getAuthHeaders()
    });
  }

  createProduct(item: T | null): Observable<T | null> {
    return this.http.post<T | null>(this.apiUrl, item, {
      headers: this.getAuthHeaders()
    });
  }

  updateProduct(id: number, item: T | null): Observable<T | null> {
    return this.http.put<T | null>(`${this.apiUrl}/${id}`, item, {
      headers: this.getAuthHeaders()
    });
  }

  deleteProduct(id: number): Observable<T | null> {
    return this.http.delete<T | null>(`${this.apiUrl}/${id}`, {
      headers: this.getAuthHeaders()
    });
  }
}
