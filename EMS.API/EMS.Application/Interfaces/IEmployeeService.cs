using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMS.Application.DTOs;

namespace EMS.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<EmployeeResponseDto>> GetAllAsync();
        Task<EmployeeResponseDto?> GetByIdAsync(int id);
        Task<EmployeeResponseDto> CreateAsync(EmployeeRequestDto request);
        Task<List<EmployeeResponseDto>> CreateBulkAsync(List<EmployeeRequestDto> requests);
        Task<EmployeeResponseDto?> UpdateAsync(int id, EmployeeRequestDto request);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteBulkAsync(List<int> ids);
    }
}