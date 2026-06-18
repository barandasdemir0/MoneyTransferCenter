using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoneyTransferCenter.Application.Dtos.Auth;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.WebAPI.Controllers;
using NSubstitute;

namespace MoneyTransferCenter.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly AuthController _sut;

    private readonly IAuthService _authServiceMock =
        Substitute.For<IAuthService>();

    private readonly ILogger<AuthController> _loggerMock =
        Substitute.For<ILogger<AuthController>>();

    public AuthControllerTests()
    {
        _sut = new AuthController(
            _authServiceMock,
            _loggerMock);
    }


   
    // Kayıt işlemi başarılı olursa HTTP 200 (Ok) dönmeli
    [Fact]
    public async Task Register_ShouldReturnOk_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            Password = "Password123*"
        };

        _authServiceMock
      .RegisterAsync(request)
      .Returns(new AuthResponse
      {
          Token = "fake-token",
          Expiration = DateTime.UtcNow.AddHours(1),
          Email = request.Email,
          FullName = "Test User",
          UserId = Guid.CreateVersion7()
      });

        // Act

        var result = await _sut.Register(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }


}
