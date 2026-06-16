namespace MoneyTransferCenter.Domain.Enums;

public enum TransactionStatus
{
    //işlemi bekleme
    Pending = 0,
    //işlem başarıyla tanımlanırsa
    Completed = 10,
    //işlem başarısız olursa
    Failed = 20,
    //işlem iptal olursa
    Canceled = 30
}
