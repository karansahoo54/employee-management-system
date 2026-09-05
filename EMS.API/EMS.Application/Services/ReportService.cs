using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using EMS.Application.Interfaces;
using EMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EMS.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly EmsDbContext _context;

        public ReportService(EmsDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateEmployeeDirectoryPdfAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Employee Directory Report")
                        .FontSize(18).Bold();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Name
                            columns.RelativeColumn(2); // Email
                            columns.RelativeColumn(1.5f); // Phone
                            columns.RelativeColumn(1.5f); // Department
                            columns.RelativeColumn(1.5f); // Designation
                            columns.RelativeColumn(1); // Salary
                            columns.RelativeColumn(1); // Status
                        });

                        // Header row
                        table.Header(header =>
                        {
                            string[] headers = { "Name", "Email", "Phone", "Department", "Designation", "Salary", "Status" };
                            foreach (var h in headers)
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                                    .Text(h).Bold();
                            }
                        });

                        // Data rows
                        foreach (var emp in employees)
                        {
                            table.Cell().Padding(5).Text(emp.FullName);
                            table.Cell().Padding(5).Text(emp.Email);
                            table.Cell().Padding(5).Text(emp.Phone ?? "-");
                            table.Cell().Padding(5).Text(emp.Department?.Name ?? "-");
                            table.Cell().Padding(5).Text(emp.Designation ?? "-");
                            table.Cell().Padding(5).Text(emp.Salary?.ToString("N2") ?? "-");
                            table.Cell().Padding(5).Text(emp.IsActive ? "Active" : "Inactive");
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated on ");
                            x.Span(DateTime.Now.ToString("dd-MMM-yyyy HH:mm"));
                        });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateEmployeeDirectoryExcelAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Employees");

            // Header row
            string[] headers = { "ID", "Name", "Email", "Phone", "Department", "Designation", "Salary", "Joining Date", "Status" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            // Data rows
            int row = 2;
            foreach (var emp in employees)
            {
                worksheet.Cell(row, 1).Value = emp.EmployeeId;
                worksheet.Cell(row, 2).Value = emp.FullName;
                worksheet.Cell(row, 3).Value = emp.Email;
                worksheet.Cell(row, 4).Value = emp.Phone ?? "-";
                worksheet.Cell(row, 5).Value = emp.Department?.Name ?? "-";
                worksheet.Cell(row, 6).Value = emp.Designation ?? "-";
                worksheet.Cell(row, 7).Value = emp.Salary ?? 0;
                worksheet.Cell(row, 8).Value = emp.JoiningDate?.ToString("yyyy-MM-dd") ?? "-";
                worksheet.Cell(row, 9).Value = emp.IsActive ? "Active" : "Inactive";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

    public async Task<byte[]> GenerateDepartmentReportExcelAsync()
        {
            var departments = await _context.Departments
                .Include(d => d.Employees)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Departments");

            string[] headers = { "Department", "Employee Count", "Active Employees" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            int row = 2;
            foreach (var dept in departments)
            {
                worksheet.Cell(row, 1).Value = dept.Name;
                worksheet.Cell(row, 2).Value = dept.Employees.Count;
                worksheet.Cell(row, 3).Value = dept.Employees.Count(e => e.IsActive);
                row++;
            }

            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> GenerateAttendanceReportExcelAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Attendances.Include(a => a.Employee).AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.Date >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(a => a.Date <= toDate.Value);

            var records = await query.OrderBy(a => a.Date).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Attendance");

            string[] headers = { "Employee", "Date", "Status" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            int row = 2;
            foreach (var rec in records)
            {
                worksheet.Cell(row, 1).Value = rec.Employee?.FullName ?? "-";
                worksheet.Cell(row, 2).Value = rec.Date.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 3).Value = rec.Status;
                row++;
            }

            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> GenerateSalaryReportPdfAsync()
        {
            var employees = await _context.Employees.Include(e => e.Department).Where(e => e.IsActive).ToListAsync();
            decimal totalSalary = employees.Sum(e => e.Salary ?? 0);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Text("Salary Report").FontSize(18).Bold();

                    page.Content().Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Name").Bold();
                                h.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Department").Bold();
                                h.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Salary").Bold();
                            });

                            foreach (var emp in employees)
                            {
                                table.Cell().Padding(5).Text(emp.FullName);
                                table.Cell().Padding(5).Text(emp.Department?.Name ?? "-");
                                table.Cell().Padding(5).Text(emp.Salary?.ToString("N2") ?? "0.00");
                            }
                        });

                        col.Item().PaddingTop(15).Text($"Total Monthly Salary Outlay: {totalSalary:N2}").Bold().FontSize(12);
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated on ");
                        x.Span(DateTime.Now.ToString("dd-MMM-yyyy HH:mm"));
                    });
                });
            });

            return document.GeneratePdf();
        }
    }

}