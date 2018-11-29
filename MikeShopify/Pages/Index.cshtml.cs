using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using MikeShopify.Pages.Models;
using MikeShopify.Services;
using MikeShopify.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MikeShopify.Pages
{
    public class IndexModel : PageModel
    {
        public decimal OriginalPrice { get; private set; }
        public decimal CurrentPrice { get; private set; }
        public int ShareCount { get; private set; }

        public IEnumerable<PriceHistory> PriceHistory { get; private set; }

        public decimal OriginalValue => OriginalPrice * ShareCount;
        public decimal CurrentValue => CurrentPrice * ShareCount;
        public decimal Difference => CurrentValue - OriginalValue;

        public decimal ValueDifference(PriceHistory price) =>
            price.Price * ShareCount - OriginalValue;

        private AppSettings Settings { get; }
        private StockPriceService Service { get; }

        public IndexModel(IOptions<AppSettings> settings, StockPriceService service)
        {
            Settings = settings.Value;
            Service = service;
        }

        public async Task<IActionResult> OnGet()
        {
            OriginalPrice = Settings.OriginalStockPrice;
            ShareCount = Settings.ShareCount;
            var price = await Service.GetCurrentAsync(Settings.StockSymbol);
            if (price == null) return NotFound();
            CurrentPrice = price.Value;

            PriceHistory = await Service.GetHistoryAsync(Settings.StockSymbol);
            if (PriceHistory == null) return NotFound();

            return Page();
        }
    }
}
