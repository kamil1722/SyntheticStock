using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using FuturesService.Models;
using FuturesService.Services.Interface;

namespace FuturesService.Services
{
    public class FuturesDataService : IFuturesDataService
    {
        private readonly IBinanceRestClient _binanceClient;

        public FuturesDataService(IBinanceRestClient binanceClient)
        {
            _binanceClient = binanceClient ?? throw new ArgumentNullException(nameof(binanceClient));
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
                var klineInterval = ParseInterval(interval); // Используем ParseInterval
                var klinesResult = await _binanceClient.SpotApi.ExchangeData.GetKlinesAsync(symbol, klineInterval, startTime, endTime);

                if (!klinesResult.Success)
                {
                    Console.WriteLine($"Ошибка при получении Klines для {symbol}: {klinesResult.Error}");
                    return null;
                }

                return klinesResult.Data.ToList();  // Changed to IBinanceKline and .ToList()
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение при получении Klines для {symbol}: {ex}");
                return null;
            }
        }

        public List<PriceDifferenceResult> CalculatePriceDifference(List<IBinanceKline> klines1, List<IBinanceKline> klines2)
        {
            if (klines1 == null || klines2 == null)
            {
                return new ();
            }

            var prices1 = klines1.ToDictionary(k => k.OpenTime, k => k.ClosePrice); // Обработка null и ClosePrice
            var prices2 = klines2.ToDictionary(k => k.OpenTime, k => k.ClosePrice); // Обработка null и ClosePrice

            var commonTimestamps = prices1.Keys.Intersect(prices2.Keys).ToList();
            var results = new List<PriceDifferenceResult>();

            foreach (var timestamp in commonTimestamps)
            {
                var diff = prices1[timestamp] - prices2[timestamp];
                results.Add(new () { Time = timestamp, Difference = diff });
            }

            return results;
        }
    }
}