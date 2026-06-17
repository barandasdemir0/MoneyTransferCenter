using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MoneyTransferCenter.Application.Dtos.Auth;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Domain.Entities;

namespace MoneyTransferCenter.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthService> _logger;
    public AuthService(
        UserManager<AppUser> userManager,
        ITokenService tokenService,
        IAuditService auditService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _auditService = auditService;
        _logger = logger;
    }
    public async Task<AuthResponse> LoginAsync(LoginRequestDto request)
    {
        _logger.LogInformation("Giriş isteği alındı. E-posta: {Email}", request.Email);

        //Kullanıcıyı bul
        AppUser? user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Kullanıcı bulunamadı: {Email}", request.Email);
            throw new Exception("E-posta veya şifre hatalı.");
        }

        bool isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (isPasswordValid == false)
        {
            _logger.LogWarning("Şifre hatalı. UserId: {UserId}", user.Id);
            await _auditService.LogLoginFailedAsync(user, "Hatalı şifre denemesi");
            throw new Exception("E-posta veya şifre hatalı.");
        }
        // 3) Başarılı giriş logla
        await _auditService.LogUserLoggedInAsync(user);

        //Token oluştur
        string token = _tokenService.GenerateToken(user);
        _logger.LogInformation("Giriş başarılı. UserId: {UserId}", user.Id);
        return new AuthResponse
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddHours(24),
            Email = user.Email!,
            FullName = user.FullName,
            UserId = user.Id
        };
    }
    

    public async Task<AuthResponse> RegisterAsync(RegisterRequestDto request)
    {
        _logger.LogInformation("Kayıt isteği alındı. E-posta: {Email}", request.Email);

        //E-posta kontrolü
        AppUser? existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Kayıt reddedildi. E-posta zaten kayıtlı: {Email}", request.Email);
            throw new Exception("Bu e-posta adresi zaten kayıtlı.");
        }

        
        AppUser newUser = new AppUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            NationalId = request.NationalId,
            Email = request.Email,
            UserName = request.Email
        };

        // 3) Identity ile kaydet
        IdentityResult result = await _userManager.CreateAsync(newUser, request.Password);
        if (result.Succeeded == false)
        {
            string errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Kayıt hatası. E-posta: {Email}, Hatalar: {Errors}", request.Email, errors);
            throw new Exception($"Kayıt başarısız: {errors}");
        }

        _logger.LogInformation("Kullanıcı oluşturuldu. UserId: {UserId}", newUser.Id);
        //Audit log yaz 
        await _auditService.LogUserRegisteredAsync(newUser);
        //Token oluştur 
        string token = _tokenService.GenerateToken(newUser);
        _logger.LogInformation("Kayıt tamamlandı. UserId: {UserId}", newUser.Id);

        return new AuthResponse
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddHours(24),
            Email = newUser.Email!,
            FullName = newUser.FullName,
            UserId = newUser.Id
        };
    }
}
