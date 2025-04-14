using CurrencyConverter.Model;
using System.Net.Http;
using System.Text.Json;

namespace CurrencyConverter.Services
{
    public interface IExchangeRateProvider
    {
        Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency);
        Task<ExchangeRateResponse> ConvertAsync(string from, string to, decimal amount);
        Task<IEnumerable<ExchangeRateResponse>> GetHistoricalRatesAsync(string baseCurrency, DateTime start, DateTime end);
    }

    public class FrankfurterProvider : IExchangeRateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FrankfurterProvider> _logger;
        public FrankfurterProvider(HttpClient httpClient, ILogger<FrankfurterProvider> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public static List<string> ExcludedCurrency = ["TRY", "PLN", "THB", "MXN"];
        public async Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency)
        {
            var res = await _httpClient.GetStringAsync($"https://api.frankfurter.app/latest?base={baseCurrency}");
            return JsonSerializer.Deserialize<ExchangeRateResponse>(res)!;
        }

        public async Task<ExchangeRateResponse> ConvertAsync(string from, string to, decimal amount)
        {
            var res = await _httpClient.GetStringAsync($"https://api.frankfurter.app/latest?amount={amount}&from={from}&to={to}");
            return JsonSerializer.Deserialize<ExchangeRateResponse>(res)!;
        }

        public async Task<IEnumerable<ExchangeRateResponse>> GetHistoricalRatesAsync(string baseCurrency, DateTime start, DateTime end)
        {
            //var client = _httpClient.CreateClient("Frankfurter");
            var res = await _httpClient.GetStringAsync($"https://api.frankfurter.app/{start:yyyy-MM-dd}..{end:yyyy-MM-dd}?base={baseCurrency}");
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(res)!;
            var results = new List<ExchangeRateResponse>();
            foreach (var entry in dict["rates"].EnumerateObject())
            {
                var rates = JsonSerializer.Deserialize<Dictionary<string, decimal>>(entry.Value.ToString()!)!;
                results.Add(new ExchangeRateResponse
                {
                    @base = baseCurrency,
                    date = entry.Name,
                    rates = rates
                });
            }
            return results;
        }
    }
}
