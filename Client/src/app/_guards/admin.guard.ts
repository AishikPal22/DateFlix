import { CanActivateFn } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { inject } from '@angular/core';
import { map } from 'rxjs';

export const adminGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const toastr = inject(ToastrService);

  return accountService.currentUser$.pipe(
    map(user => {
      if (!user)
        return false;

      if (user.roles.includes('Admin') || user.roles.includes('Moderator'))
        return true;

      else {
        toastr.error("Access prohibited.");
        return false;
      }
    })
  );
};
