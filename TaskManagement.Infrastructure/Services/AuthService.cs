using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Application.Common.Constants;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.DTOs;
using TaskManagement.Infrastructure.Identity;

namespace TaskManagement.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        _logger.LogInformation("Attempting to login user with email {email}.", dto.Email);

        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null)
        {
            _logger.LogInformation("User with email {email} not found!", dto.Email);
            throw new InvalidOperationException("User not registered");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);

        if (!isPasswordValid)
        {
            _logger.LogWarning("Login failed - invalid password for {Email}", dto.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        _logger.LogInformation("User {Email} logged in successfully", dto.Email);
        var token = await GenerateJwtTokenAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email!,
            UserName = user.UserName!
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        _logger.LogInformation("Attempting to register user with email {email}.", dto.Email);

        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user != null)
        {
            _logger.LogInformation("User with email {email} already exists.", dto.Email);
            throw new InvalidOperationException("Email already registered");
        }

        user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            EmailConfirmed = true // For simplicity, set to true. In production, send confirmation email,
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("User registration failed: {Errors}", errors);
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, Role.UserRole);

        _logger.LogInformation("User {Email} registered successfully", dto.Email);

        var token = await GenerateJwtTokenAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email!,
            UserName = user.UserName!
        };
    }

    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var secret = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("No secret provided");
        var issuer = jwtSettings["Issuer"] ?? "TaskManagementAPI";
        var audience = jwtSettings["Audience"] ?? "TaskManagementAPI";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, user.UserName!),
        };

        // User can have many roles
        var userRoles = await _userManager.GetRolesAsync(user);

        foreach (var role in userRoles)
        {
            claims.Add(new(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}