import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  // Never attach Authorization header to login or register requests
  const isAuthEndpoint = req.url.includes('/api/Auth/login') || req.url.includes('/api/Auth/register');

  const token = localStorage.getItem('token');
  const isValidToken = token && token.trim().length > 0 && token !== 'undefined' && token !== 'null';

  let outgoingReq = req;

  if (!isAuthEndpoint && isValidToken) {
    outgoingReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
  }

  return next(outgoingReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // If a protected route returns 401 Unauthorized, wipe stale tokens and redirect to login
      if (error.status === 401 && !isAuthEndpoint) {
        localStorage.removeItem('token');
        localStorage.removeItem('username');
        localStorage.removeItem('role');
        router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};
