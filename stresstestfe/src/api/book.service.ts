import {Injectable} from '@angular/core';
import {BaseService} from './base.service';
import {HttpClient} from '@angular/common/http';
import {Room} from './room.service';

export interface Book {
  id: number;
  name?: string | null;
  author?: string | null;
  publisher?: string | null;
  year: number;
  room: Room | null;
}

@Injectable({
  providedIn: 'root'
})
export class BookService extends BaseService<Book> {
  constructor(httpClient: HttpClient) {
    super(httpClient, 'Book');
  }
}
