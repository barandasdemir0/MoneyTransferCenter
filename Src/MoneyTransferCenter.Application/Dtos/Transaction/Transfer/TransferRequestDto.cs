namespace MoneyTransferCenter.Application.Dtos.Transaction.Transfer;

public record TransferRequestDto
{
    public string ReceiverIBAN { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? Description { get; init; }
}
