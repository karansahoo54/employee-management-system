import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirm-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="isOpen" class="modal-backdrop fade show"></div>
    <div *ngIf="isOpen" class="modal fade show d-block" tabindex="-1" role="dialog" (click)="onBackdropClick($event)">
      <div class="modal-dialog modal-dialog-centered" role="document">
        <div class="modal-content shadow-lg border-0 rounded-3">
          <div class="modal-header border-0 pb-0">
            <h5 class="modal-title d-flex align-items-center gap-2 fw-semibold">
              <i class="bi bi-exclamation-triangle-fill text-danger fs-4"></i>
              {{ title }}
            </h5>
            <button type="button" class="btn-close" (click)="cancel()"></button>
          </div>
          <div class="modal-body text-muted py-3">
            {{ message }}
          </div>
          <div class="modal-footer border-0 pt-0">
            <button type="button" class="btn btn-light" (click)="cancel()">{{ cancelText }}</button>
            <button type="button" class="btn" [ngClass]="confirmBtnClass" (click)="confirm()">
              {{ confirmText }}
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .modal-backdrop {
      background-color: rgba(15, 23, 42, 0.6);
      backdrop-filter: blur(2px);
    }
    .modal-content {
      border-radius: 12px;
    }
  `]
})
export class ConfirmModalComponent {
  @Input() isOpen = false;
  @Input() title = 'Confirm Action';
  @Input() message = 'Are you sure you want to proceed?';
  @Input() confirmText = 'Delete';
  @Input() cancelText = 'Cancel';
  @Input() confirmBtnClass = 'btn-danger';

  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  confirm(): void {
    this.confirmed.emit();
  }

  cancel(): void {
    this.cancelled.emit();
  }

  onBackdropClick(e: MouseEvent): void {
    if ((e.target as HTMLElement).classList.contains('modal')) {
      this.cancel();
    }
  }
}
