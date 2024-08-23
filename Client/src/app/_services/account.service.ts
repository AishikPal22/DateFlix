import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { User } from '../_models/user';
import { NotificationService } from './notification.service';
import { LoadingService } from './loading.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = 'http://localhost:5119/api/'; //change this before production, import from environments folder.
  private currentUserSource = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient, private notificationService: NotificationService, private loadingService: LoadingService) { }

  login(model: any) {
    return this.http.post<User>(this.baseUrl + 'Account/Login', model).pipe(
      map((response: User) => {
        const user = response;
        if (user) {
          this.setCurrentUser(user);
        }
      })
    );
  }

  register(model: any) {
    return this.http.post<User>(this.baseUrl + 'Account/Register', model).pipe(
      map(user => {
        if (user) {
          this.setCurrentUser(user);
        }
        //return user;
      })
    );
  }

  setCurrentUser(user: User) {
    this.loadingService.show();
    try {
      user.roles = [];
      const roles = this.getDecodedToken(user.token).role;
      Array.isArray(roles) ? user.roles = roles : user.roles.push(roles);
      localStorage.setItem('user', JSON.stringify(user));
      this.currentUserSource.next(user);
      this.notificationService.showSuccess('User logged in successfully');
    } catch (error) {
      this.notificationService.showError('Failed to set current user');
    } finally {
      this.loadingService.hide();
    }
  }

  logout() {
    localStorage.removeItem('user');
    localStorage.clear();
    sessionStorage.removeItem('user');
    sessionStorage.clear();
    this.currentUserSource.next(null);
    this.notificationService.showSuccess('User logged out successfully');
  }

  checkTokenExpiration() {
    const userString = localStorage.getItem('user');
    const user = userString ? JSON.parse(userString) : null;
    if (user) {
      const token = user.token;
      const decodedToken = this.getDecodedToken(token);
      const expirationDate = new Date(0);
      expirationDate.setUTCSeconds(decodedToken.exp);
      if (expirationDate < new Date()) {
        this.logout();
        this.notificationService.showError('Session expired. Please log in again.');
      }
    }
  }

  getDecodedToken(token: string) {
    return JSON.parse(atob(token.split('.')[1]));
  }
}