using Mapster;
using Microsoft.Extensions.Logging;
using MoneyTransferCenter.Application.Dtos.Transaction;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Domain.Constants;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Enums;
using MoneyTransferCenter.Domain.Exceptions;
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
                throw new DomainException("Hesap bulunamadı.", "ACCOUNT_NOT_FOUND");
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
                account.IBAN,
                request.Amount,
                NewBalance = account.Balance,
                OccurredAt = DateTimeOffset.UtcNow
            });

            await _outboxMessageRepository.AddAsync(OutboxMessage.Create(OutboxEventTypes.DepositCompleted, payload));
            await _unitOfWork.CommitTransactionAsync();

         

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

    public async Task<List<TransactionHistoryItemResponseDto>> GetHistoryAsync(Guid userId, TransactionHistoryRequestDto request)
    {

        var account = await _accountRepository.GetByUserIdAsync(userId)
            ?? throw new DomainException("Hesap bulunamadı.", "ACCOUNT_NOT_FOUND");
 
        var transactions = request.Filter switch
        {
            TransactionHistoryFilter.Sent => await _transactionRepository.GetSentByAccountIdAsync(
                              account.Id, request.Page, request.PageSize),
            TransactionHistoryFilter.Received => await _transactionRepository.GetReceivedByAccountIdAsync(
                              account.Id, request.Page, request.PageSize),
            _ => await _transactionRepository.GetAllByAccountIdAsync(
                              account.Id, request.Page, request.PageSize)
        };

        _logger.LogInformation(
            "İşlem geçmişi görüntülendi. UserId: {UserId}, Filter: {Filter}",
            userId,
            request.Filter);

        return transactions.Adapt<List<TransactionHistoryItemResponseDto>>();

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
                throw new DomainException("Gönderen hesabı bulunamadı.", "SENDER_NOT_FOUND");

            }

            // Alıcının hesabını bul
            Account? receiverAccount = await _accountRepository.GetByIbanAsync(request.ReceiverIBAN);
            if (receiverAccount == null)
            {
                throw new DomainException("Alıcı IBAN bulunamadı.", "RECEIVER_NOT_FOUND");
            }

            // Kendine transfer engeli
            if (senderAccount.Id == receiverAccount.Id)
            {
                throw new DomainException("Kendi hesabınıza transfer yapamazsınız.", "SELF_TRANSFER_ERROR");
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
                transaction.ReferenceNumber,
                SenderAccountId = senderAccount.Id,
                SenderIBAN = senderAccount.IBAN,
                ReceiverAccountId = receiverAccount.Id,
                ReceiverIBAN = receiverAccount.IBAN,
                request.Amount,
                request.Description,
                OccurredAt = DateTimeOffset.UtcNow
            });
            await _outboxMessageRepository.AddAsync(OutboxMessage.Create(OutboxEventTypes.TransferCompleted, payload));

            // Hepsini atomik olarak commit et
            await _unitOfWork.CommitTransactionAsync();
            _logger.LogInformation("Transfer audit yazılacak. UserId: {UserId}", userId);
           

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
