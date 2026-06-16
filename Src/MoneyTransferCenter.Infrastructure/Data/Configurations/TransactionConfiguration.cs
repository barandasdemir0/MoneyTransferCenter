using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Enums;

namespace MoneyTransferCenter.Infrastructure.Data.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .HasPrecision(18, 2);

        builder.Property(t => t.ReferenceNumber)
            .HasMaxLength(30).IsRequired(); // TRF-20260616-A1B2C3D4 formatı (~24 karakter)

        builder.HasIndex(t => t.ReferenceNumber)
           .IsUnique();

        builder.Property(t => t.Status)
                   .HasConversion(status => status.Value, //dbye int olarak kaydet
                                  value => TransactionStatus.FromValue(value));  //çıkarken string olarak al

        builder.Property(t => t.Description)
            .HasMaxLength(250);

        builder.Property(t => t.FailureReason)
            .HasMaxLength(500);

        builder.HasIndex(t => t.SenderAccountId);

        builder.HasIndex(t => t.ReceiverAccountId);

        // Account (Gönderen) -> Transaction : 1'e Çok
        builder.HasOne(t => t.SenderAccount)
            .WithMany(a => a.SentTransactions)
            .HasForeignKey(t => t.SenderAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Account (Alıcı) -> Transaction : 1'e Çok
        builder.HasOne(t => t.ReceiverAccount)
            .WithMany(a => a.ReceivedTransactions)
            .HasForeignKey(t => t.ReceiverAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
