using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Application.Interfaces
{
    public interface IReportService
    {
        Task<byte[]> GenerateEmployeeDirectoryPdfAsync();
        Task<byte[]> GenerateEmployeeDirectoryExcelAsync();
        Task<byte[]> GenerateDepartmentReportExcelAsync();
        Task<byte[]> GenerateAttendanceReportExcelAsync(DateTime? fromDate, DateTime? toDate);
        Task<byte[]> GenerateSalaryReportPdfAsync();
    }
}