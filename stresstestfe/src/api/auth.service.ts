import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://192.168.49.2/api/Gateway/login'; // Mock API

  constructor(protected http: HttpClient) {
  }

  private setAuthHeaders(token: string): void {
    if (typeof sessionStorage === 'undefined') {
      // We're on server side, return
      return;
    }
    sessionStorage.setItem('token', token);
  }

  login(username: string, password: string): void {
    this.http.post<{ accessToken: string }>(this.apiUrl, {username: username, password: password})
      .subscribe({
        next: (res: { accessToken: string }) => this.setAuthHeaders(res.accessToken),
        error: (err) => console.error(err)
      });
  }
}
