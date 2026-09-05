using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace EMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires a valid JWT token for ALL endpoints in this controller
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _employeeService.GetAllAsync();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null)
                return NotFound(new { message = $"Employee with ID {id} not found." });

            return Ok(employee);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployeeRequestDto request)
        {
            var employee = await _employeeService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = employee.EmployeeId }, employee);
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] BulkEmployeeRequestDto request)
        {
            var employees = await _employeeService.CreateBulkAsync(request.Employees);
            return Ok(employees);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeRequestDto request)
        {
            var updated = await _employeeService.UpdateAsync(id, request);
            if (updated == null)
                return NotFound(new { message = $"Employee with ID {id} not found." });

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _employeeService.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = $"Employee with ID {id} not found." });

            return NoContent();
        }

        [HttpDelete("bulk")]
        public async Task<IActionResult> DeleteBulk([FromBody] List<int> ids)
        {
            var success = await _employeeService.DeleteBulkAsync(ids);
            if (!success)
                return NotFound(new { message = "No matching employees found." });

            return NoContent();
        }
    }
}
