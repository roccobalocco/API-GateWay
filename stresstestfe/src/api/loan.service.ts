import {BaseService} from './base.service';
import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {User} from './user.service';
import {Book} from './book.service';

export interface Loan {
  id:	number;
  loanDate:	Date;
  returnDate:	Date;
  isReturned:	boolean;
  user:	User | null;
  book:	Book | null;
  status:	string | null;
  comments:	string | null;
}

@Injectable({
  providedIn: 'root'
})
export class LoanService extends BaseService<Loan> {
  constructor(httpClient: HttpClient) {
    super(httpClient, 'Loan');
  }
}
