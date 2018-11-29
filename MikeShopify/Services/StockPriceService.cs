using Microsoft.Extensions.Options;
using MikeShopify.Pages.Models;
using MikeShopify.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MikeShopify.Services
{
    public class StockPriceService
    {
        private const string ApiUrlBase = "https://www.alphavantage.co/query?function=";
        private const string TimeFrequency = "60min";

        private readonly string HistoryElementName = $"Time Series ({TimeFrequency})";

        private string CurrentUrl { get; }
        private string HistoryUrl { get; }

        public StockPriceService(IOptions<AppSettings> settings)
        {
            var apiKeyParam = $"&apikey={settings.Value.StockPriceApiKey}";
            CurrentUrl = $"{ApiUrlBase}GLOBAL_QUOTE{apiKeyParam}";
            HistoryUrl = $"{ApiUrlBase}TIME_SERIES_INTRADAY&interval={TimeFrequency}{apiKeyParam}";
        }

        public async Task<decimal?> GetCurrentAsync(string stockSymbol)
        {
            var url = $"{CurrentUrl}&symbol={stockSymbol}";
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var jsonObj = JObject.Parse(json);
            var result = jsonObj["Global Quote"]["05. price"].Value<decimal>();
            return result;
        }

        public async Task<IEnumerable<PriceHistory>> GetHistoryAsync(string stockSymbol)
        {
            var url = $"{HistoryUrl}&symbol={stockSymbol}";
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var jsonObj = JObject.Parse(json);
            var history = jsonObj[HistoryElementName].ToArray();
            var result = history
                .Select(i => new PriceHistory
                {
                    Time = DateTime.Parse(((JProperty)i).Name),
                    Price = i.First().Value<decimal>("4. close")
                })
                .OrderBy(i => i.Time);
            return result;
        }
    }
}
