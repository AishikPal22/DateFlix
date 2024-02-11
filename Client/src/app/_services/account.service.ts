import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { User } from '../_models/user';
 
@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = 'http://localhost:5119/api/'; //change this before production, import from environments folder.
  private currentUserSource = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUserSource.asObservable();
 
  constructor(private http: HttpClient) { }
 
  login(model: any) {
    return this.http.post<User>(this.baseUrl + 'Account/Login', model).pipe(
      map((response: User) => {
        const user = response;
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user);
        }
      })
    );
  }
 
  register(model: any) {
    return this.http.post<User>(this.baseUrl + 'Account/Register', model).pipe(
      map(user => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user);
        }
        return user;
      })
    )
  }
 
  setCurrentUser(user: User) {
    this.currentUserSource.next(user);
  }
 
  logout() {
    localStorage.removeItem('user');
    localStorage.clear();
    sessionStorage.removeItem('user');
    sessionStorage.clear();
    this.currentUserSource.next(null);
  }
}