namespace MoneyTransferCenter.Application.Dtos.Transaction.WithDraw;

public sealed record WithdrawResponseDto(
    Guid AccountId,
    string IBAN,
    decimal NewBalance,
    decimal WithdrawnAmount,
    DateTimeOffset TransactionDate
);
