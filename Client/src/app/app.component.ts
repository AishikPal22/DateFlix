import { Component, OnInit } from '@angular/core';
import { AccountService } from './_services/account.service';
import { User } from './_models/user';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit{
  title = 'Client';
  private tokenCheckInterval: any;

  constructor(private accountService: AccountService) { }
 
  ngOnInit(): void {
    // this.getUsers();
    this.setCurrentUser();
    this.accountService.checkTokenExpiration();
    this.tokenCheckInterval = setInterval(() => {
      this.accountService.checkTokenExpiration();
    }, 60000);
  }
 
  setCurrentUser() {
    const userString = localStorage.getItem('user');
    if (!userString) return;
    const user: User = JSON.parse(userString);
    this.accountService.setCurrentUser(user);
  }

  ngOnDestroy() {
    if (this.tokenCheckInterval) {
      clearInterval(this.tokenCheckInterval);
    }
  }
}
