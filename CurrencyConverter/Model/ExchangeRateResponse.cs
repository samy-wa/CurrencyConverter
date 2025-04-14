using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Model
{
    public class ExchangeRateResponse 
    {
        public double amount { get; set; }
        public string @base { get; set; }
        public string date { get; set; }
        //public Rates rates { get; set; }
        public Dictionary<string, decimal> rates { get; set; }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);


}
