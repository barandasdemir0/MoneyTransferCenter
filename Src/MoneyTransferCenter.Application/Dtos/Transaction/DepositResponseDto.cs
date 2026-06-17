namespace MoneyTransferCenter.Application.Dtos.Transaction;

public sealed record DepositResponseDto(
    Guid AccountId,
    string IBAN,
    decimal NewBalance,
    decimal DepositedAmount,
    DateTimeOffset TransactionDate
);

