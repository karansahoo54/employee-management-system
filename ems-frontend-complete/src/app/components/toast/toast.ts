import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container position-fixed top-0 end-0 p-3" style="z-index: 9999;">
      <div *ngFor="let toast of toastService.toasts()"
           class="toast show align-items-center mb-2 shadow-lg border-0"
           [ngClass]="'toast-' + toast.type"
           role="alert" aria-live="assertive" aria-atomic="true">
        <div class="d-flex">
          <div class="toast-body d-flex align-items-center gap-2">
            <i class="bi" [ngClass]="toast.icon"></i>
            <span>{{ toast.message }}</span>
          </div>
          <button type="button" class="btn-close btn-close-white me-2 m-auto" (click)="toastService.remove(toast.id)"></button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .toast {
      min-width: 280px;
      max-width: 420px;
      border-radius: 8px;
      color: #fff;
      font-size: 0.875rem;
      animation: slideIn 0.25s ease-out;
    }
    .toast-success { background: linear-gradient(135deg, #10b981, #059669); }
    .toast-error { background: linear-gradient(135deg, #ef4444, #dc2626); }
    .toast-info { background: linear-gradient(135deg, #3b82f6, #2563eb); }
    .toast-warning { background: linear-gradient(135deg, #f59e0b, #d97706); }
    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
  `]
})
export class ToastComponent {
  constructor(public toastService: ToastService) {}
}
