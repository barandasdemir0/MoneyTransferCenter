using MoneyTransferCenter.Domain.Common;

namespace MoneyTransferCenter.Domain.Entities;

public sealed class OutboxMessage:BaseEntity
{
    public string Type { get; private init; } = string.Empty; //eventtype transfermi account activetedmi gibisinden

    // Event verisi (JSON)
    public string Payload { get; private init; } = string.Empty;

    // İşlendi mi?
    public bool IsProcessed { get; private set; }

    // İşlenme zamanı
    public DateTimeOffset? ProcessedAt { get; private set; }

    // Hata durumunda retry sayacı
    public int RetryCount { get; private set; }

    // Son hata mesajı
    public string? LastError { get; private set; }


    // EF Core için
    private OutboxMessage() { }

    public static OutboxMessage Create(string type, string payload)
    {
        return new OutboxMessage
        {
            Type = type,
            Payload = payload,
            IsProcessed = false,
            RetryCount = 0
        };
    }

    public void MarkAsProcessed()
    {
        IsProcessed = true;
        ProcessedAt = DateTimeOffset.UtcNow;
    }
    public void RecordFailure(string error)
    {
        RetryCount++;
        LastError = error;
    }
}
