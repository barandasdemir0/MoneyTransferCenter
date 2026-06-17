namespace MoneyTransferCenter.Application.Dtos.Transaction;

public sealed record DepositRequestDto
{
    public decimal Amount { get; init; }

};
