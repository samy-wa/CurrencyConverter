using CurrencyConverter.Model;
using CurrencyConverter.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyConverter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(ICurrencyService currencyService, ILogger<CurrencyController> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        [HttpGet("latest/{baseCurrency}")]
        [Authorize]
        public async Task<IActionResult> GetLatestRates(string baseCurrency)
        {
            var result = await _currencyService.GetLatestRatesAsync(baseCurrency);
            return Ok(result);
        }

        [HttpGet("convert")]
        [Authorize]
        public async Task<IActionResult> Convert([FromQuery] string from, [FromQuery] string to, [FromQuery] decimal amount)
        {
            try
            {
                var result = await _currencyService.ConvertAsync(from, to, amount);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("history/{baseCurrency}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetHistory(string baseCurrency, [FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _currencyService.GetHistoricalRatesAsync(baseCurrency, start, end, page, pageSize);
            return Ok(result);
        }

        [HttpGet("supported")]
        [Authorize]
        public async Task<IActionResult> GetSupportedCurrencies()
        {
            var result = await _currencyService.GetSupportedCurrenciesAsync();
            return Ok(result);
        }
    }

}
