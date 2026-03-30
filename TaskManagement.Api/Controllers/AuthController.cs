using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        _logger.LogInformation("Attempting to register user with email {email}.", dto.Email);

        var registeredUser = await _authService.RegisterAsync(dto);

        _logger.LogInformation("User registered with success: {email}.", dto.Email);

        return Ok(registeredUser);

    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        _logger.LogInformation("Attempting to login user with email {email}.", dto.Email);

        var registeredUser = await _authService.LoginAsync(dto);

        _logger.LogInformation("User logged in with success: {email}.", dto.Email);

        return Ok(registeredUser);

    }
}