using Ardalis.SmartEnum;

namespace MoneyTransferCenter.Domain.Enums;

public sealed class TransactionStatus : SmartEnum<TransactionStatus>
{
    //beklemede
    public static readonly TransactionStatus Pending = new TransactionStatus("Pending", 0);

    // başarı durumu
    public static readonly TransactionStatus Completed = new TransactionStatus("Completed", 10);

    // hata durumu
    public static readonly TransactionStatus Failed = new TransactionStatus("Failed", 20);

    //iptal durumu
    public static readonly TransactionStatus Canceled = new TransactionStatus("Canceled", 30);

    private TransactionStatus(string name, int value) : base(name, value)
    {
    }


    public bool CanTransitionTo(TransactionStatus nextStatus)
    {
        // işlem başarıyla sonuçlandıysa duruumu bir daha asla değiştirilemez.
        if (Value == Completed.Value)
        {
            return false; 
        }
        // Eğer işlem başarısız olduysa durumu kilitlenir. 
        if (Value == Failed.Value)
        {
            return false; 
        }
        // işlem iptal edildiyse durumu kilitlenir.
        if (Value == Canceled.Value)
        {
            return false;
        }
        // Eğer işlem şu an Beklemede ise iş akışına göre herhangi bir duruma geçebilir.
        if (Value == Pending.Value)
        {
            return true;
        }
        // Yukarıdaki senaryolara uymayan herhangi bir durum varsa engelle.
        return false;
    }
}
