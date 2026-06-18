using MoneyTransferCenter.Domain.Enums;

namespace MoneyTransferCenter.Application.Dtos.Transaction;

public sealed record TransactionHistoryRequestDto
{
    public TransactionHistoryFilter Filter { get; init; } = TransactionHistoryFilter.All;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
