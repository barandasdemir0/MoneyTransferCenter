using FluentAssertions;
using Microsoft.Extensions.Logging;
using MoneyTransferCenter.Application.Dtos.Account;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Application.Services;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Enums;
using MoneyTransferCenter.Domain.Exceptions;
using MoneyTransferCenter.Domain.Interfaces;
using MoneyTransferCenter.Domain.Interfaces.Repositories;
using NSubstitute;

namespace MoneyTransferCenter.UnitTests.Application;

public class AccountServiceTests
{
    private readonly IAccountRepository _accountRepositoryMock = Substitute.For<IAccountRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IIbanGenerator _ibanGeneratorMock = Substitute.For<IIbanGenerator>();
    private readonly IAuditService _auditServiceMock = Substitute.For<IAuditService>();
    private readonly ILogger<AccountService> _loggerMock = Substitute.For<ILogger<AccountService>>();

    private readonly AccountService _sut;
    public AccountServiceTests()
    {
        _sut = new AccountService(
            _accountRepositoryMock, _ibanGeneratorMock, _auditServiceMock, _loggerMock, _unitOfWorkMock);
    }


    // Bir kullanıcının sisteme sadece 1 kez hesap açabileceği kuralını test eder.
    [Fact]
    public async Task CreateAccountForUserAsync_ShouldThrowException_WhenUserAlreadyHasAccount()
    {
        // Arrange 
        var userId = Guid.CreateVersion7();
        _accountRepositoryMock.GetByUserIdAsync(userId).Returns(new Account(userId, "TR123"));
        // Act 
        Func<Task> act = async () => await _sut.CreateAccountForUserAsync(userId);
        // Assert 
        await act.Should().ThrowAsync<DomainException>()
                 .WithMessage("Bu kullanıcının zaten bir hesabı bulunmaktadır.");
    }

    // Kullanıcı profilini tamamladığında, hesabının başarıyla 'Aktif' duruma geçtiğini test eder.
    [Fact]
    public async Task CompleteProfileAsync_ShouldActivateAccount_WhenValidRequest()
    {
        // Arrange (Hazırlık)
        var userId = Guid.CreateVersion7();
        var request = new CompleteProfileRequestDto
        {
            Address = "Test Mah.",
            City = "Istanbul",
            PostalCode = "34000",
            TelephoneNumber = "0555",            
        };

        var account = new Account(userId, "TR123456789012345678901234");
        _accountRepositoryMock.GetByUserIdAsync(userId).Returns(account);
        // Act 
        await _sut.CompleteProfileAsync(userId, request);
        // Assert 
        account.IsProfileComplete().Should().BeTrue();
        account.Status.Should().Be(AccountStatus.Active);
 
    }

}
