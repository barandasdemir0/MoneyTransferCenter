namespace MoneyTransferCenter.Application.Dtos.Transaction;

public sealed  record TransferResponseDto(
    string ReferenceNumber,
    string SenderIBAN,
    string ReceiverIBAN,
    decimal Amount,
    string? Description,
    string Status,
    DateTimeOffset TransactionDate
);
