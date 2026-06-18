using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTransferCenter.Application.Dtos.Account;
using MoneyTransferCenter.Application.Interfaces;
using System.Security.Claims;

namespace MoneyTransferCenter.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountController> _logger;
    public AccountController(IAccountService accountService, ILogger<AccountController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }



    // Kendi hesap bilgilerimi getir
    [HttpGet("me")]
    public async Task<IActionResult> GetMyAccount()
    {
        Guid userId = GetCurrentUserId();
        _logger.LogInformation("Hesap bilgisi istendi. UserId: {UserId}", userId);
        AccountResponseDto result = await _accountService.GetMyAccountAsync(userId);
        return Ok(result);
    }

    // Profil tamamla (adres, şehir, posta kodu, telefon)
    [HttpPut("complete-profile")]
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileRequestDto request)
    {
        Guid userId = GetCurrentUserId();
        _logger.LogInformation("Profil tamamlama istendi. UserId: {UserId}", userId);
        AccountResponseDto result = await _accountService.CompleteProfileAsync(userId, request);
        return Ok(result);
    }

    // Tüm kullanıcıları listele
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("Tüm kullanıcılar listeleniyor.");
        List<UserListItemResponseDto> result = await _accountService.GetAllUsersAsync();
        return Ok(result);
    }

    // JWT token'dan kullanıcı ID'sini oku
    private Guid GetCurrentUserId()
    {
        string? userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString))
        {
            throw new Exception("Kullanıcı kimliği bulunamadı. Lütfen giriş yapınız.");
        }
        return Guid.Parse(userIdString);
    }
}
