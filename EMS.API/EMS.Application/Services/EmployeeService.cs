using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;
using EMS.Domain.Entities;
using EMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMS.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly EmsDbContext _context;

        public EmployeeService(EmsDbContext context)
        {
            _context = context;
        }

        public async Task<List<EmployeeResponseDto>> GetAllAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .OrderBy(e => e.EmployeeId)
                .ToListAsync();

            return employees.Select(MapToDto).ToList();
        }

        public async Task<EmployeeResponseDto?> GetByIdAsync(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            return employee == null ? null : MapToDto(employee);
        }

        public async Task<EmployeeResponseDto> CreateAsync(EmployeeRequestDto request)
        {
            var employee = new Employee
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                DepartmentId = request.DepartmentId,
                Designation = request.Designation,
                Salary = request.Salary,
                JoiningDate = request.JoiningDate.HasValue ? DateTime.SpecifyKind(request.JoiningDate.Value, DateTimeKind.Utc) : null,
                IsActive = request.IsActive
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Reload with Department included so the response has DepartmentName
            if (employee.DepartmentId.HasValue)
            {
                await _context.Entry(employee).Reference(e => e.Department).LoadAsync();
            }

            return MapToDto(employee);
        }

        public async Task<List<EmployeeResponseDto>> CreateBulkAsync(List<EmployeeRequestDto> requests)
        {
            var employees = requests.Select(request => new Employee
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                DepartmentId = request.DepartmentId,
                Designation = request.Designation,
                Salary = request.Salary,
                JoiningDate = request.JoiningDate.HasValue ? DateTime.SpecifyKind(request.JoiningDate.Value, DateTimeKind.Utc) : null,
                IsActive = request.IsActive
            }).ToList();

            _context.Employees.AddRange(employees);
            await _context.SaveChangesAsync();

            foreach (var emp in employees)
            {
                if (emp.DepartmentId.HasValue)
                {
                    await _context.Entry(emp).Reference(e => e.Department).LoadAsync();
                }
            }

            return employees.Select(e => MapToDto(e)).ToList();
        }

        public async Task<EmployeeResponseDto?> UpdateAsync(int id, EmployeeRequestDto request)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return null;

            employee.FullName = request.FullName;
            employee.Email = request.Email;
            employee.Phone = request.Phone;
            employee.DepartmentId = request.DepartmentId;
            employee.Designation = request.Designation;
            employee.Salary = request.Salary;
            employee.JoiningDate = request.JoiningDate.HasValue ? DateTime.SpecifyKind(request.JoiningDate.Value, DateTimeKind.Utc) : null;
            employee.IsActive = request.IsActive;

            await _context.SaveChangesAsync();
            if (employee.DepartmentId.HasValue)
            {
                await _context.Entry(employee).Reference(e => e.Department).LoadAsync();
            }
            else
            {
                employee.Department = null;
            }

            return MapToDto(employee);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return false;

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBulkAsync(List<int> ids)
        {
            var employees = await _context.Employees
                .Where(e => ids.Contains(e.EmployeeId))
                .ToListAsync();

            if (employees.Count == 0)
                return false;

            _context.Employees.RemoveRange(employees);
            await _context.SaveChangesAsync();
            return true;
        }

        // Helper method: converts Employee entity -> EmployeeResponseDto
        // This is the ONE place mapping logic lives - no repeated hardcoded mapping elsewhere
        private static EmployeeResponseDto MapToDto(Employee e)
        {
            return new EmployeeResponseDto
            {
                EmployeeId = e.EmployeeId,
                FullName = e.FullName,
                Email = e.Email,
                Phone = e.Phone,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department?.Name,
                Designation = e.Designation,
                Salary = e.Salary,
                JoiningDate = e.JoiningDate,
                IsActive = e.IsActive
            };
        }
    }
}
