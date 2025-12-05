namespace Employees.Controllers.MVC;

using Microsoft.AspNetCore.Mvc;
using Employees.Models;
using Employees.ViewModels;
using Employees.Repositories;

public class EmployeesController : Controller
{
    private readonly IEmployeeRepository _repository;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IEmployeeRepository repository, ILogger<EmployeesController> logger)
    {
        _repository = repository;
        _logger = logger;
    }


    // Get: /Employees

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, string? department, bool includeInactive = false)

    {
        try
        {
            IEnumerable<Employee> employees;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                employees = await _repository.SearchAsync(searchTerm);
            }
            else if (!string.IsNullOrWhiteSpace(department))
            {
                employees = await _repository.GetByDepartmentAsync(department);
            }
            else
            {
                employees = await _repository.GetAllAsync(includeInactive);
            }

            var viewModel = new EmployeeListViewModel
            {
                Employees = employees.Select(MapToViewModel),
                SearchTerm = searchTerm,
                Department = department,
                IncludeInactive = includeInactive,
                TotalCount = employees.Count()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching employees");
            TempData["ErrorMessage"] = "An error occured while fetching employees";
            return View(new EmployeeListViewModel());
        }
    }

    // Get: /Employees/Details/{id}
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var employee = await _repository.GetByIdAsync(id);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee not found";
            return RedirectToAction(nameof(Index));
        }
        return View(MapToViewModel(employee));
    }

    // Get: /Employees/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new EmployeeViewModel { HireDate = DateTime.Now, IsActive = true });
    }

    // Post: /Employees/Create

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Check if we enter the duplicate email
            if (await _repository.EmailExistsAsync(model.Email))
            {
                ModelState.AddModelError("Email", "Email already exist");
                return View(model);
            }

            var employee = MapToEntity(model);
            await _repository.CreateAsync(employee);
            TempData["SuccessMessage"] = $"Employee {employee.FirstName} {employee.LastName} has successfully created.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating empliyee");
            ModelState.AddModelError("", "An error occured while creating an employee.");
            return View(model);
        }
    }

    //Get /Employees/Edit/{id}

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _repository.GetByIdAsync(id);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee not found";
            return RedirectToAction(nameof(Index));
        }
        return View(MapToViewModel(employee));
    }

    // POST: /Employees/Edit/5

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmployeeViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var existingEmployee = await _repository.GetByIdAsync(id);
            if (existingEmployee == null)
            {
                TempData["ErrorMessage"] = "Employee not found";
                return RedirectToAction(nameof(Index));
            }

            // Check for duplication of email

            if (await _repository.EmailExistsAsync(model.Email, id))
            {
                ModelState.AddModelError("Email", "An Employee with this email is already exist");
                return View(model);
            }

            existingEmployee.FirstName = model.FirstName;
            existingEmployee.LastName = model.LastName;
            existingEmployee.Email = model.Email;
            existingEmployee.Phone = model.Phone;
            existingEmployee.Department = model.Department;
            existingEmployee.Position = model.Position;
            existingEmployee.Salary = model.Salary;
            existingEmployee.HireDate = model.HireDate;
            existingEmployee.IsActive = model.IsActive;

            await _repository.UpdateAsync(existingEmployee);
            TempData["SuccessMessage"] = $"Employee {existingEmployee.FirstName} {existingEmployee.LastName} updated successfully";
            return RedirectToAction(nameof(Index));
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {id}", id);
            ModelState.AddModelError("", "An Erro occured while updating the employee");
            return View(model);
        }
    }

    // Get /Employess/Delete/{id}
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _repository.GetByIdAsync(id);
        
        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee not found.";
            return RedirectToAction(nameof(Index));
        }
        
        return View(MapToViewModel(employee));
    }

    //Post: /Employees/Delete/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var employee = await _repository.GetByIdAsync(id);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            var fullName = $"{employee.FirstName} {employee.LastName}";
            await _repository.DeleteAsync(id);

            TempData["SuccessMessage"] = $"Employee {fullName} deleted successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee {id}", id);
            TempData["ErrorMessage"] = "An error occured while deleteing and employee";
            return RedirectToAction(nameof(Index));
        }
    }

    private static Employee MapToEntity(EmployeeViewModel model)
    {
        return new Employee
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Phone = model.Phone,
            Department = model.Department,
            Position = model.Position,
            Salary = model.Salary,
            HireDate = model.HireDate,
            IsActive = model.IsActive
        };
    }
    private static EmployeeViewModel MapToViewModel(Employee employee)
    {
        return new EmployeeViewModel
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            Department = employee.Department,
            Position = employee.Position,
            Salary = employee.Salary,
            HireDate = employee.HireDate,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt ?? DateTime.MinValue

        };
    }
}