using MoneyTransferCenter.Application.Dtos.ExchangeRate;

namespace MoneyTransferCenter.Application.Interfaces;

public interface IExchangeRateService
{
   Task<List<ExchangeRateDto>> GetCurrentRatesAsync();
}
