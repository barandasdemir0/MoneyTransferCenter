using MoneyTransferCenter.Domain.Common;
using MoneyTransferCenter.Domain.Enums;

namespace MoneyTransferCenter.Domain.Entities;

public sealed class Transaction:BaseEntity
{
    public Guid SenderAccountId { get; private init; } //gönderici ıd 
    public Guid ReceiverAccountId { get; private init; } //alıcı ıd
    public decimal Amount { get; private init; } //tutar
    public string? Description { get; private init; } //açıklama
    public string ReferenceNumber { get; private init; } = string.Empty; // Benzersiz transfer referans numarası
    public TransactionStatus Status { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? FailureReason { get; private set; } //hata ise sebebi

    // Navigation properties
    public Account? SenderAccount { get; set; }
    public Account? ReceiverAccount { get; set; }

    // EF Core için listeleme
    private Transaction() { }

    // Factory method: Yeni transfer oluşturur
    public static Transaction Create(Guid senderAccountId, Guid receiverAccountId, decimal amount, string? description = null)
    {
        return new Transaction
        {
            SenderAccountId = senderAccountId,
            ReceiverAccountId = receiverAccountId,
            Amount = amount,
            Description = description,
            ReferenceNumber = $"TRF-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.CreateVersion7().ToString("N")[..8].ToUpper()}",
            Status = TransactionStatus.Pending
        };
    }

    public void MarkAsCompleted()
    {
        Status = TransactionStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }
    public void MarkAsFailed(string reason)
    {
        Status = TransactionStatus.Failed;
        FailureReason = reason;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
