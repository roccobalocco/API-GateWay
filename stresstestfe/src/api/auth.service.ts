import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {catchError, map, Observable, of} from 'rxjs';


export function getAuthHeaders(): HttpHeaders {
  if (typeof sessionStorage === 'undefined') {
    // We're on server side, return
    return new HttpHeaders();
  }
  const token = sessionStorage.getItem('token');
  return new HttpHeaders({
    'Authorization': token ? `Bearer ${token}` : ''
  });
}

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

  login(username: string, password: string): Observable<boolean> {
    return this.http.post<{ accessToken: string }>(this.apiUrl, {username, password})
      .pipe(map(res => {
          this.setAuthHeaders(res.accessToken);
          console.error("logged in");
          return true;
        }),
        catchError(err => {
          console.error(err);
          return of(false);
        })
      );
  }

}
