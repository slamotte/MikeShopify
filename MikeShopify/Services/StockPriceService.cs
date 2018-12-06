using Microsoft.Extensions.Caching.Memory;
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
    public class StockPriceService : IStockPriceService
    {
        private const string ApiUrlBase = "https://www.alphavantage.co/query?function=";
        private const string TimeFrequency = "30min";

        private readonly string _historyElementName = $"Time Series ({TimeFrequency})";
        private readonly DateTime _earliestHistoryTime = DateTime.Now - TimeSpan.FromDays(180);

        private string CurrentUrl { get; }
        private string HistoryUrl { get; }
        private string ForExUrl { get; }

        private IMemoryCache Cache { get; }
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(3);

        public StockPriceService(IOptions<AppSettings> settings, IMemoryCache cache)
        {
            Cache = cache;

            var apiKeyParam = $"apikey={settings.Value.StockPriceApiKey}";
            CurrentUrl = $"{ApiUrlBase}GLOBAL_QUOTE&{apiKeyParam}";
            HistoryUrl = $"{ApiUrlBase}TIME_SERIES_INTRADAY&outputsize=full&interval={TimeFrequency}&{apiKeyParam}";
            ForExUrl = $"{ApiUrlBase}FX_DAILY&from_symbol=USD&to_symbol=CAD&outputsize=full&{apiKeyParam}";
        }

        #region Stock Prices

        public async Task<decimal?> GetCurrentPriceAsync(string stockSymbol)
        {
            var url = $"{CurrentUrl}&symbol={stockSymbol}";

            // Check for and return cached value
            var cachedValue = GetCachedValue<decimal?>(url);
            if (cachedValue.HasValue) return cachedValue;

            var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var jsonObj = JObject.Parse(json);
            var result = jsonObj["Global Quote"]["05. price"].Value<decimal>();

            // Cache the result
            SetCachedValue(url, result);

            return result;
        }

        public async Task<IEnumerable<PriceHistory>> GetPriceHistoryAsync(string stockSymbol)
        {
            var url = $"{HistoryUrl}&symbol={stockSymbol}";

            // Check for and return cached value
            var cachedValue = GetCachedValue<IEnumerable<PriceHistory>>(url);
            if (cachedValue != null) return cachedValue;

            var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var jsonObj = JObject.Parse(json);
            var history = jsonObj[_historyElementName].ToArray();
            var result = history
                .Select(i => new PriceHistory
                {
                    Time = DateTime.Parse(((JProperty)i).Name),
                    Price = i.First().Value<decimal>("4. close")
                })
                .Where(i => i.Time > _earliestHistoryTime)
                .OrderBy(i => i.Time);

            // Cache the result
            SetCachedValue(url, result);

            return result;
        }

        #endregion

        #region Foreign Exchange

        public async Task<IEnumerable<ForeignExchangeRate>> GetForeignExchangeHistoryAsync()
        {
            const string elementName = "Time Series FX (Daily)";
            const string cacheKey = "ForEx";

            // Check for and return cached value
            var cachedValue = GetCachedValue<IEnumerable<ForeignExchangeRate>>(cacheKey);
            if (cachedValue != null) return cachedValue;

            var client = new HttpClient();
            var response = await client.GetAsync(ForExUrl);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var jsonObj = JObject.Parse(json);
            var history = jsonObj[elementName].ToArray();
            var result = history
                .Select(i => new ForeignExchangeRate
                {
                    Time = DateTime.Parse(((JProperty)i).Name),
                    Rate = i.First().Value<decimal>("4. close")
                })
                .Where(i => i.Time > _earliestHistoryTime)
                .OrderBy(i => i.Time);

            // Cache the result
            SetCachedValue(cacheKey, result);

            return result;
        }

        #endregion

        #region Cache Access

        private T GetCachedValue<T>(string key) =>
            Cache.TryGetValue(key, out T cachedValue) ? cachedValue : default(T);

        private void SetCachedValue(string key, object value) =>
            Cache.Set(key, value, CacheLifetime);

        #endregion
    }
}
