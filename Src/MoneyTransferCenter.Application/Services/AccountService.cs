using Mapster;
using Microsoft.Extensions.Logging;
using MoneyTransferCenter.Application.Dtos.Account;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Exceptions;
using MoneyTransferCenter.Domain.Interfaces;
using MoneyTransferCenter.Domain.Interfaces.Repositories;

namespace MoneyTransferCenter.Application.Services;

public sealed class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIbanGenerator _ibanGenerator;
    private readonly IAuditService _auditService;
    private readonly ILogger<AccountService> _logger;
    public AccountService(
        IAccountRepository accountRepository,
        IIbanGenerator ibanGenerator,
        IAuditService auditService,
        ILogger<AccountService> logger,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _ibanGenerator = ibanGenerator;
        _auditService = auditService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountResponseDto> CompleteProfileAsync(Guid userId, CompleteProfileRequestDto request)
    {
        _logger.LogInformation("Profil tamamlanıyor. UserId: {UserId}", userId);
        Account? account = await _accountRepository.GetByUserIdAsync(userId);
        if (account == null)
        {
            _logger.LogWarning("Hesap bulunamadı. UserId: {UserId}", userId);
            throw new DomainException("Hesap bulunamadı.", "ACCOUNT_NOT_FOUND");
        }

        if (account.Status.CanModify() == false)
        {
            _logger.LogWarning("Kapalı hesap güncellenemez. UserId: {UserId}", userId);
            throw new DomainException("Kapatılmış hesap üzerinde değişiklik yapılamaz.", "ACCOUNT_CLOSED");
        }

        // DDD ile metodlarıyla profili güncelle
        account.UpdateProfile(request.Address, request.City, request.PostalCode, request.TelephoneNumber);
        _logger.LogInformation("Profil bilgileri güncellendi. UserId: {UserId}", userId);
        await _auditService.LogProfileCompletedAsync(userId);

        if (account.Status.CanBeActivated() && account.IsProfileComplete())
        {
            account.Activate();
            _logger.LogInformation("Hesap otomatik aktif edildi. UserId: {UserId}", userId);
            await _auditService.LogAccountActivatedAsync(userId);
        }
        _accountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync();
        return account.Adapt<AccountResponseDto>();
    }

    public async Task<AccountResponseDto> CreateAccountForUserAsync(Guid userId)
    {
        _logger.LogInformation("Hesap oluşturuluyor. UserId: {UserId}", userId);


        Account? existingAccount = await _accountRepository.GetByUserIdAsync(userId);
        if (existingAccount != null)
        {
            _logger.LogWarning("Kullanıcının zaten hesabı var. UserId: {UserId}", userId);
            throw new DomainException("Bu kullanıcının zaten bir hesabı bulunmaktadır.", "ACCOUNT_ALREADY_EXISTS");
        }

        // Benzersiz IBAN üret
        string iban = await _ibanGenerator.GenerateAsync();
        _logger.LogInformation("IBAN üretildi: {IBAN}", iban);



        Account newAccount = new Account(userId, iban);


        await _accountRepository.AddAsync(newAccount);
        await _unitOfWork.SaveChangesAsync();
        await _auditService.LogAccountCreatedAsync(userId, iban);

        _logger.LogInformation("Hesap oluşturuldu. UserId: {UserId}, IBAN: {IBAN}", userId, iban);
        return newAccount.Adapt<AccountResponseDto>();

       
    }

    public async Task<List<UserListItemResponseDto>> GetAllUsersAsync()
    {
        _logger.LogInformation("Tüm kullanıcılar listeleniyor.");
        List<Account> accounts = await _accountRepository.GetAllAsync();
        List<UserListItemResponseDto> result = accounts.Adapt<List<UserListItemResponseDto>>();
        _logger.LogInformation("{Count} kullanıcı listelendi.", result.Count);
        return result;
    }

    public async Task<AccountResponseDto> GetMyAccountAsync(Guid userId)
    {
        Account? account = await _accountRepository.GetByUserIdAsync(userId);
        if (account == null)
        {
            throw new DomainException("Hesap bulunamadı.", "ACCOUNT_NOT_FOUND");
        }
        return account.Adapt<AccountResponseDto>();
    }


}
