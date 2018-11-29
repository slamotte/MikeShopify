using Microsoft.Extensions.Options;
using MikeShopify.Settings;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MikeShopify.Services
{
    public class StockPriceService
    {
        private const string UrlBase = "https://www.alphavantage.co/query?function=GLOBAL_QUOTE";

        private string Url { get; }

        public StockPriceService(IOptions<AppSettings> settings)
        {
            Url = $"{UrlBase}&apikey={settings.Value.StockPriceApiKey}";
        }

        public async Task<decimal?> GetAsync(string stockSymbol)
        {
            var url = $"{Url}&symbol={stockSymbol}";
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(json);
            var result = obj["Global Quote"]["05. price"].Value<decimal>();
            return result;
        }
    }
}
