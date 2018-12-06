using MikeShopify.Pages.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MikeShopify.Services
{
    public class ForeignExchangeRates : IForeignExchangeRates
    {
        private IStockPriceService Service { get; }

        private List<ForeignExchangeRate> Rates { get; set; }

        public ForeignExchangeRates(IStockPriceService service)
        {
            Service = service;
        }

        public async Task<decimal> GetRateAsync(DateTime time)
        {
            // Lazy load list of rates
            if (Rates == null)
                Rates = (await Service.GetForeignExchangeHistoryAsync())
                    .OrderByDescending(i => i.Time)
                    .ToList();

            // Return rate in effect as of given time
            var result = Rates
                .Where(i => i.Time <= time)
                .Select(i => i.Rate)
                .First();
            return result;
        }
    }
}
