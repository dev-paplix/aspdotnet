namespace Employees.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Employees.Data;
using Employees.Models;
using Employees.DTOs;

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
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> Register(UserRegisterDto dto)
    {
        try
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return BadRequest(new ApiResponse<UserResponseDto>
                {
                    Success = false,
                    Message = "Username already exists."
                });
            }
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(new ApiResponse<UserResponseDto>
                {
                    Success = false,
                    Message = "Email already exists."
                });
            }

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new {id =user.Id },
                new ApiResponse<UserResponseDto>
                {
                    Success = true,
                    Message = "User Registered Successfully",
                    Data = MapToUserDto(user)
                });
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Registering User");
            return StatusCode(500, new ApiResponse<UserResponseDto>
            {
                Success = false,
                Message = "An error occurred while registering the user."
            });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(UserLoginDto dto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "Invalid username or password"
                });
            }
            if (!user.IsActive)
            {
                return Unauthorized(new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "User account is not active"
                });
            }

            var token =  GenerateJwtToken(user);

            return Ok(new ApiResponse<LoginResponseDto>
            {
                Success = true,
                Message = "Login Successful",
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
                Message = "An error occured during login"
            });
        }
    }

    [HttpGet ("users")]
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
                Message = "Users retrive successfully",
                Data = userDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retreiving users");
            return StatusCode(500, new ApiResponse<IEnumerable<UserResponseDto>>
            {
                Success = false,
                Message = "An error occured while fetching users"
            });
        }
    }

    //Get api/auth/users/{id}
    [HttpGet("users/{id}")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetUser(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);

            if (user ==null)
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
                Message = "User fetched successfully",
                Data = MapToUserDto(user)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user {id}", id);
            return StatusCode(500, new ApiResponse<UserResponseDto>
            {
                Success = false,
                Message = "Error occured while fetching user"
            });
        }
    }

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
            _logger.LogError(ex, "Error fetching user {id}", id);
            return StatusCode(500, new ApiResponse<UserResponseDto>
            {
                Success = false,
                Message = "Error occured while updating the user role"
            });
        }
    }

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
            _logger.LogError(ex, "Error fetching user {id}", id);
            return StatusCode(500, new ApiResponse<UserResponseDto>
            {
                Success = false,
                Message = "Error occured while updating the user status"
            });
        }
    }

    [HttpDelete]
    public async Task<ActionResult<ApiResponse<string>>> DeleteUser(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "User deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {id}", id);
            return StatusCode(500, new ApiResponse<string>
            {
                Success = false,
                Message = "Error occured while deleting the user"
            });
        }
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

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
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static UserResponseDto MapToUserDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

}