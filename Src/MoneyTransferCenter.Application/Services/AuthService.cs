using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoneyTransferCenter.Application.Dtos.Auth;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Application.Telemetry;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Exceptions;

namespace MoneyTransferCenter.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    private readonly IAccountService _accountService;
    private readonly ILogger<AuthService> _logger;
    public AuthService(
        UserManager<AppUser> userManager,
        ITokenService tokenService,
        IAuditService auditService,
        ILogger<AuthService> logger,
        IAccountService accountService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _auditService = auditService;
        _logger = logger;
        _accountService = accountService;
    }
    public async Task<AuthResponse> LoginAsync(LoginRequestDto request)
    {
        _logger.LogInformation("Giriş isteği alındı. E-posta: {Email}", request.Email);

        //Kullanıcıyı bul
        AppUser? user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Kullanıcı bulunamadı: {Email}", request.Email);
            AppMetrics.LoginFailedCount.Add(1);
            throw new DomainException("E-posta veya şifre hatalı.", "INVALID_CREDENTIALS");

        }

        bool isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (isPasswordValid == false)
        {
            _logger.LogWarning("Şifre hatalı. UserId: {UserId}", user.Id);
            AppMetrics.LoginFailedCount.Add(1);
            await _auditService.LogLoginFailedAsync(user, "Hatalı şifre denemesi");
            throw new DomainException("E-posta veya şifre hatalı.", "INVALID_CREDENTIALS");
           
        }

        // 3) Başarılı giriş logla
        await _auditService.LogUserLoggedInAsync(user);

        

        //Token oluştur
        string token = _tokenService.GenerateToken(user);
        _logger.LogInformation("Giriş başarılı. UserId: {UserId}", user.Id);
        AppMetrics.LoginCount.Add(1);
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
            AppMetrics.RegisterFailedCount.Add(1);
            throw new DomainException("Bu e-posta adresi zaten kayıtlı.", "EMAIL_ALREADY_EXISTS");
        }

        AppUser? existingNationalId = await _userManager.Users.FirstOrDefaultAsync(x => x.NationalId == request.NationalId);
        if (existingNationalId != null)
        {
            _logger.LogWarning("Kayıt reddedildi. Tc Kimlik numarası zaten kayıtlı: {NationalId}", request.NationalId);
            AppMetrics.RegisterFailedCount.Add(1);
            throw new DomainException("Tc Kimlik numarası zaten kayıtlı.", "NATIONAL_ID_ALREADY_EXISTS");
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
            AppMetrics.RegisterFailedCount.Add(1);
            throw new DomainException($"Kayıt başarısız: {errors}", "REGISTRATION_FAILED");
        }

        try
        {
            await _accountService.CreateAccountForUserAsync(newUser.Id);
            _logger.LogInformation("Kullanıcı oluşturuldu. UserId: {UserId}", newUser.Id);
            AppMetrics.RegisterCount.Add(1);
            //Audit log yaz 
            await _auditService.LogUserRegisteredAsync(newUser);
        }
        catch (Exception ex)
        {
            // Eğer hesap açılırken hata çıkarsa, Identity'e kaydettiğimiz kullanıcıyı GERİ SİLİYORUZ.
            await _userManager.DeleteAsync(newUser);
            _logger.LogError(ex, "Banka hesabı oluşturulurken hata çıktı. Kullanıcı kaydı iptal edildi. UserId: {UserId}", newUser.Id);
            AppMetrics.RegisterFailedCount.Add(1);
            throw new DomainException("Kayıt işlemi sırasında sistemsel bir hata oluştu. Lütfen tekrar deneyin.", "REGISTRATION_SYSTEM_ERROR");
        }



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
