import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportService } from '../../services/report';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.html',
  styleUrl: './reports.css'
})
export class ReportsComponent {
  loadingStates: Record<string, boolean> = {};

  // Attendance date range filter
  fromDate = '';
  toDate = '';

  constructor(
    private reportService: ReportService,
    private toastService: ToastService
  ) {
    // Default attendance range to current month
    const now = new Date();
    const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
    this.fromDate = firstDay.toISOString().substring(0, 10);
    this.toDate = now.toISOString().substring(0, 10);
  }

  isLoading(key: string): boolean {
    return !!this.loadingStates[key];
  }

  private downloadFile(blob: Blob, filename: string, key: string, successMsg: string): void {
    this.loadingStates[key] = false;
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
    this.toastService.success(successMsg);
  }

  private handleError(key: string, reportName: string): void {
    this.loadingStates[key] = false;
    this.toastService.error(`Failed to generate ${reportName}. Please try again.`);
  }

  downloadEmployeePdf(): void {
    const key = 'emp-pdf';
    this.loadingStates[key] = true;
    this.reportService.downloadEmployeePdf().subscribe({
      next: (blob) => this.downloadFile(blob, 'EmployeeDirectory.pdf', key, 'Employee Directory PDF downloaded'),
      error: () => this.handleError(key, 'Employee Directory PDF')
    });
  }

  downloadEmployeeExcel(): void {
    const key = 'emp-excel';
    this.loadingStates[key] = true;
    this.reportService.downloadEmployeeExcel().subscribe({
      next: (blob) => this.downloadFile(blob, 'EmployeeDirectory.xlsx', key, 'Employee Directory Excel downloaded'),
      error: () => this.handleError(key, 'Employee Directory Excel')
    });
  }

  downloadDepartmentExcel(): void {
    const key = 'dept-excel';
    this.loadingStates[key] = true;
    this.reportService.downloadDepartmentExcel().subscribe({
      next: (blob) => this.downloadFile(blob, 'DepartmentReport.xlsx', key, 'Department Report Excel downloaded'),
      error: () => this.handleError(key, 'Department Report Excel')
    });
  }

  downloadAttendanceExcel(): void {
    const key = 'att-excel';
    this.loadingStates[key] = true;
    this.reportService.downloadAttendanceExcel(this.fromDate || undefined, this.toDate || undefined).subscribe({
      next: (blob) => this.downloadFile(blob, 'AttendanceReport.xlsx', key, 'Attendance Report Excel downloaded'),
      error: () => this.handleError(key, 'Attendance Report Excel')
    });
  }

  downloadSalaryPdf(): void {
    const key = 'sal-pdf';
    this.loadingStates[key] = true;
    this.reportService.downloadSalaryPdf().subscribe({
      next: (blob) => this.downloadFile(blob, 'SalaryReport.pdf', key, 'Salary & Payroll Report PDF downloaded'),
      error: () => this.handleError(key, 'Salary Report PDF')
    });
  }
}
