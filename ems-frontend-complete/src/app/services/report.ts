import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private apiUrl = `${environment.apiUrl}/Report`;

  constructor(private http: HttpClient) {}

  downloadEmployeePdf(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/employees/pdf`, { responseType: 'blob' });
  }

  downloadEmployeeExcel(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/employees/excel`, { responseType: 'blob' });
  }

  downloadDepartmentExcel(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/departments/excel`, { responseType: 'blob' });
  }

  downloadAttendanceExcel(fromDate?: string, toDate?: string): Observable<Blob> {
    let params = '';
    if (fromDate) params += `fromDate=${fromDate}`;
    if (toDate) params += `${params ? '&' : ''}toDate=${toDate}`;
    return this.http.get(`${this.apiUrl}/attendance/excel${params ? '?' + params : ''}`, { responseType: 'blob' });
  }

  downloadSalaryPdf(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/salary/pdf`, { responseType: 'blob' });
  }
}
