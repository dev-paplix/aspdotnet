namespace Employees.ViewModels;

public class EmployeeListViewModel
{
    public IEnumerable<EmployeeViewModel> Employees { get; set; } = new List<EmployeeViewModel>();
    public string? SearchTerm { get; set; }
    public string? Department { get; set; }
    public bool IncludeInactive { get; set; }
    public int TotalCount { get; set; }
}