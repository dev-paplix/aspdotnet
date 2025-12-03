namespace Employees.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Employees.Models;
using Employees.DTOs;
using Employees.Data;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController: ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(AppDbContext context, ILogger<EmployeesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Get: api/employees

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeResponseDto>>>> GetAllEmployees(
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            var query = _context.Employees.AsQueryable();

            if(!includeInactive)
            {
                query = query.Where(e => e.IsActive);
            }

            var employees = await query
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();
            
            var employeeDtos = employees.Select(e => MapToDto(e)).ToList();

            return Ok(new ApiResponse<IEnumerable<EmployeeResponseDto>>
            {
                Success = true,
                Message = "Employees fetched successfully",
                Data = employeeDtos
                
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching employees");
            return StatusCode(500, new ApiResponse<IEnumerable<EmployeeResponseDto>>
            {
                Success = false,
                Message = "An error occured while fetching employees"
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<EmployeeResponseDto>>> GetEmployee(int id)
    {
        try
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound(new ApiResponse<EmployeeResponseDto>
                {
                    Success = false,
                    Message = "Employee not found"
                });
            }

            return Ok(new ApiResponse<EmployeeResponseDto>
            {
                Success = true,
                Message = "Employee fetched successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching employee {id}", id);
            return StatusCode(500, new ApiResponse<EmployeeResponseDto>
            {
                Success = false,
                Message = "An error occured while fetching employee"
            });
        }
    }

    // Post: api/employees
    [HttpPost]
    public async Task<ActionResult<ApiResponse<EmployeeResponseDto>>> CreateEmployee(
        [FromBody] EmployeeCreateDto dto)
    {
        try
        {
            if (await _context.Employees.AnyAsync(e => e.Email == dto.Email))
            {
                return BadRequest(new ApiResponse<EmployeeResponseDto>
                {
                    Success = false,
                    Message = "An employee with the same email already exists"
                });
            }
            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                Department = dto.Department,
                Position = dto.Position,
                Salary = dto.Salary,
                Email = dto.Email,
                HireDate = dto.HireDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, new ApiResponse<EmployeeResponseDto>
            {
                Success = true,
                Message = "Employee created successfully",
                Data = MapToDto(employee)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return StatusCode(500, new ApiResponse<EmployeeResponseDto>
            {
                Success = false,
                Message = "An error occured while creating employee"
            });
        }
    } 

    // PUT : api/employees/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<EmployeeResponseDto>>> UpdateEmployee(
        int id, [FromBody]EmployeeUpdateDto dto)
    {
        try
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound(new ApiResponse<EmployeeResponseDto>
                {
                    Success = false,
                    Message = "Employee not found"
                });
            }

            if (employee.Email != dto.Email &&
                await _context.Employees.AnyAsync(e => e.Email == dto.Email))
            {
                return BadRequest(new ApiResponse<EmployeeResponseDto>
                {
                    Success = false,
                    Message = "An employee with the same email already exists"
                });
            }

            if(dto.FirstName != null) employee.FirstName = dto.FirstName;
            if(dto.LastName != null) employee.LastName = dto.LastName;
            if(dto.Phone != null) employee.Phone = dto.Phone;
            if(dto.Department != null) employee.Department = dto.Department;
            if(dto.Position != null) employee.Position = dto.Position;
            if(dto.Salary.HasValue) employee.Salary = dto.Salary.Value;
            if(dto.HireDate != default) employee.HireDate = dto.HireDate;
            if(dto.Email != null) employee.Email = dto.Email;
            employee.IsActive = dto.IsActive;
            employee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<EmployeeResponseDto>
            {
                Success = true,
                Message = "Employee updated successfully",
                Data = MapToDto(employee)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {id}", id);
            return StatusCode(500, new ApiResponse<EmployeeResponseDto>
            {
                Success = false,
                Message = "An error occured while updating employee"
            });
        }
    }

    //Delete api/employees/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEmployee(int id)
    {
        try
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Employee not found"
                });
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Employee deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee {id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occured while deleting employee"
            });
        }
    }

    private static EmployeeResponseDto MapToDto(Employee employee)
    {
        return new EmployeeResponseDto
        {
            Id =  employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Phone = employee.Phone,
            Department = employee.Department,
            Position = employee.Position,
            Salary = employee.Salary,
            Email = employee.Email,
            HireDate = employee.HireDate,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt
        };
    }

}
