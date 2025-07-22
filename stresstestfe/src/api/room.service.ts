import {Injectable} from '@angular/core';
import {BaseService} from './base.service';
import {HttpClient} from '@angular/common/http';

export interface Room {
  id: number;
  name: string | null;
  description: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class RoomService extends BaseService<Room> {
  constructor(httpClient: HttpClient) {
    super(httpClient, 'Room');
  }
}
