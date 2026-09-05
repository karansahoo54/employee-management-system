using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Domain.Entities
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int? DepartmentId { get; set; }
        public string? Designation { get; set; }
        public decimal? Salary { get; set; }
        public DateTime? JoiningDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Department? Department { get; set; }
        public ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
    }
}
