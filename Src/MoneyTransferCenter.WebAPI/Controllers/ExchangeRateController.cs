using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MoneyTransferCenter.Application.Dtos.ExchangeRate;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Domain.Constants;

namespace MoneyTransferCenter.WebAPI.Controllers;

[EnableRateLimiting(RateLimitPolicies.Standard)]
[Route("api/[controller]")]
[ApiController]
public class ExchangeRateController : ControllerBase
{
    private readonly IExchangeRateService _exchangeRateService;
    private readonly ILogger<ExchangeRateController> _logger;

    public ExchangeRateController(IExchangeRateService exchangeRateService, ILogger<ExchangeRateController> logger)
    {
        _exchangeRateService = exchangeRateService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentRates()
    {
        _logger.LogInformation("Döviz kurları endpoint'i çağrıldı.");
        List<ExchangeRateDto> rates = await _exchangeRateService.GetCurrentRatesAsync();
        return Ok(rates);
    }
}
