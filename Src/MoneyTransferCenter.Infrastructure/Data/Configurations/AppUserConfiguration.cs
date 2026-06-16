using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTransferCenter.Domain.Entities;

namespace MoneyTransferCenter.Infrastructure.Data.Configurations;

public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
  
        builder.Property(u => u.FirstName)
            .HasMaxLength(50).IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(50).IsRequired();

        builder.Property(u => u.NationalId)
            .HasMaxLength(11).IsRequired(); 

        builder.HasIndex(u => u.NationalId)
            .IsUnique(); 

        // FullName kolonu oluşturma
        builder.Ignore(u => u.FullName);
    }
}
