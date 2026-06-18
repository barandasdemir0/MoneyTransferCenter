namespace MoneyTransferCenter.Application.Dtos.Transaction.Deposit;

public sealed record DepositResponseDto(
    Guid AccountId,
    string IBAN,
    decimal NewBalance,
    decimal DepositedAmount,
    DateTimeOffset TransactionDate
);

