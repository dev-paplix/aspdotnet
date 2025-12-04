namespace Employees.ViewModels;

using Employees.Models;

public class EmployeeFilterViewModel
{
    public string? SearchTerm {get; set;}
    public string? Department {get; set; }
    public bool IncludeInactive {get; set; }
    public decimal? MinSalary {get; set; }
    public decimal? MaxSalary {get; set; }
    public DateTime? HireDateFrom {get; set; }
    public DateTime? HireDateTo {get; set; }

    public int PageNumber {get; set; } = 1;
    public int PageSize {get; set; } = 10;
    public string SortBy {get; set; } = "LastName";
    public string SortOrder {get; set; } = "asc";
}

public class PaginatedViewModel<T>
{
    public List<T> Items {get; set; } = new();
    public int TotalCount {get; set; }
    public int PageNumber {get; set; }
    public int PageSize {get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public class DashboardViewModel
{
    public int TotalEmployees {get; set; }
    public int ActiveEmployees {get; set; }
    public int InactiveEmployees {get; set; }
    public decimal AverageSalary {get; set; }
    public decimal TotalSalaryExpense {get; set; }
    public Dictionary<string, int> EmployeesByDepartment {get; set; } = new();
    public List<Employee> RecentHires {get; set; } = new();
    public List<Employee> HighestPaidEmployees {get; set; } = new();
}

public class SearchResultViewModel
{
    public List<EmployeeViewModel> Results {get; set; } = new();
    public string SearchTerm {get; set; } = string.Empty;
    public int ResultCount {get; set; }
    public long SearchDurationMs {get; set; }
    public Dictionary<string, string> FacetsByDepartment {get; set; } = new();
}