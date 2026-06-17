using MoneyTransferCenter.Domain.Common;
using MoneyTransferCenter.Domain.Enums;
using MoneyTransferCenter.Domain.Exceptions;

namespace MoneyTransferCenter.Domain.Entities;

public class Account : BaseEntity
{
    public Guid UserId { get; private init; }
    public string IBAN { get; private init; } = string.Empty;

    public decimal Balance { get; private set; }
    public AccountStatus Status { get; private set; } = AccountStatus.Passive;

    // Profil bilgileri (aktivasyon için gerekli)
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? PostalCode { get; private set; }
    public string? TelephoneNumber { get; private set; }
    public DateTimeOffset? DateOfBirth { get; private set; }


    public AppUser? User { get; set; }
    public ICollection<Transaction> SentTransactions { get; set; } = new List<Transaction>();
    public ICollection<Transaction> ReceivedTransactions { get; set; } = new List<Transaction>();


    // bunun amacı, EF Core'un optimistik eşzamanlılık kontrolü yapabilmesi için bir sütun sağlamaktır.
    public byte[] RowVersion { get; set; } = [];


    private Account() { }
    // Yeni hesap oluşturma constructor'ı
    public Account(Guid userId, string iban)
    {
        UserId = userId;
        IBAN = iban;
        Balance = 0;
        Status = AccountStatus.Passive;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("Tutar sıfırdan büyük olmalıdır.", "INVALID_AMOUNT");

        }
        if (Status != AccountStatus.Active)
        {
            throw new DomainException("Aktif olmayan hesaba para yatırılamaz.", "ACCOUNT_NOT_ACTIVE");
        }
        Balance += amount;
    }


    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("Tutar sıfırdan büyük olmalıdır.", "INVALID_AMOUNT");
        }


        if (Status != AccountStatus.Active)
        {
            throw new DomainException("Aktif olmayan hesaptan para çekilemez.", "ACCOUNT_NOT_ACTIVE");
        }

        if (Balance < amount)
        {
            throw new DomainException($"Yetersiz bakiye. Mevcut: {Balance}, İstenen: {amount}", "INSUFFICIENT_BALANCE");
        }

        Balance -= amount;
    }

    public void CompleteProfile(string address, string city, string postalCode,
    string telephoneNumber, DateTimeOffset dateOfBirth)
    {
        if (Status == AccountStatus.Closed)
        {
            throw new DomainException("Kapatılmış hesap güncellenemez.", "ACCOUNT_CLOSED");
        }
        Address = address;
        City = city;
        PostalCode = postalCode;
        TelephoneNumber = telephoneNumber;
        DateOfBirth = dateOfBirth;
        Status = AccountStatus.Active;
    }

    
    public void UpdateProfile(string address, string city, string postalCode, string telephoneNumber)
    {
        if (Status.CanModify() == false)
        {
            throw new DomainException("Kapatılmış hesap üzerinde değişiklik yapılamaz.", "ACCOUNT_CLOSED");
        }
        Address = address;
        City = city;
        PostalCode = postalCode;
        TelephoneNumber = telephoneNumber;
    }
    public bool IsProfileComplete()
    {
        return string.IsNullOrEmpty(Address) == false
            && string.IsNullOrEmpty(City) == false
            && string.IsNullOrEmpty(PostalCode) == false
            && string.IsNullOrEmpty(TelephoneNumber) == false;
    }

    public void Activate()
    {
        if (Status.CanBeActivated() == false)
        {
            throw new DomainException("Hesap aktif edilemez.", "ACCOUNT_CANNOT_BE_ACTIVATED");
        }
        Status = AccountStatus.Active;
    }
    public void Close()
    {
        if (Balance != 0)
        {
            throw new DomainException("Bakiyesi olan hesap kapatılamaz.", "BALANCE_NOT_ZERO");
        }

        Status = AccountStatus.Closed;
    }


}
