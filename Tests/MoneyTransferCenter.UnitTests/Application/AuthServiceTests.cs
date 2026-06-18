using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MoneyTransferCenter.Application.Dtos.Auth;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Application.Services;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Exceptions;
using NSubstitute;

namespace MoneyTransferCenter.UnitTests.Application;

public class AuthServiceTests
{
    //UserManager sınıfının yapıcı metodunda birçok bağımlılık vardır ve bu bağımlılıkların çoğu test sırasında kullanılmayacak. Bu nedenle, sadece gerekli olan IUserStore<AppUser> bağımlılığını sağlamak için Substitute.For<IUserStore<AppUser>>() kullanıyoruz ve diğer bağımlılıkları null olarak geçiyoruz. Bu, testlerin daha basit ve odaklı olmasını sağlar
    private readonly UserManager<AppUser> _userManagerMock = Substitute.For<UserManager<AppUser>>(Substitute.For<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);
    private readonly ITokenService _tokenServiceMock = Substitute.For<ITokenService>();
    private readonly IAuditService _auditServiceMock = Substitute.For<IAuditService>();
    private readonly IAccountService _accountServiceMock = Substitute.For<IAccountService>();
    private readonly ILogger<AuthService> _loggerMock = Substitute.For<ILogger<AuthService>>();

    private readonly AuthService _sut;
    public AuthServiceTests()
    {
        _sut = new AuthService(
            _userManagerMock, _tokenServiceMock, _auditServiceMock, _loggerMock, _accountServiceMock);
    }

    // yanlış epostayı test eden kod
    [Fact]
    public async Task LoginAsync_ShouldThrowDomainException_WhenEmailNotFound()
    {
        // Arrange 
        var request = new LoginRequestDto 
        {
            Email = "test@test.com", 
            Password = "Password123*"
        };

        AppUser? existingUser = null;

        _userManagerMock
            .FindByEmailAsync(request.Email)
            .Returns(existingUser);
        // Act 
        Func<Task> act = async () => await _sut.LoginAsync(request);
        // Assert 
        await act.Should().ThrowAsync<DomainException>().WithMessage("E-posta veya şifre hatalı.");
    }

    // Kullanıcı kayıt olurken aynı e-posta ile ikinci kez kayıt olmasını engellediğini test eder.
    [Fact]
    public async Task RegisterAsync_ShouldThrowDomainException_WhenEmailExists()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            Password = "Password123*",
            NationalId = "12345678901",
        };
        _userManagerMock.FindByEmailAsync(request.Email).Returns(new AppUser());
        // Act 
        Func<Task> act = async () => await _sut.RegisterAsync(request);
        // Assert 
        await act.Should().ThrowAsync<DomainException>().WithMessage("Bu e-posta adresi zaten kayıtlı.");
    }
}
