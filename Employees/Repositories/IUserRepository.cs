namespace Employees.Repositories;

using Employees.Models;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> UsernameExistsAsync(string username, int? excludeId = null);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);

}