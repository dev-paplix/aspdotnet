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

}
