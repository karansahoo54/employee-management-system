import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EmployeeService, EmployeeRequest } from '../../services/employee';
import { DepartmentService, Department } from '../../services/department';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-employee-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './employee-form.html',
  styleUrl: './employee-form.css'
})
export class EmployeeFormComponent implements OnInit {
  employee: EmployeeRequest = {
    fullName: '',
    email: '',
    phone: '',
    departmentId: undefined,
    designation: '',
    salary: undefined,
    joiningDate: '',
    isActive: true
  };

  departments: Department[] = [];
  isEditMode = false;
  employeeId: number | null = null;
  errorMessage = '';
  isLoading = false;
  submitted = false;

  // Inline Quick-Add Department
  showAddDept = false;
  newDeptName = '';
  isAddingDept = false;

  constructor(
    private employeeService: EmployeeService,
    private departmentService: DepartmentService,
    private route: ActivatedRoute,
    private router: Router,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadDepartments();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode = true;
      this.employeeId = +idParam;
      this.loadEmployee(this.employeeId);
    } else {
      // Default joining date to today in YYYY-MM-DD
      this.employee.joiningDate = new Date().toISOString().substring(0, 10);
    }
  }

  loadDepartments(): void {
    this.departmentService.getAll().subscribe({
      next: (data) => {
        this.departments = data;
        // If there are no departments at all, seed a default one
        if (this.departments.length === 0) {
          this.quickSeedDepartment();
        }
      },
      error: () => this.toastService.error('Failed to load departments')
    });
  }

  private quickSeedDepartment(): void {
    this.departmentService.create('Engineering').subscribe({
      next: (dept) => {
        this.departments.push(dept);
        this.employee.departmentId = dept.departmentId;
      }
    });
  }

  createDepartmentInline(): void {
    const trimmed = this.newDeptName.trim();
    if (!trimmed) return;

    this.isAddingDept = true;
    this.departmentService.create(trimmed).subscribe({
      next: (dept) => {
        this.departments.push(dept);
        this.employee.departmentId = dept.departmentId;
        this.newDeptName = '';
        this.showAddDept = false;
        this.isAddingDept = false;
        this.toastService.success(`Department "${dept.name}" added`);
      },
      error: () => {
        this.isAddingDept = false;
        this.toastService.error('Failed to add department');
      }
    });
  }

  loadEmployee(id: number): void {
    this.isLoading = true;
    this.employeeService.getById(id).subscribe({
      next: (data) => {
        this.employee = {
          fullName: data.fullName,
          email: data.email,
          phone: data.phone || '',
          departmentId: data.departmentId,
          designation: data.designation || '',
          salary: data.salary,
          joiningDate: data.joiningDate ? data.joiningDate.substring(0, 10) : '',
          isActive: data.isActive
        };
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load employee details.';
        this.isLoading = false;
        this.toastService.error('Employee not found');
      }
    });
  }

  isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = '';

    // Validation checks
    if (!this.employee.fullName || this.employee.fullName.trim().length < 2) {
      this.errorMessage = 'Please provide a valid full name (at least 2 characters).';
      return;
    }

    if (!this.employee.email || !this.isValidEmail(this.employee.email)) {
      this.errorMessage = 'Please provide a valid email address.';
      return;
    }

    if (this.employee.salary !== undefined && this.employee.salary !== null && this.employee.salary < 0) {
      this.errorMessage = 'Salary cannot be negative.';
      return;
    }

    this.isLoading = true;

    // Clean payload
    const payload: EmployeeRequest = {
      fullName: this.employee.fullName.trim(),
      email: this.employee.email.trim(),
      phone: this.employee.phone?.trim() || undefined,
      departmentId: this.employee.departmentId || undefined,
      designation: this.employee.designation?.trim() || undefined,
      salary: this.employee.salary ? Number(this.employee.salary) : undefined,
      joiningDate: this.employee.joiningDate || undefined,
      isActive: this.employee.isActive
    };

    const request$ = this.isEditMode && this.employeeId
      ? this.employeeService.update(this.employeeId, payload)
      : this.employeeService.create(payload);

    request$.subscribe({
      next: () => {
        this.isLoading = false;
        this.toastService.success(
          this.isEditMode ? 'Employee updated successfully!' : 'Employee created successfully!'
        );
        this.router.navigate(['/employees']);
      },
      error: (err: any) => {
        this.isLoading = false;
        const msg = err.error?.message || err.error?.title || 'Failed to save employee. Check your inputs.';
        this.errorMessage = msg;
        this.toastService.error(msg);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/employees']);
  }
}
