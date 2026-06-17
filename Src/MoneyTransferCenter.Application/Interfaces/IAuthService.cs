using MoneyTransferCenter.Application.Dtos.Auth;

namespace MoneyTransferCenter.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponse> LoginAsync(LoginRequestDto request);
}
