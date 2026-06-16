namespace MoneyTransferCenter.Domain.Interfaces;

public interface IIbanGenerator
{
    // TR formatında benzersiz IBAN üretir.
    // Format: TR + 2 check digit + 5 banka kodu + 1 rezerv + 16 hesap no
    Task<string> GenerateAsync();
}
