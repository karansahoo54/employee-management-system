using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;


namespace EMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("employees/pdf")]
        public async Task<IActionResult> GetEmployeeDirectoryPdf()
        {
            var pdfBytes = await _reportService.GenerateEmployeeDirectoryPdfAsync();
            return File(pdfBytes, "application/pdf", "EmployeeDirectory.pdf");
        }

        [HttpGet("employees/excel")]
        public async Task<IActionResult> GetEmployeeDirectoryExcel()
        {
            var excelBytes = await _reportService.GenerateEmployeeDirectoryExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "EmployeeDirectory.xlsx");
        }
        [HttpGet("departments/excel")]
        public async Task<IActionResult> GetDepartmentReportExcel()
        {
            var bytes = await _reportService.GenerateDepartmentReportExcelAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DepartmentReport.xlsx");
        }

        [HttpGet("attendance/excel")]
        public async Task<IActionResult> GetAttendanceReportExcel([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var bytes = await _reportService.GenerateAttendanceReportExcelAsync(fromDate, toDate);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AttendanceReport.xlsx");
        }

        [HttpGet("salary/pdf")]
        public async Task<IActionResult> GetSalaryReportPdf()
        {
            var bytes = await _reportService.GenerateSalaryReportPdfAsync();
            return File(bytes, "application/pdf", "SalaryReport.pdf");
        }
    }
}