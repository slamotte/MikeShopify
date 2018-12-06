using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using MikeShopify.Pages.Models;
using MikeShopify.Services;
using MikeShopify.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.Extensions;

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
        private IStockPriceService StockPriceService { get; }
        private IForeignExchangeRates ForeignExchangeRates { get; }

        public async Task<decimal> AsCadAsync(decimal amount, DateTime time) =>
            amount * await ForeignExchangeRates.GetRateAsync(time);

        public async Task<string> FormatCadAsync(decimal amount, DateTime time)
        {
            var cad = amount * await ForeignExchangeRates.GetRateAsync(time);
            return $"{cad.FormatCurrency()} CAD";
        }

        public IndexModel(
            IOptions<AppSettings> settings,
            IStockPriceService stockPriceService,
            IForeignExchangeRates foreignExchangeRates)
        {
            Settings = settings.Value;
            StockPriceService = stockPriceService;
            ForeignExchangeRates = foreignExchangeRates;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            OriginalPrice = Settings.OriginalStockPrice;
            ShareCount = Settings.ShareCount;
            var price = await StockPriceService.GetCurrentPriceAsync(Settings.StockSymbol);
            if (price == null) return NotFound();
            CurrentPrice = price.Value;

            PriceHistory = await StockPriceService.GetPriceHistoryAsync(Settings.StockSymbol);
            if (PriceHistory == null) return NotFound();

            return Page();
        }
    }
}
