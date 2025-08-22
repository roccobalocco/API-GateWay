import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import { getAuthHeaders } from './auth.service';

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

  getProducts(): Observable<T[] | null> {
    return this.http.get<T[] | null>(this.apiUrl, {
      headers: getAuthHeaders()
    });
  }

  getProduct(id: number): Observable<T | null> {
    return this.http.get<T | null>(`${this.apiUrl}/${id}`, {
      headers: getAuthHeaders()
    });
  }

  createProduct(item: T | null): Observable<T | null> {
    return this.http.post<T | null>(this.apiUrl, item, {
      headers: getAuthHeaders()
    });
  }

  updateProduct(id: number, item: T | null): Observable<T | null> {
    return this.http.put<T | null>(`${this.apiUrl}/${id}`, item, {
      headers: getAuthHeaders()
    });
  }

  deleteProduct(id: number): Observable<T | null> {
    return this.http.delete<T | null>(`${this.apiUrl}/${id}`, {
      headers: getAuthHeaders()
    });
  }
}
