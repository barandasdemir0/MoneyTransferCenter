using Microsoft.EntityFrameworkCore;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Enums;

namespace MoneyTransferCenter.Infrastructure.Data.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);

        builder.ToTable("Accounts");

        builder.Property(a => a.IBAN)
         .HasMaxLength(26).IsRequired(); // TR IBAN her zaman 26 karakter

        builder.Property(a => a.Balance)
            .HasPrecision(18, 2); // 18 basamak, 2 ondalık (para için standart)

        builder.Property(a => a.Status)
                    .HasConversion(status => status.Value, //dbye int olarak kaydet
                                   value => AccountStatus.FromValue(value));  //çıkarken string olarak al

        builder.Property(a => a.Address)
            .HasMaxLength(250);

        builder.Property(a => a.City)
            .HasMaxLength(50);

        builder.Property(a => a.PostalCode)
            .HasMaxLength(10);

        builder.Property(a => a.TelephoneNumber)
            .HasMaxLength(15);

        //iki kullanıcı aynı anda para transferi yapmaya çalışırsa RowVersion sayesinde çakışmayı önler
        builder.Property(a => a.RowVersion)
            .IsRowVersion();

        builder.HasIndex(a => a.IBAN)
            .IsUnique(); // Aynı IBAN iki hesapta olamaz

        builder.HasIndex(a => a.UserId)
            .IsUnique(); // Her kullanıcının sadece 1 hesabı olur

        // AppUser <-> Account : 1'e 1 ilişki
        builder.HasOne(a => a.User)
            .WithOne(u => u.Account)
            .HasForeignKey<Account>(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
       
        
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
