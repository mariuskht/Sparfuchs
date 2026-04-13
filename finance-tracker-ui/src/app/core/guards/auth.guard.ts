import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/** Redirects to /login if the user is not authenticated or the encryption key is missing. */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isLoggedIn) return true;

  // Token in sessionStorage but key lost (e.g. page refresh) → force re-login
  return router.createUrlTree(['/login']);
};
