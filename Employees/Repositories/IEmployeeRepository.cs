namespace Employees.Repositories;
using Employees.Models;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync(bool includeInactive = false);
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByEmailAsync(string email);

    Task<Employee> CreateAsync(Employee employee);
    Task<Employee> UpdateAsync(Employee employee);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<Employee>> SearchAsync(string searchTerm);
    Task<IEnumerable<Employee>> GetByDepartmentAsync(string department);


}