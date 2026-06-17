namespace MoneyTransferCenter.Application.Dtos.Auth;

public sealed record AuthResponse
{

    public string Token { get; init; } = string.Empty;
    public DateTime Expiration { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public Guid UserId { get; init; }
}
