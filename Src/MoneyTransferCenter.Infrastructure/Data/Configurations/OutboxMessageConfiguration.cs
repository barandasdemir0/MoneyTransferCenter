using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTransferCenter.Domain.Entities;

namespace MoneyTransferCenter.Infrastructure.Data.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(o => o.Id);

        // WHERE IsProcessed = false sorgusunu hızlı çalıştırır
        builder.HasIndex(o => o.IsProcessed);

        builder.Property(o => o.Type)
            .HasMaxLength(100).IsRequired(); 

        builder.Property(o => o.Payload)
            .IsRequired(); 

        builder.Property(o => o.LastError)
            .HasMaxLength(500);

    


        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}
