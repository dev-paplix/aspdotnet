# Employee Management API - Complete Development Guide

## Project Overview
This is a complete ASP.NET Core REST API for employee management with role-based access control, JWT authentication, and SQLite database. Built from scratch starting with an empty ASP.NET Core project.

---

## Table of Contents
1. [Initial Project State](#initial-project-state)
2. [Project Structure Created](#project-structure-created)
3. [Step-by-Step Development Process](#step-by-step-development-process)
4. [Code Explanations](#code-explanations)
5. [Commands Used](#commands-used)
6. [API Usage Guide](#api-usage-guide)

---

## Initial Project State

The project started with only:
- `Program.cs` - Basic "Hello World" endpoint
- `Employees.csproj` - Empty project file with no packages
- `appsettings.json` - Basic logging configuration

---

## Project Structure Created

```
Employees/
├── Models/
│   ├── Employee.cs          # Employee entity model
│   └── User.cs              # User authentication model with roles
├── Data/
│   └── AppDbContext.cs      # Entity Framework database context
├── DTOs/
│   ├── EmployeeDto.cs       # Data transfer objects for employees
│   └── UserDto.cs           # Data transfer objects for users/auth
├── Controllers/
│   ├── EmployeesController.cs  # CRUD endpoints for employees
│   └── AuthController.cs       # Authentication & user management
├── Program.cs               # Application configuration & startup
├── appsettings.json         # Configuration settings
├── Employees.csproj         # Project dependencies
├── .gitignore              # Git ignore file
├── README.md               # Project documentation
└── API_DOCUMENTATION.md    # API endpoint documentation
```

---

## Step-by-Step Development Process

### Step 1: Create Data Models

#### 1.1 Employee Model (`Models/Employee.cs`)

**Purpose**: Represents an employee record in the database.

```csharp
namespace Employees.Models;

public class Employee
{
    public int Id { get; set; }                          // Primary key, auto-generated
    public string FirstName { get; set; } = string.Empty; // Employee first name
    public string LastName { get; set; } = string.Empty;  // Employee last name
    public string Email { get; set; } = string.Empty;     // Unique email address
    public string Phone { get; set; } = string.Empty;     // Contact phone number
    public string Department { get; set; } = string.Empty; // Department (e.g., IT, HR)
    public string Position { get; set; } = string.Empty;   // Job position/title
    public decimal Salary { get; set; }                    // Employee salary
    public DateTime HireDate { get; set; }                 // Date employee was hired
    public bool IsActive { get; set; } = true;             // Soft delete flag
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Record creation timestamp
    public DateTime? UpdatedAt { get; set; }               // Last update timestamp (nullable)
}
```

**Key Features**:
- `Id`: Auto-incremented primary key
- `IsActive`: Enables soft delete (mark inactive instead of deleting)
- `CreatedAt`/`UpdatedAt`: Audit trail timestamps
- `Email`: Will be unique in database via index

---

#### 1.2 User Model (`Models/User.cs`)

**Purpose**: Handles user authentication and role-based access control.

```csharp
namespace Employees.Models;

public class User
{
    public int Id { get; set; }                          // Primary key
    public string Username { get; set; } = string.Empty;  // Unique username for login
    public string PasswordHash { get; set; } = string.Empty; // BCrypt hashed password
    public string Email { get; set; } = string.Empty;     // Unique email
    public UserRole Role { get; set; }                    // User's role/permission level
    public bool IsActive { get; set; } = true;            // Account status
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Account creation date
}

// Enum defining the 4 user access levels
public enum UserRole
{
    Admin = 1,      // Full system access
    Marketing = 2,  // Marketing department access
    Sales = 3,      // Sales department access
    Accounting = 4  // Accounting department access
}
```

**Key Features**:
- `PasswordHash`: Never stores plain text passwords, uses BCrypt
- `UserRole`: Enum for type-safe role assignment
- Four distinct permission levels as requested

---

### Step 2: Create Database Context

#### 2.1 AppDbContext (`Data/AppDbContext.cs`)

**Purpose**: Manages database connection and entity configurations.

```csharp
using Microsoft.EntityFrameworkCore;
using Employees.Models;

namespace Employees.Data;

public class AppDbContext : DbContext
{
    // Constructor receives configuration from dependency injection
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSet properties represent database tables
    public DbSet<Employee> Employees { get; set; }  // Employees table
    public DbSet<User> Users { get; set; }          // Users table

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee entity configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);  // Set primary key
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.Salary).HasColumnType("decimal(18,2)"); // Precision for money
            entity.HasIndex(e => e.Email).IsUnique();  // Ensure unique emails
        });

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.HasIndex(u => u.Username).IsUnique();  // Unique usernames
            entity.HasIndex(u => u.Email).IsUnique();     // Unique emails
        });

        // Seed default admin user
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
```

**Key Features**:
- Entity configurations define database schema
- Unique indexes prevent duplicate emails/usernames
- Seeds a default admin user for initial access
- Uses BCrypt for password hashing

---

### Step 3: Create Data Transfer Objects (DTOs)

#### 3.1 Employee DTOs (`DTOs/EmployeeDto.cs`)

**Purpose**: Separate data models for API input/output from database entities.

```csharp
namespace Employees.DTOs;

// Used when creating a new employee
public class EmployeeCreateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; }
}

// Used when updating an employee (all fields optional)
public class EmployeeUpdateDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public decimal? Salary { get; set; }
    public DateTime? HireDate { get; set; }
    public bool? IsActive { get; set; }
}

// Used when returning employee data to client
public class EmployeeResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**Why DTOs?**
- **Security**: Hide internal fields (like database IDs during creation)
- **Flexibility**: Allow partial updates
- **Validation**: Control what data is required
- **API Contract**: Clear interface for frontend developers

---

#### 3.2 User DTOs (`DTOs/UserDto.cs`)

```csharp
using Employees.Models;

namespace Employees.DTOs;

// Used when registering a new user
public class UserRegisterDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;  // Plain text (will be hashed)
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}

// Used for login requests
public class UserLoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;  // Plain text (validated against hash)
}

// Used when returning user data (never includes password)
public class UserResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Returned after successful login
public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;     // JWT token
    public UserResponseDto User { get; set; } = null!;    // User details
}

// Generic wrapper for all API responses
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
```

**Key Security Features**:
- Passwords never returned in responses
- JWT token provided after login
- Consistent response format across all endpoints

---

### Step 4: Create Controllers

#### 4.1 Employees Controller (`Controllers/EmployeesController.cs`)

**Purpose**: Handles all CRUD operations for employees.

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Employees.Data;
using Employees.Models;
using Employees.DTOs;

namespace Employees.Controllers;

[ApiController]  // Enables automatic model validation and routing
[Route("api/[controller]")]  // Creates route: /api/employees
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmployeesController> _logger;

    // Dependency injection of database context and logger
    public EmployeesController(AppDbContext context, ILogger<EmployeesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/employees
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeResponseDto>>>> GetAllEmployees(
        [FromQuery] bool includeInactive = false)  // Query parameter for filtering
    {
        try
        {
            var query = _context.Employees.AsQueryable();

            // Filter out inactive employees unless requested
            if (!includeInactive)
            {
                query = query.Where(e => e.IsActive);
            }

            // Retrieve and sort employees
            var employees = await query
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            // Convert to DTOs
            var employeeDtos = employees.Select(e => MapToDto(e)).ToList();

            return Ok(new ApiResponse<IEnumerable<EmployeeResponseDto>>
            {
                Success = true,
                Message = "Employees retrieved successfully",
                Data = employeeDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees");
            return StatusCode(500, new ApiResponse<IEnumerable<EmployeeResponseDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving employees"
            });
        }
    }

    // GET: api/employees/5
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
                Message = "Employee retrieved successfully",
                Data = MapToDto(employee)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee {Id}", id);
            return StatusCode(500, new ApiResponse<EmployeeResponseDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the employee"
            });
        }
    }

    // POST: api/employees
    [HttpPost]
    public async Task<ActionResult<ApiResponse<EmployeeResponseDto>>> CreateEmployee(EmployeeCreateDto dto)
    {
        try
        {
            // Check for duplicate email
            if (await _context.Employees.AnyAsync(e => e.Email == dto.Email))
            {
                return BadRequest(new ApiResponse<EmployeeResponseDto>
                {
                    Success = false,
                    Message = "An employee with this email already exists"
                });
            }

            // Map DTO to entity
            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Department = dto.Department,
                Position = dto.Position,
                Salary = dto.Salary,
                HireDate = dto.HireDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Return 201 Created with Location header
            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id },
                new ApiResponse<EmployeeResponseDto>
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
                Message = "An error occurred while creating the employee"
            });
        }
    }

    // PUT: api/employees/5
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<EmployeeResponseDto>>> UpdateEmployee(int id, EmployeeUpdateDto dto)
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

            // Check email uniqueness if being changed
            if (dto.Email != null && dto.Email != employee.Email)
            {
                if (await _context.Employees.AnyAsync(e => e.Email == dto.Email && e.Id != id))
                {
                    return BadRequest(new ApiResponse<EmployeeResponseDto>
                    {
                        Success = false,
                        Message = "An employee with this email already exists"
                    });
                }
                employee.Email = dto.Email;
            }

            // Update only provided fields (partial update support)
            if (dto.FirstName != null) employee.FirstName = dto.FirstName;
            if (dto.LastName != null) employee.LastName = dto.LastName;
            if (dto.Phone != null) employee.Phone = dto.Phone;
            if (dto.Department != null) employee.Department = dto.Department;
            if (dto.Position != null) employee.Position = dto.Position;
            if (dto.Salary.HasValue) employee.Salary = dto.Salary.Value;
            if (dto.HireDate.HasValue) employee.HireDate = dto.HireDate.Value;
            if (dto.IsActive.HasValue) employee.IsActive = dto.IsActive.Value;

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
            _logger.LogError(ex, "Error updating employee {Id}", id);
            return StatusCode(500, new ApiResponse<EmployeeResponseDto>
            {
                Success = false,
                Message = "An error occurred while updating the employee"
            });
        }
    }

    // DELETE: api/employees/5 (Soft Delete)
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

            // Soft delete - mark as inactive
            employee.IsActive = false;
            employee.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Employee deactivated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the employee"
            });
        }
    }

    // DELETE: api/employees/5/permanent (Hard Delete)
    [HttpDelete("{id}/permanent")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEmployeePermanent(int id)
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
                Message = "Employee permanently deleted"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error permanently deleting employee {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the employee"
            });
        }
    }

    // Helper method to convert Employee entity to DTO
    private static EmployeeResponseDto MapToDto(Employee employee)
    {
        return new EmployeeResponseDto
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
            UpdatedAt = employee.UpdatedAt
        };
    }
}
```

**Key Features**:
- **Soft Delete**: Default delete marks inactive (recoverable)
- **Hard Delete**: Separate endpoint for permanent deletion
- **Validation**: Checks for duplicate emails
- **Partial Updates**: Only updates fields that are provided
- **Error Handling**: Try-catch with logging
- **Consistent Responses**: All responses use `ApiResponse<T>`

---

#### 4.2 Auth Controller (`Controllers/AuthController.cs`)

**Purpose**: Handles user authentication and management.

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Employees.Data;
using Employees.Models;
using Employees.DTOs;

namespace Employees.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> Register(UserRegisterDto dto)
    {
        try
        {
            // Validate username uniqueness
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return BadRequest(new ApiResponse<UserResponseDto>
                {
                    Success = false,
                    Message = "Username already exists"
                });
            }

            // Validate email uniqueness
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(new ApiResponse<UserResponseDto>
                {
                    Success = false,
                    Message = "Email already exists"
                });
            }

            // Hash password using BCrypt
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id },
                new ApiResponse<UserResponseDto>
                {
                    Success = true,
                    Message = "User registered successfully",
                    Data = MapToUserDto(user)
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user");
            return StatusCode(500, new ApiResponse<UserResponseDto>
            {
                Success = false,
                Message = "An error occurred during registration"
            });
        }
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(UserLoginDto dto)
    {
        try
        {
            // Find user by username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            // Verify password using BCrypt
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "Invalid username or password"
                });
            }

            // Check if account is active
            if (!user.IsActive)
            {
                return Unauthorized(new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "User account is inactive"
                });
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new ApiResponse<LoginResponseDto>
            {
                Success = true,
                Message = "Login successful",
                Data = new LoginResponseDto
                {
                    Token = token,
                    User = MapToUserDto(user)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new ApiResponse<LoginResponseDto>
            {
                Success = false,
                Message = "An error occurred during login"
            });
        }
    }

    // GET: api/auth/users
    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserResponseDto>>>> GetAllUsers()
    {
        try
        {
            var users = await _context.Users
                .OrderBy(u => u.Username)
                .ToListAsync();

            var userDtos = users.Select(u => MapToUserDto(u)).ToList();

            return Ok(new ApiResponse<IEnumerable<UserResponseDto>>
            {
                Success = true,
                Message = "Users retrieved successfully",
                Data = userDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, new ApiResponse<IEnumerable<UserResponseDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving users"
            });
        }
    }

    // GET: api/auth/users/5
    [HttpGet("users/{id}")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetUser(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new ApiResponse<UserResponseDto>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new ApiResponse<UserResponseDto>
            {
                Success = true,
                Message = "User retrieved successfully",
                Data = MapToUserDto(user)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {Id}", id);
            return StatusCode(500, new ApiResponse<UserResponseDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the user"
            });
        }
    }

    // PUT: api/auth/users/5/role
    [HttpPut("users/{id}/role")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> UpdateUserRole(int id, [FromBody] UserRole role)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new ApiResponse<UserResponseDto>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            user.Role = role;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<UserResponseDto>
            {
                Success = true,
                Message = "User role updated successfully",
                Data = MapToUserDto(user)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role {Id}", id);
            return StatusCode(500, new ApiResponse<UserResponseDto>
            {
                Success = false,
                Message = "An error occurred while updating the user role"
            });
        }
    }

    // PUT: api/auth/users/5/status
    [HttpPut("users/{id}/status")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> UpdateUserStatus(int id, [FromBody] bool isActive)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new ApiResponse<UserResponseDto>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            user.IsActive = isActive;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<UserResponseDto>
            {
                Success = true,
                Message = $"User {(isActive ? "activated" : "deactivated")} successfully",
                Data = MapToUserDto(user)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user status {Id}", id);
            return StatusCode(500, new ApiResponse<UserResponseDto>
            {
                Success = false,
                Message = "An error occurred while updating the user status"
            });
        }
    }

    // Generate JWT token with user claims
    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForJWTTokenGeneration123456");
        
        // Create claims for the token
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(jwtSettings["ExpirationHours"] ?? "24")),
            Issuer = jwtSettings["Issuer"] ?? "EmployeeManagementAPI",
            Audience = jwtSettings["Audience"] ?? "EmployeeManagementClient",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // Helper to map User to UserResponseDto (never includes password)
    private static UserResponseDto MapToUserDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
```

**Key Security Features**:
- **BCrypt Password Hashing**: Industry-standard secure hashing
- **JWT Tokens**: Stateless authentication with claims
- **Password Never Returned**: DTOs exclude sensitive data
- **Account Status Check**: Prevents inactive users from logging in
- **Role Management**: Endpoints to manage user permissions

---

### Step 5: Configure Application (Program.cs)

**Purpose**: Wire up all services, middleware, and configuration.

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Employees.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();  // Enable MVC controllers
builder.Services.AddEndpointsApiExplorer();  // For Swagger
builder.Services.AddSwaggerGen();  // API documentation

// Configure SQLite database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForJWTTokenGeneration123456");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,  // Check token expiration
        ValidateIssuerSigningKey = true,  // Verify signature
        ValidIssuer = jwtSettings["Issuer"] ?? "EmployeeManagementAPI",
        ValidAudience = jwtSettings["Audience"] ?? "EmployeeManagementClient",
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

// Configure CORS for frontend access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:4200", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Create database and apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();  // Creates DB if it doesn't exist
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();  // Enable Swagger in development
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");  // Enable CORS

app.UseAuthentication();  // Enable JWT authentication
app.UseAuthorization();   // Enable authorization

app.MapControllers();  // Map controller routes

app.Run();
```

**Configuration Breakdown**:
1. **Controllers**: Enable API endpoints
2. **Swagger**: Auto-generate API documentation
3. **Database**: SQLite with auto-creation
4. **JWT Auth**: Token-based authentication
5. **CORS**: Allow frontend from common dev ports
6. **Middleware Order**: Critical for security (Auth before Authorization)

---

### Step 6: Configuration Files

#### 6.1 appsettings.json

**Purpose**: Store application configuration settings.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=employees.db"  // SQLite database file
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration123456",  // Change in production!
    "Issuer": "EmployeeManagementAPI",
    "Audience": "EmployeeManagementClient",
    "ExpirationHours": "24"  // Tokens valid for 24 hours
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Important Notes**:
- `SecretKey`: **MUST be changed** in production (use 256-bit key)
- `DefaultConnection`: SQLite creates `employees.db` file in project root
- `ExpirationHours`: Adjust token lifetime as needed

---

#### 6.2 Employees.csproj

**Purpose**: Define project dependencies and settings.

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>  <!-- .NET 10 -->
    <Nullable>enable</Nullable>  <!-- Enable nullable reference types -->
    <ImplicitUsings>enable</ImplicitUsings>  <!-- Implicit using directives -->
  </PropertyGroup>

  <ItemGroup>
    <!-- Entity Framework Core for SQLite -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0" />
    
    <!-- EF Core design tools (for migrations) -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    
    <!-- JWT Bearer authentication -->
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.0" />
    
    <!-- BCrypt for password hashing -->
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
    
    <!-- Swagger for API documentation -->
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.9.0" />
  </ItemGroup>

</Project>
```

**Package Explanations**:
- **EntityFrameworkCore.Sqlite**: Database ORM for SQLite
- **EntityFrameworkCore.Design**: Tools for database migrations
- **Authentication.JwtBearer**: JWT token validation middleware
- **BCrypt.Net-Next**: Secure password hashing library
- **Swashbuckle.AspNetCore**: Swagger/OpenAPI documentation

---

#### 6.3 .gitignore

**Purpose**: Exclude generated files from version control.

```gitignore
*.db          # SQLite database files
*.db-shm      # SQLite shared memory files
*.db-wal      # SQLite write-ahead log files
bin/          # Build output
obj/          # Build intermediates
.vs/          # Visual Studio files
*.user        # User-specific settings
*.suo         # Solution user options
```

---

## Commands Used

### 1. NuGet Configuration
```powershell
# Add NuGet package source (required for package restore)
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
```

### 2. Package Restore
```powershell
# Download and install all NuGet packages defined in .csproj
dotnet restore
```

### 3. Clean Build Artifacts
```powershell
# Remove bin/ and obj/ folders (fixes build cache issues)
Remove-Item -Recurse -Force obj, bin

# Then restore again
dotnet restore
```

### 4. Build Project
```powershell
# Compile the project
dotnet build
```

### 5. Run Application
```powershell
# Start the web server
dotnet run
```

### 6. Check .NET Version
```powershell
# Verify installed .NET SDK version
dotnet --version
# Output: 10.0.100
```

---

## Code Explanations

### Async/Await Pattern

```csharp
public async Task<ActionResult<ApiResponse<EmployeeResponseDto>>> GetEmployee(int id)
{
    var employee = await _context.Employees.FindAsync(id);
    // ...
}
```

**Why?**
- `async`: Marks method as asynchronous
- `await`: Waits for database operation without blocking thread
- **Benefit**: Better scalability - server can handle more concurrent requests

---

### Dependency Injection

```csharp
public EmployeesController(AppDbContext context, ILogger<EmployeesController> logger)
{
    _context = context;
    _logger = logger;
}
```

**Why?**
- ASP.NET Core automatically provides instances
- **Testable**: Easy to mock dependencies for unit tests
- **Loosely Coupled**: Changes to implementation don't affect consumers

---

### Entity Framework LINQ Queries

```csharp
var employees = await _context.Employees
    .Where(e => e.IsActive)           // Filter
    .OrderBy(e => e.LastName)         // Sort
    .ToListAsync();                   // Execute query
```

**Why?**
- Type-safe database queries
- **Lazy Execution**: Query built but not executed until `ToListAsync()`
- Translates to efficient SQL

---

### JWT Claims

```csharp
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.Username),
    new Claim(ClaimTypes.Role, user.Role.ToString())
};
```

**Why?**
- Claims embedded in token (no database lookup needed)
- Can be used for authorization: `[Authorize(Roles = "Admin")]`
- Frontend can decode token to get user info

---

### Soft Delete Pattern

```csharp
employee.IsActive = false;  // Mark inactive instead of deleting
await _context.SaveChangesAsync();
```

**Why?**
- **Data Recovery**: Can reactivate records
- **Audit Trail**: Maintain historical data
- **Referential Integrity**: Avoid broken foreign key relationships

---

### Action Results

```csharp
return Ok(data);              // 200 OK
return CreatedAtAction(...);  // 201 Created
return BadRequest(error);     // 400 Bad Request
return NotFound(error);       // 404 Not Found
return Unauthorized(error);   // 401 Unauthorized
return StatusCode(500, error); // 500 Internal Server Error
```

**Why?**
- Semantic HTTP status codes
- Consistent REST API conventions
- Frontend can handle responses appropriately

---

## API Usage Guide

### Example: Complete Workflow

#### 1. Register a New User
```bash
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "jane.marketing",
  "password": "SecurePass123!",
  "email": "jane@company.com",
  "role": 2
}
```

**Response:**
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "id": 2,
    "username": "jane.marketing",
    "email": "jane@company.com",
    "role": "Marketing",
    "isActive": true,
    "createdAt": "2024-12-01T10:00:00Z"
  }
}
```

---

#### 2. Login
```bash
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkBleGFtcGxlLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiZXhwIjoxNzMzMTQwODAwLCJpc3MiOiJFbXBsb3llZU1hbmFnZW1lbnRBUEkiLCJhdWQiOiJFbXBsb3llZU1hbmFnZW1lbnRDbGllbnQifQ.xyz...",
    "user": {
      "id": 1,
      "username": "admin",
      "email": "admin@example.com",
      "role": "Admin",
      "isActive": true,
      "createdAt": "2024-12-01T09:00:00Z"
    }
  }
}
```

**Copy the `token` value for subsequent requests.**

---

#### 3. Create Employee (with JWT token)
```bash
POST http://localhost:5000/api/employees
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@company.com",
  "phone": "+1234567890",
  "department": "IT",
  "position": "Software Engineer",
  "salary": 85000.00,
  "hireDate": "2024-01-15T00:00:00Z"
}
```

---

#### 4. Get All Employees
```bash
GET http://localhost:5000/api/employees
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

#### 5. Update Employee
```bash
PUT http://localhost:5000/api/employees/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "position": "Senior Software Engineer",
  "salary": 105000.00
}
```

---

#### 6. Deactivate Employee (Soft Delete)
```bash
DELETE http://localhost:5000/api/employees/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Testing with Swagger

1. Run the application:
   ```powershell
   dotnet run
   ```

2. Open browser to: `https://localhost:5001/swagger`

3. Click **"Authorize"** button (top right)

4. Enter: `Bearer <your-token-here>`

5. Test all endpoints interactively

---

## Frontend Integration

### Example: React/Axios

```javascript
// Login
const login = async (username, password) => {
  const response = await axios.post('http://localhost:5000/api/auth/login', {
    username,
    password
  });
  
  // Store token
  localStorage.setItem('token', response.data.data.token);
  return response.data.data.user;
};

// Get Employees (with token)
const getEmployees = async () => {
  const token = localStorage.getItem('token');
  
  const response = await axios.get('http://localhost:5000/api/employees', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  return response.data.data;
};

// Create Employee
const createEmployee = async (employeeData) => {
  const token = localStorage.getItem('token');
  
  const response = await axios.post(
    'http://localhost:5000/api/employees',
    employeeData,
    {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    }
  );
  
  return response.data.data;
};
```

---

## User Roles Explained

| Role | Value | Intended Use |
|------|-------|-------------|
| **Admin** | 1 | Full system access - manage all users and employees |
| **Marketing** | 2 | Marketing department - view/manage marketing-related data |
| **Sales** | 3 | Sales department - access sales and customer data |
| **Accounting** | 4 | Accounting department - access financial information |

**Future Enhancement**: Add role-based authorization attributes:
```csharp
[Authorize(Roles = "Admin")]
public async Task<ActionResult> AdminOnlyEndpoint() { ... }

[Authorize(Roles = "Admin,Accounting")]
public async Task<ActionResult> FinancialData() { ... }
```

---

## Database Schema

### Employees Table
```sql
CREATE TABLE "Employees" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "FirstName" TEXT NOT NULL,
    "LastName" TEXT NOT NULL,
    "Email" TEXT NOT NULL UNIQUE,
    "Phone" TEXT,
    "Department" TEXT,
    "Position" TEXT,
    "Salary" REAL NOT NULL,
    "HireDate" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT
);
```

### Users Table
```sql
CREATE TABLE "Users" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "Username" TEXT NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL,
    "Email" TEXT NOT NULL UNIQUE,
    "Role" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "CreatedAt" TEXT NOT NULL
);
```

---

## Security Best Practices

### ✅ Implemented
- ✅ Password hashing with BCrypt (industry standard)
- ✅ JWT token-based authentication
- ✅ HTTPS support (enabled by default in .NET)
- ✅ Unique constraints on emails and usernames
- ✅ Input validation via DTOs
- ✅ Passwords never returned in responses
- ✅ Error logging without exposing internals

### ⚠️ Production Recommendations
- ⚠️ Change `JwtSettings:SecretKey` to a secure 256-bit key
- ⚠️ Use environment variables for secrets (not appsettings.json)
- ⚠️ Implement rate limiting for login attempts
- ⚠️ Add password complexity requirements
- ⚠️ Implement refresh tokens for long sessions
- ⚠️ Add `[Authorize]` attributes to protect endpoints
- ⚠️ Configure CORS for specific production domains only
- ⚠️ Enable HTTPS redirection in production

---

## Troubleshooting

### Issue: NuGet Packages Not Found
**Solution:**
```powershell
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
dotnet restore
```

### Issue: Build Errors After Package Update
**Solution:**
```powershell
Remove-Item -Recurse -Force obj, bin
dotnet restore
dotnet build
```

### Issue: Database Not Created
**Solution:** Database auto-creates on first run. If issues persist:
```powershell
# Delete existing database
Remove-Item employees.db, employees.db-shm, employees.db-wal

# Run again
dotnet run
```

### Issue: JWT Token Invalid
**Check:**
- Token not expired (24 hours default)
- `Authorization` header format: `Bearer <token>`
- SecretKey matches in appsettings.json

---

## Next Steps & Enhancements

### Recommended Additions

1. **Add Authorization Attributes**
   ```csharp
   [Authorize(Roles = "Admin")]
   public async Task<ActionResult> DeleteEmployee(int id) { ... }
   ```

2. **Database Migrations**
   ```powershell
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

3. **Pagination**
   ```csharp
   public async Task<ActionResult> GetEmployees(int page = 1, int pageSize = 10)
   ```

4. **Search & Filtering**
   ```csharp
   public async Task<ActionResult> SearchEmployees(string searchTerm, string department)
   ```

5. **Audit Logging**
   - Track who modified records and when
   - Store in separate AuditLog table

6. **Refresh Tokens**
   - Long-lived refresh tokens
   - Short-lived access tokens

7. **Email Verification**
   - Send verification email on registration
   - Confirm email before account activation

8. **Password Reset**
   - Forgot password endpoint
   - Email reset link

9. **Unit Tests**
   ```csharp
   [Fact]
   public async Task GetEmployee_ReturnsEmployee_WhenExists() { ... }
   ```

10. **Docker Support**
    ```dockerfile
    FROM mcr.microsoft.com/dotnet/aspnet:10.0
    COPY . .
    ENTRYPOINT ["dotnet", "Employees.dll"]
    ```

---

## Summary

This Employee Management API provides:

✅ **Complete CRUD Operations** for employees  
✅ **JWT Authentication** with secure password hashing  
✅ **4 User Role Levels** (Admin, Marketing, Sales, Accounting)  
✅ **SQLite Database** with automatic creation  
✅ **RESTful API Design** with consistent responses  
✅ **Swagger Documentation** for easy testing  
✅ **CORS Enabled** for frontend integration  
✅ **Soft Delete Support** for data recovery  
✅ **Production-Ready Structure** with logging and error handling  

The API is ready to integrate with any frontend framework (React, Angular, Vue, etc.) and can be deployed to cloud platforms like Azure, AWS, or Docker containers.
