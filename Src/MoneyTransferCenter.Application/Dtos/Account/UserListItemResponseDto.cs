namespace MoneyTransferCenter.Application.Dtos.Account;

public record UserListItemResponseDto(
    Guid UserId,
    string FullName,
    string Email,
    string? IBAN,
    string? AccountStatus,
    DateTimeOffset CreatedAt
);
