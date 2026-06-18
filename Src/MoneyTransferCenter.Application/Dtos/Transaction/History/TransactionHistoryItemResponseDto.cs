namespace MoneyTransferCenter.Application.Dtos.Transaction.History;

public sealed record TransactionHistoryItemResponseDto(
    Guid TransactionId,
    string ReferenceNumber,
    string SenderIBAN,
    string SenderFullName,
    string ReceiverIBAN,
    string ReceiverFullName,
    decimal Amount,
    string Status,  
    string? Description,
    DateTimeOffset CreatedAt
);

