namespace Employees.Controllers.MVC;

using Microsoft.AspNetCore.Mvc;
using Employees.Repositories;
using Employees.ViewModels;

[Route("dashboard")]
public class DashboardController : Controller
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DashboardController(
        IEmployeeRepository employeeRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _employeeRepository = employeeRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        var sesssionKey = "DashboardViews";
        var currentViews = HttpContext.Session.GetInt32(sesssionKey) ?? 0;
        HttpContext.Session.SetInt32(sesssionKey, currentViews + 1);
        ViewBag.PageViews = currentViews + 1 ;

        var appState = _httpContextAccessor.HttpContext?.RequestServices.GetService<ApplicationStatesService>();
        ViewBag.TotalAppRequests = appState?.TotalRequests ?? 0;

        var employees = await _employeeRepository.GetAllAsync();
        var activeEmployees = employees.Where(e => e.IsActive).ToList();

        var viewModels = new DashboardViewModel
        {
            TotalEmployees = employees.Count(),
            ActiveEmployees = activeEmployees.Count,
            InactiveEmployees = employees.Count(e => !e.IsActive),
            AverageSalary = activeEmployees.Any() ? activeEmployees.Average(e=> e.Salary) : 0,
            TotalSalaryExpense = activeEmployees.Sum(e => e.Salary),
            EmployeesByDepartment = activeEmployees
                .GroupBy(e => e.Department)
                .ToDictionary(g => g.Key, g => g.Count()),
            RecentHires = activeEmployees
                .OrderByDescending(e => e.HireDate)
                .Take(5)
                .ToList(),
            HighestPaidEmployees = activeEmployees
                .OrderByDescending(e => e.Salary)
                .Take(5)
                .ToList()
        };
        return View(viewModels);
    }
    [HttpGet("stats")]
    public async Task<IActionResult> Statistics(string department = "")
    {
        var employees = await _employeeRepository.GetAllAsync();
        var filtered = string.IsNullOrEmpty(department) ? employees : employees.Where(e => e.Department == department);

        var stats = new
        {
            Count = filtered.Count(),
            AverageSalary = filtered.Any() ? filtered.Average(e => e.Salary):0,
            MedianSalary = CalculateMedian(filtered.Select(e => e.Salary).ToList()),
            MinSalary = filtered.Any() ? filtered.Min(e=> e.Salary) : 0,
            MaxSalary = filtered.Any() ? filtered.Max(e => e.Salary) : 0
        };

        return Json(stats);
    }

    [HttpGet("reset-session")]
    public IActionResult ResetSesssion()
    {
        HttpContext.Session.Clear();
        TempData["InfoMessage"] = "Session data has been cleared.";
        return RedirectToAction(nameof(Index));
    }

    private decimal CalculateMedian(List<decimal> values)
    {
        if (!values.Any()) return 0;

        var sorted = values.OrderBy( v => v).ToList();
        int count = sorted.Count;

        if (count %2 == 0)
        {
            return (sorted[count / 2-1] + sorted[count/2]) /2 ;
        }

        return sorted[count / 2];
    }
}

public class ApplicationStatesService
{
    private int _totalRequests = 0;
    private readonly object _lock = new object();
    public int TotalRequests
    {
        get
        {
            lock (_lock)
            {
                return _totalRequests;
            }
        }
    }
    public void IncrementRequests()
    {
        lock (_lock)
        {
            _totalRequests ++;
        }
    }
}