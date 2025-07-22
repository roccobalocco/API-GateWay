import {Injectable} from '@angular/core';
import {BaseService} from './base.service';
import {HttpClient} from '@angular/common/http';

export interface User {
  id: number;
  name: string | null;
  email: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class UserService extends BaseService<User> {
  constructor(httpClient: HttpClient) {
    super(httpClient, 'User');
  }
}
