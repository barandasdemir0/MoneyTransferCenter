using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MoneyTransferCenter.Application.Dtos.Auth;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Domain.Constants;

namespace MoneyTransferCenter.WebAPI.Controllers;

[EnableRateLimiting(RateLimitPolicies.Strict)]
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        _logger.LogInformation("Register endpoint çağrıldı. E-posta: {Email}", request.Email);
        AuthResponse result = await _authService.RegisterAsync(request);
        _logger.LogInformation("Register başarılı. UserId: {UserId}", result.UserId);
        return Ok(result);
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        _logger.LogInformation("Login endpoint çağrıldı. E-posta: {Email}", request.Email);
        AuthResponse result = await _authService.LoginAsync(request);
        _logger.LogInformation("Login başarılı. UserId: {UserId}", result.UserId);
        return Ok(result);
    }
}
