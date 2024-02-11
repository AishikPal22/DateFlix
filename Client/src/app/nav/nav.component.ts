import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { MembersService } from '../_services/members.service';
 
@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  // loggedIn: boolean = false;
 
  constructor(public accountService: AccountService, private router: Router, private toastr: ToastrService) { }
 
  ngOnInit(): void {  }
 
  login() {
    this.accountService.login(this.model).subscribe({
      next: _ => {
        //this.memberService.resetUserParams();
        this.router.navigateByUrl('/members');
      },
      error: (error) => {
        if (error.status !== 500) {
          this.toastr.error(error.error);
        }
        this.router.navigateByUrl('/');
      }
    })
  }
 
  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}