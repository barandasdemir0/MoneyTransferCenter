namespace MoneyTransferCenter.Application.Dtos.Transaction.Deposit;

public sealed record DepositRequestDto
{
    public decimal Amount { get; init; }

};
