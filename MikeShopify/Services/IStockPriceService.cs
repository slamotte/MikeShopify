using MikeShopify.Pages.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MikeShopify.Services
{
    public interface IStockPriceService
    {
        Task<decimal?> GetCurrentPriceAsync(string stockSymbol);
        Task<IEnumerable<PriceHistory>> GetPriceHistoryAsync(string stockSymbol);

        Task<IEnumerable<ForeignExchangeRate>> GetForeignExchangeHistoryAsync();
    }
}
