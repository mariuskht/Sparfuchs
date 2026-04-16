import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isLoggedIn) return true;

  // Valid JWT cookie but encryption key lost (e.g. page refresh) → re-enter password only
  if (auth.hasCookieSession) return router.createUrlTree(['/reauth']);

  return router.createUrlTree(['/login']);
};
