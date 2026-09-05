import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {
  username = '';
  password = '';
  showPassword = false;
  errorMessage = '';
  isLoading = false;
  submitted = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private toastService: ToastService
  ) {}

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  fillDemoCredentials(): void {
    this.username = 'admin';
    this.password = 'Admin@123';
    this.errorMessage = '';
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = '';

    // Synchronize with DOM in case browser password manager autofilled without triggering events
    const userEl = document.querySelector('input[name="username"]') as HTMLInputElement;
    const passEl = document.querySelector('input[name="password"]') as HTMLInputElement;

    const trimmedUser = (this.username || userEl?.value || '').trim();
    const cleanPass = this.password || passEl?.value || '';

    if (!trimmedUser || !cleanPass) {
      this.errorMessage = 'Please enter both username and password.';
      return;
    }

    this.isLoading = true;

    this.authService.login({ username: trimmedUser, password: cleanPass }).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.toastService.success(`Welcome back, ${res.username || 'Admin'}!`);
        this.router.navigate(['/employees']);
      },
      error: (err: any) => {
        this.isLoading = false;
        if (err.status === 401) {
          this.errorMessage = 'Invalid username or password. Please try again.';
          this.toastService.error('Invalid credentials');
        } else {
          this.errorMessage = 'Unable to connect to the server. Please check the backend API.';
          this.toastService.error('Connection error');
        }
      }
    });
  }
}
