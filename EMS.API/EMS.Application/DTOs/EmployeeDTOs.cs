using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Application.DTOs
{
    // Used when creating or updating an employee
    public class EmployeeRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int? DepartmentId { get; set; }
        public string? Designation { get; set; }
        public decimal? Salary { get; set; }
        public DateTime? JoiningDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // Used when returning employee data to the frontend
    public class EmployeeResponseDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? Designation { get; set; }
        public decimal? Salary { get; set; }
        public DateTime? JoiningDate { get; set; }
        public bool IsActive { get; set; }
    }

    // For bulk creation (multiple employees at once)
    public class BulkEmployeeRequestDto
    {
        public List<EmployeeRequestDto> Employees { get; set; } = new();
    }
}
