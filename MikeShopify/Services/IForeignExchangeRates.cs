using System;
using System.Threading.Tasks;

namespace MikeShopify.Services
{
    public interface IForeignExchangeRates
    {
        Task<decimal> GetRateAsync(DateTime time);
    }
}
