using Binance.Net.Enums;
using Binance.Net.Interfaces;
using FuturesService.Models;

namespace FuturesService.Services.Interface
{
    public interface IFuturesDataService
    {
        public Task<List<IBinanceKline>?> GetHistoricalKlinesAsync(string symbol, string interval, DateTime startTime, DateTime endTime);
        public List<PriceDifferenceResult> CalculatePriceDifference(List<IBinanceKline> klines1, List<IBinanceKline> klines2);
        public List<FuturesPriceDifference> GetFutures(List<PriceDifferenceResult> priceDifferences, string symbol1, string symbol2, string interval);
    }
}
