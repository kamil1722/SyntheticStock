using Binance.Net.Enums;
using Binance.Net.Interfaces;
using FuturesService.Models;

namespace FuturesService.Services.Interface
{
    public interface IFuturesDataService
    {
        public Task<List<IBinanceKline>?> GetHistoricalKlinesAsync(string symbol, string interval, DateTime startTime, DateTime endTime);
        public List<PriceDifferenceResult> CalculatePriceDifference(List<IBinanceKline> klines1, List<IBinanceKline> klines2);
    }
}
