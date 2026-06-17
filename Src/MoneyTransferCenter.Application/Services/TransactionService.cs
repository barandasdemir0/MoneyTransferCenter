using Microsoft.Extensions.Logging;
using MoneyTransferCenter.Application.Dtos.Transaction;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Interfaces;
using MoneyTransferCenter.Domain.Interfaces.Repositories;
using System.Text.Json;

namespace MoneyTransferCenter.Application.Services;

public sealed class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly IAccountRepository _accountRepository;
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(IUnitOfWork unitOfWork, IAuditService auditService, ILogger<TransactionService> logger, IAccountRepository accountRepository, IOutboxMessageRepository outboxMessageRepository)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _logger = logger;
        _accountRepository = accountRepository;
        _outboxMessageRepository = outboxMessageRepository;
    }

    public async Task<DepositResponseDto> DepositAsync(Guid userId, DepositRequestDto request)
    {
        _logger.LogInformation("Para yükleme başlatıldı. UserId: {UserId}, Tutar: {Amount}", userId, request.Amount);

        Account? account = await _accountRepository.GetByUserIdAsync(userId);

        if (account == null)
        {
            throw new Exception("Hesap bulunamadı.");
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // DDD ile para yükle 
            account.Deposit(request.Amount);

            // Hesabı güncelle ve kaydet
            _accountRepository.Update(account);

            // Outbox mesajını oluştur ve kaydet
            string payload = JsonSerializer.Serialize(new
            {
                AccountId = account.Id,
                IBAN = account.IBAN,
                Amount = request.Amount,
                NewBalance = account.Balance,
                OccurredAt = DateTimeOffset.UtcNow
            });

            await _outboxMessageRepository.AddAsync(OutboxMessage.Create("DepositCompleted", payload));
            await _unitOfWork.CommitTransactionAsync();

            await _auditService.LogMoneyDepositedAsync(
                    userId,
                account.Id,
                account.IBAN,
                    request.Amount,
                    account.Balance);
            _logger.LogInformation("Para yüklendi. IBAN: {IBAN}, Yeni Bakiye: {Balance}", account.IBAN, account.Balance);

            return new DepositResponseDto(
                AccountId: account.Id,
                IBAN: account.IBAN,
                NewBalance: account.Balance,
                DepositedAmount: request.Amount,
                TransactionDate: DateTimeOffset.UtcNow


            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Para yükleme başarısız. UserId: {UserId}", userId);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
      

      


    }

    public Task<TransferResponseDto> TransferAsync(Guid userId, TransferRequestDto request)
    {
        throw new NotImplementedException();
    }
}
