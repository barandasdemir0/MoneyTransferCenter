using FluentAssertions;
using Microsoft.Extensions.Logging;
using MoneyTransferCenter.Application.Dtos.Transaction.Deposit;
using MoneyTransferCenter.Application.Dtos.Transaction.Transfer;
using MoneyTransferCenter.Application.Dtos.Transaction.WithDraw;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Application.Services;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Exceptions;
using MoneyTransferCenter.Domain.Interfaces;
using MoneyTransferCenter.Domain.Interfaces.Repositories;
using NSubstitute;

namespace MoneyTransferCenter.UnitTests.Application;

public class TransactionServiceTests
{
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IAuditService _auditServiceMock = Substitute.For<IAuditService>();
    private readonly ITransactionRepository _transactionRepositoryMock = Substitute.For<ITransactionRepository>();
    private readonly IAccountRepository _accountRepositoryMock = Substitute.For<IAccountRepository>();
    private readonly IOutboxMessageRepository _outboxMessageRepositoryMock = Substitute.For<IOutboxMessageRepository>();
    private readonly ILogger<TransactionService> _loggerMock = Substitute.For<ILogger<TransactionService>>();
    private readonly TransactionService _sut;

    public TransactionServiceTests()
    {
        _sut = new TransactionService(
            _unitOfWorkMock, _auditServiceMock, _loggerMock,
            _accountRepositoryMock, _outboxMessageRepositoryMock, _transactionRepositoryMock);
    }

    // hesapda para yatırma işlemi için testler
    [Fact]
    public async Task DepositAsync_ShouldThrowDomainException_WhenAccountNotFound()
    {
        // Arrange 
        var userId = Guid.CreateVersion7();
        var request = new DepositRequestDto 
        {
            Amount = 100 
        }; 
        Account? account = null;
        _accountRepositoryMock
            .GetByUserIdAsync(userId)
            .Returns(account);

        // Act 
        Func<Task> act = async () => await _sut.DepositAsync(userId, request);

        // Assert 
        await act.Should().ThrowAsync<DomainException>().WithMessage("Hesap bulunamadı.");
    }


    //para yoksa para çekme işlemi için testler
    [Fact]
    public async Task WithdrawAsync_ShouldThrowDomainException_WhenAccountNotFound()
    {
        // Arrange 
        var userId = Guid.CreateVersion7();
        var request = new WithdrawRequestDto(100);
        Account? account = null;
        _accountRepositoryMock
            .GetByUserIdAsync(userId)
            .Returns(account);
        // Act 
        Func<Task> act = async () => await _sut.WithdrawAsync(userId, request);
        // Assert 
        await act.Should().ThrowAsync<DomainException>().WithMessage("Hesap bulunamadı.");

    }


    // transfer işlemi için alıcı hesap bulunamadığında hata testleri
    [Fact]
    public async Task TransferAsync_ShouldThrowDomainException_WhenReceiverAccountNotFound()
    {
        // Arrange 
        var senderId = Guid.CreateVersion7();
      

        var request = new TransferRequestDto
        {
            ReceiverIBAN = "TR99999999",
            Amount = 100,
            Description = "Kira"
        };

        var senderAccount = new Account(senderId, "TR123456");
        _accountRepositoryMock.GetByUserIdAsync(senderId).Returns(senderAccount); 

        Account? receiverAccount = null; 
        _accountRepositoryMock.GetByIbanAsync(request.ReceiverIBAN).Returns(receiverAccount);
        // Act 
        Func<Task> act = async () => await _sut.TransferAsync(senderId, request);
        // Assert 
        await act.Should().ThrowAsync<DomainException>().WithMessage("Alıcı IBAN bulunamadı.");
    }




}
