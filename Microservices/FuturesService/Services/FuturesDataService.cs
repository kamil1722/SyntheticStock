using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using CryptoExchange.Net.CommonObjects;
using FuturesService.Models;
using FuturesService.Services.Interface;
using Microsoft.Extensions.Logging;

namespace FuturesService.Services
{
    public class FuturesDataService : IFuturesDataService
    {
        private readonly IBinanceRestClient _binanceClient;
        private readonly ILogger<FuturesDataService> _logger;

        public FuturesDataService(IBinanceRestClient binanceClient, ILogger<FuturesDataService> logger)
        {
            _binanceClient = binanceClient ?? throw new ArgumentNullException(nameof(binanceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private KlineInterval ParseInterval(string interval)
        {
            switch (interval.ToLower())
            {
                case "1m": return KlineInterval.OneMinute;
                case "5m": return KlineInterval.FiveMinutes;
                case "15m": return KlineInterval.FifteenMinutes;
                case "30m": return KlineInterval.ThirtyMinutes;
                case "1h": return KlineInterval.OneHour;
                case "4h": return KlineInterval.FourHour;
                case "1d": return KlineInterval.OneDay;
                case "1w": return KlineInterval.OneWeek;
                case "1M": return KlineInterval.OneMonth;
                default: throw new ArgumentException($"Неподдерживаемый интервал: {interval}");
            }
        }

        public async Task<List<IBinanceKline>?> GetHistoricalKlinesAsync(string symbol, string interval, DateTime startTime, DateTime endTime)
        {
            try
            {
                var klineInterval = ParseInterval(interval);
                var klinesResult = await _binanceClient.SpotApi.ExchangeData.GetKlinesAsync(symbol, klineInterval, startTime, endTime);

                if (!klinesResult.Success)
                {
                    _logger.LogError($"Ошибка при получении Klines для {symbol}: {klinesResult.Error}");
                    return null;
                }

                return klinesResult.Data.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Исключение при получении Klines для {symbol}: {ex}");
                return null;
            }
        }

        public List<PriceDifferenceResult> CalculatePriceDifference(List<IBinanceKline> klines1, List<IBinanceKline> klines2)
        {
            if (klines1 == null || klines2 == null)
            {
                return new ();
            }

            var prices1 = klines1.ToDictionary(k => k.OpenTime, k => k.ClosePrice);
            var prices2 = klines2.ToDictionary(k => k.OpenTime, k => k.ClosePrice);

            // Получить все уникальные временные метки из обеих линий
            var allTimestamps = prices1.Keys.Union(prices2.Keys).OrderBy(t => t).ToList();

            var results = new List<PriceDifferenceResult>();

            decimal? lastPrice1 = null;
            decimal? lastPrice2 = null;

            foreach (var timestamp in allTimestamps)
            {
                var price1 = prices1.ContainsKey(timestamp) ? prices1[timestamp] : lastPrice1;
                var price2 = prices2.ContainsKey(timestamp) ? prices2[timestamp] : lastPrice2;

                if (price1.HasValue)
                {
                    lastPrice1 = price1;
                }

                if (price2.HasValue)
                {
                    lastPrice2 = price2;
                }

                if (lastPrice1.HasValue && lastPrice2.HasValue)
                {
                    var diff = Math.Abs(lastPrice1.Value - lastPrice2.Value);
                    results.Add(new PriceDifferenceResult { Time = timestamp, Difference = diff });
                }
            }

            return results;
        }

        public List<FuturesPriceDifference> GetFutures(List<PriceDifferenceResult> priceDifferences, string symbol1, string symbol2, string interval)
        {

            var futuresDataList = new List<FuturesPriceDifference>();

            foreach (var diff in priceDifferences)
            {
                futuresDataList.Add(new FuturesPriceDifference
                {
                    symbol1 = symbol1,
                    symbol2 = symbol2,
                    time = diff.Time,
                    difference = diff.Difference,
                    interval = interval
                });
            }

            return futuresDataList;
        }
    }
}