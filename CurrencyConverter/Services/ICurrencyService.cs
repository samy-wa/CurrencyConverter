using CurrencyConverter.Model;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyConverter.Services
{
    public interface ICurrencyService
    {
        Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency);
        Task<ExchangeRateResponse> ConvertAsync(string from, string to, decimal amount);
        Task<IEnumerable<ExchangeRateResponse>> GetHistoricalRatesAsync(string baseCurrency, DateTime start, DateTime end, int page, int pageSize);
        Task<IEnumerable<string>> GetSupportedCurrenciesAsync();
    }


    public class CurrencyService : ICurrencyService
    {
        private static readonly string[] BlockedCurrencies = ["TRY", "PLN", "THB", "MXN"];
        private readonly IExchangeRateProvider _provider;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CurrencyService> _logger;

        public CurrencyService(IExchangeRateProvider provider, IMemoryCache cache, ILogger<CurrencyService> logger)
        {
            _provider = provider;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency)
        {
            string cacheKey = $"latest_{baseCurrency}";
            if (!_cache.TryGetValue(cacheKey, out ExchangeRateResponse result))
            {
                result = await _provider.GetLatestRatesAsync(baseCurrency);
                result.rates = result.rates.Where(kvp => !BlockedCurrencies.Contains(kvp.Key)).ToDictionary(k => k.Key, v => v.Value);
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
            }
            return result;
        }

        public async Task<ExchangeRateResponse> ConvertAsync(string from, string to, decimal amount)
        {
            if (BlockedCurrencies.Contains(from) || BlockedCurrencies.Contains(to))
                throw new ArgumentException("Conversion for specified currencies is not allowed.");
            return await _provider.ConvertAsync(from, to, amount);
        }

        public async Task<IEnumerable<ExchangeRateResponse>> GetHistoricalRatesAsync(string baseCurrency, DateTime start, DateTime end, int page, int pageSize)
        {
            var all = await _provider.GetHistoricalRatesAsync(baseCurrency, start, end);
            return all.Skip((page - 1) * pageSize).Take(pageSize);
        }

        public async Task<IEnumerable<string>> GetSupportedCurrenciesAsync()
        {
            var latest = await _provider.GetLatestRatesAsync("USD");
            return latest.rates.Keys.Where(k => !BlockedCurrencies.Contains(k));
        }
    }
}
