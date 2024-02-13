import { ResolveFn } from '@angular/router';
import { Member } from '../_models/member';
import { inject } from '@angular/core';
import { MembersService } from '../_services/members.service';

export const memberDetailedResolver: ResolveFn<Member> = (route, state) => {
  const membersService = inject(MembersService);
  
  return membersService.getMember(route.paramMap.get('username')!);
};
