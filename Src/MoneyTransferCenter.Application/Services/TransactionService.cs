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
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(IUnitOfWork unitOfWork, IAuditService auditService, ILogger<TransactionService> logger, IAccountRepository accountRepository, IOutboxMessageRepository outboxMessageRepository, ITransactionRepository transactionRepository)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _logger = logger;
        _accountRepository = accountRepository;
        _outboxMessageRepository = outboxMessageRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<DepositResponseDto> DepositAsync(Guid userId, DepositRequestDto request)
    {
        try
        {
            _logger.LogInformation("Para yükleme başlatıldı. UserId: {UserId}, Tutar: {Amount}", userId, request.Amount);

            Account? account = await _accountRepository.GetByUserIdAsync(userId);

            if (account == null)
            {
                throw new Exception("Hesap bulunamadı.");
            }

            await _unitOfWork.BeginTransactionAsync();


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

            await _auditService.LogMoneyDepositFailedAsync(
                userId,
                request.Amount,
                ex.Message);

            throw;
        }





    }

    public async Task<TransferResponseDto> TransferAsync(Guid userId, TransferRequestDto request)
    {
        Account? senderAccount = null;
        try
        {
            _logger.LogInformation("Transfer başlatıldı. UserId: {UserId}, Alıcı IBAN: {IBAN}, Tutar: {Amount}", userId, request.ReceiverIBAN, request.Amount);

            // Gönderenin hesabını bul
            senderAccount = await _accountRepository.GetByUserIdAsync(userId);
            if (senderAccount == null)
            {
                throw new Exception("Gönderen hesabı bulunamadı.");
            }

            // Alıcının hesabını bul
            Account? receiverAccount = await _accountRepository.GetByIbanAsync(request.ReceiverIBAN);
            if (receiverAccount == null)
            {
                throw new Exception("Alıcı IBAN bulunamadı.");
            }

            // Kendine transfer engeli
            if (senderAccount.Id == receiverAccount.Id)
            {
                throw new Exception("Kendi hesabınıza transfer yapamazsınız.");
            }

            // Transfer kaydı oluştur (henüz Pending)
            Transaction transaction = Transaction.Create(
                senderAccountId: senderAccount.Id,
                receiverAccountId: receiverAccount.Id,
                amount: request.Amount,
                description: request.Description
            );

            await _unitOfWork.BeginTransactionAsync();


            // DDD burada çalışır
            senderAccount.Withdraw(request.Amount);
            receiverAccount.Deposit(request.Amount);
            transaction.MarkAsCompleted();
            await _transactionRepository.AddAsync(transaction);
            _accountRepository.Update(senderAccount);
            _accountRepository.Update(receiverAccount);

            // Outbox kaydı (AYNI MSSQL transaction içinde)
            // → MongoDB çökse bile bu kayıt MSSQL'de durur, BackgroundService sonra işler
            string payload = JsonSerializer.Serialize(new
            {
                TransactionId = transaction.Id,
                ReferenceNumber = transaction.ReferenceNumber,
                SenderAccountId = senderAccount.Id,
                SenderIBAN = senderAccount.IBAN,
                ReceiverAccountId = receiverAccount.Id,
                ReceiverIBAN = receiverAccount.IBAN,
                Amount = request.Amount,
                Description = request.Description,
                OccurredAt = DateTimeOffset.UtcNow
            });
            await _outboxMessageRepository.AddAsync(OutboxMessage.Create("TransferCompleted", payload));
            // Hepsini atomik olarak commit et
            await _unitOfWork.CommitTransactionAsync();
            _logger.LogInformation("Transfer audit yazılacak. UserId: {UserId}", userId);
            await _auditService.LogMoneyTransferredAsync(
                userId,
                senderAccount.Id,
                senderAccount.IBAN,
                receiverAccount.IBAN,
                request.Amount);

            _logger.LogInformation("Transfer tamamlandı. Referans:{Ref}", transaction.ReferenceNumber);
            return new TransferResponseDto(
                ReferenceNumber: transaction.ReferenceNumber,
                SenderIBAN: senderAccount.IBAN,
                ReceiverIBAN: receiverAccount.IBAN,
                Amount: request.Amount,
                Description: request.Description,
                Status: "Completed",
                TransactionDate: DateTimeOffset.UtcNow
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transfer başarısız. UserId: {UserId}", userId);

            await _unitOfWork.RollbackTransactionAsync();

            await _auditService.LogMoneyTransferFailedAsync(
                userId,
                senderAccount?.IBAN,
                request.ReceiverIBAN,
                request.Amount,
                ex.Message);

            throw;
        }
    }
}
