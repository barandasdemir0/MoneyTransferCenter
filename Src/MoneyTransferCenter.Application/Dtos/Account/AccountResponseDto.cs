namespace MoneyTransferCenter.Application.Dtos.Account;

public record AccountResponseDto(
    Guid Id,
    string IBAN,
    decimal Balance,
    string Status,
    string? Address,
    string? City,
    string? PostalCode,
    string? TelephoneNumber,
    string OwnerFullName,
    string OwnerEmail,
    DateTimeOffset CreatedAt
);

