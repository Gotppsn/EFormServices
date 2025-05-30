// EFormServices.Web/Controllers/AuthController.cs
// Got code 30/05/2025
using Microsoft.AspNetCore.Mvc;
using EFormServices.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EFormServices.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = MockAuthDataService.GetUserByCredentials(request.Email, request.Password);
        if (user == null)
            return Unauthorized(new { message = "Invalid email or password" });

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        return Ok(new LoginResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            User = new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.OrganizationId
            }
        });
    }

    [HttpPost("refresh")]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            return BadRequest("Invalid token");

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return BadRequest("Invalid token");

        var newToken = GenerateJwtToken(int.Parse(userId));
        var newRefreshToken = GenerateRefreshToken();

        return Ok(new LoginResponse
        {
            AccessToken = newToken,
            RefreshToken = newRefreshToken
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Logged out successfully" });
    }

    private string GenerateJwtToken(Domain.Entities.User user)
    {
        return GenerateJwtToken(user.Id, user.Email, user.OrganizationId, user.FirstName, user.LastName);
    }

    private string GenerateJwtToken(int userId, string? email = null, int? organizationId = null, string? firstName = null, string? lastName = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrEmpty(email))
            claims.Add(new Claim(ClaimTypes.Email, email));
        if (organizationId.HasValue)
            claims.Add(new Claim("OrganizationId", organizationId.ToString()!));
        if (!string.IsNullOrEmpty(firstName))
            claims.Add(new Claim(ClaimTypes.GivenName, firstName));
        if (!string.IsNullOrEmpty(lastName))
            claims.Add(new Claim(ClaimTypes.Surname, lastName));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
        return principal;
    }
}

public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public object? User { get; init; }
}

public record RefreshTokenRequest
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}