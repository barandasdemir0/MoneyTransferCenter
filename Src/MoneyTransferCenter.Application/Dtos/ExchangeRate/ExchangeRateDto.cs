namespace MoneyTransferCenter.Application.Dtos.ExchangeRate;

public sealed record ExchangeRateDto
(
    string CurrencyCode,   // Döviz kodu 
    string CurrencyName,   // Döviz adı 
    decimal ForexBuying,   // Döviz alış fiyatı
    decimal ForexSelling   // Döviz satış fiyatı
);
