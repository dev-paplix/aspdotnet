namespace Employees.Controllers.MVC;

using Microsoft.AspNetCore.Mvc;
using Employees.Models;
using Employees.ViewModels;
using Employees.Repositories;

public class AccountController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IUserRepository userRepository, ILogger<AccountController> logger)

    {
        _userRepository = userRepository;
        _logger = logger;
    }

    // GET: /Account/login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToAction("Index", "Employees");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // Post: /Account/login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var user = await _userRepository.GetByUsernameAsync(model.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid Username or Password");
                return View(model);
            }
            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Your account is deactivated");
                return View(model);
            }

            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.Role.ToString());

            TempData["SuccessMessage"] = $"Welcome back, {user.Username}!";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Employees");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login!");
            ModelState.AddModelError("", "An Error occured during login");
            return View(model);
        }
    }

    // Get: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // Post: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try
        {
            // Check if username is unique
            if (await _userRepository.UsernameExistsAsync(model.Username))
            {
                ModelState.AddModelError("Username", "Username already exist");
                return View(model);
            }

            // Check if eamil is unique
            if (await _userRepository.EmailExistsAsync(model.Email))
            {
                ModelState.AddModelError("Email", "Email already exist");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role,
                IsActive = true
            };

            await _userRepository.CreateAsync(user);

            TempData["SuccessMessage"] = "Registration successful!";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            ModelState.AddModelError("", "An erro occured during registration");
            return View(model);
        }
    }

    //POST: //Account/Logout

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["SuccessMessage"] = "You have been logged out successfully";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
