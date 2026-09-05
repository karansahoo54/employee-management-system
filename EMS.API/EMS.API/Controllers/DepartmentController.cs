using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EMS.Application.DTOs;
using EMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EMS.Domain.Entities;

namespace EMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly EmsDbContext _context;

        public DepartmentController(EmsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _context.Departments
                .Select(d => new DepartmentDto { DepartmentId = d.DepartmentId, Name = d.Name })
                .ToListAsync();

            return Ok(departments);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DepartmentRequestDto request)
        {
            var department = new Department { Name = request.Name };
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return Ok(new DepartmentDto { DepartmentId = department.DepartmentId, Name = department.Name });
        }
    }
}