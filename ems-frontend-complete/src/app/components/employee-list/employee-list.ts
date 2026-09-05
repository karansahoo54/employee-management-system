import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { EmployeeService, Employee } from '../../services/employee';
import { ToastService } from '../../services/toast.service';
import { ConfirmModalComponent } from '../confirm-modal/confirm-modal';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, ConfirmModalComponent],
  templateUrl: './employee-list.html',
  styleUrl: './employee-list.css'
})
export class EmployeeListComponent implements OnInit {
  employees: Employee[] = [];
  selectedIds: number[] = [];
  isLoading = false;
  errorMessage = '';

  // Filter & Search
  searchTerm = '';
  statusFilter = 'ALL'; // 'ALL', 'ACTIVE', 'INACTIVE'

  // Modal State
  modalOpen = false;
  modalTitle = '';
  modalMessage = '';
  modalAction: 'SINGLE' | 'BULK' = 'SINGLE';
  targetEmployeeId: number | null = null;

  constructor(
    private employeeService: EmployeeService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.employeeService.getAll().subscribe({
      next: (data) => {
        this.employees = data;
        this.isLoading = false;
        // Clean selection of any deleted IDs
        const existingIds = new Set(data.map(e => e.employeeId));
        this.selectedIds = this.selectedIds.filter(id => existingIds.has(id));
      },
      error: () => {
        this.errorMessage = 'Failed to load employees from the server.';
        this.isLoading = false;
        this.toastService.error('Error fetching employee directory');
      }
    });
  }

  get filteredEmployees(): Employee[] {
    return this.employees.filter(emp => {
      const term = this.searchTerm.toLowerCase().trim();
      const matchesSearch = !term ||
        emp.fullName.toLowerCase().includes(term) ||
        emp.email.toLowerCase().includes(term) ||
        (emp.departmentName && emp.departmentName.toLowerCase().includes(term)) ||
        (emp.designation && emp.designation.toLowerCase().includes(term));

      const matchesStatus =
        this.statusFilter === 'ALL' ||
        (this.statusFilter === 'ACTIVE' && emp.isActive) ||
        (this.statusFilter === 'INACTIVE' && !emp.isActive);

      return matchesSearch && matchesStatus;
    });
  }

  get allSelected(): boolean {
    const list = this.filteredEmployees;
    return list.length > 0 && list.every(e => this.selectedIds.includes(e.employeeId));
  }

  toggleSelectAll(event: any): void {
    const checked = event.target.checked;
    const currentFilteredIds = this.filteredEmployees.map(e => e.employeeId);

    if (checked) {
      const merged = new Set([...this.selectedIds, ...currentFilteredIds]);
      this.selectedIds = Array.from(merged);
    } else {
      this.selectedIds = this.selectedIds.filter(id => !currentFilteredIds.includes(id));
    }
  }

  toggleSelect(id: number, event: any): void {
    if (event.target.checked) {
      if (!this.selectedIds.includes(id)) {
        this.selectedIds.push(id);
      }
    } else {
      this.selectedIds = this.selectedIds.filter(x => x !== id);
    }
  }

  isSelected(id: number): boolean {
    return this.selectedIds.includes(id);
  }

  getInitials(name: string): string {
    if (!name) return '??';
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.slice(0, 2).toUpperCase();
  }

  // --- Deletion Flow with Custom Modal ---
  promptDeleteSingle(id: number, name: string): void {
    this.modalAction = 'SINGLE';
    this.targetEmployeeId = id;
    this.modalTitle = 'Delete Employee';
    this.modalMessage = `Are you sure you want to permanently delete "${name}"? This action cannot be undone.`;
    this.modalOpen = true;
  }

  promptDeleteBulk(): void {
    if (this.selectedIds.length === 0) return;
    this.modalAction = 'BULK';
    this.modalTitle = 'Delete Selected Employees';
    this.modalMessage = `Are you sure you want to delete ${this.selectedIds.length} selected employees? This action cannot be undone.`;
    this.modalOpen = true;
  }

  onModalConfirm(): void {
    this.modalOpen = false;

    if (this.modalAction === 'SINGLE' && this.targetEmployeeId !== null) {
      const idToDelete = this.targetEmployeeId;
      this.employeeService.delete(idToDelete).subscribe({
        next: () => {
          this.toastService.success('Employee deleted successfully');
          this.loadEmployees();
        },
        error: () => {
          this.toastService.error('Failed to delete employee');
        }
      });
    } else if (this.modalAction === 'BULK' && this.selectedIds.length > 0) {
      const idsToDelete = [...this.selectedIds];
      this.employeeService.deleteBulk(idsToDelete).subscribe({
        next: () => {
          this.toastService.success(`Deleted ${idsToDelete.length} employees successfully`);
          this.selectedIds = [];
          this.loadEmployees();
        },
        error: () => {
          this.toastService.error('Failed to delete selected employees');
        }
      });
    }
  }

  onModalCancel(): void {
    this.modalOpen = false;
    this.targetEmployeeId = null;
  }
}
