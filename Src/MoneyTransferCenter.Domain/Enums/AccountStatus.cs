using Ardalis.SmartEnum;

namespace MoneyTransferCenter.Domain.Enums;

public sealed class AccountStatus:SmartEnum<AccountStatus>
{
    public static readonly AccountStatus Passive = new AccountStatus("Passive", 0);
    public static readonly AccountStatus Active = new AccountStatus("Active", 10);
    public static readonly AccountStatus Closed = new AccountStatus("Closed", 20);

    private AccountStatus(string name, int value) : base(name, value)
    {
    }

   
    public bool CanTransact()
    {
        // Kural: Sadece aktif hesaplar finansal işlem yapabilir.
        if (Value == Active.Value)
        {
            return true; 
        }
      
        return false;
    }

    public bool CanBeActivated()
    {
        // Kural 1: Sadece ilk defa açılmış olan hesaplar aktif edilebilir.
        if (Value == Passive.Value)
        {
            return true; 
        }
        return false; // Aktivasyon işlemini engelle.
    }

    public bool CanModify()
    {
        //Kapatılmış bir hesap üzerinde hiçbir değişiklik yapılamaz.
        if (Value == Closed.Value)
        {
            return false; // Hesap kapatılmış, güncellemeyi engelle.
        }
        
        return true; // Güncellemeye izin ver.
    }

  
}
