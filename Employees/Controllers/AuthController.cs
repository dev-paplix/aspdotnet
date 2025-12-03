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

            return Ok(new ApiResponse<IEnumerable<UserRegisterDto>>
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
    [HttpGet("users/{id}")]

}