using Moq;
using Binance.Net.Interfaces.Clients;
using Microsoft.Extensions.Logging;
using FuturesService.Services;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using FuturesService.Models;

namespace FuturesService.Tests
{
    [TestFixture]
    public class FuturesDataServiceTests
    {
        private FuturesDataService _service;
        private Mock<IBinanceRestClient> _mockBinanceClient;
        private Mock<ILogger<FuturesDataService>> _mockLogger;

        [SetUp]
        public void SetUp()
        {
            _mockBinanceClient = new Mock<IBinanceRestClient>();
            _mockLogger = new Mock<ILogger<FuturesDataService>>();
            _service = new FuturesDataService(_mockBinanceClient.Object, _mockLogger.Object);
        }

        [Test]
        public void ParseInterval_ShouldReturnCorrectEnum()
        {
            // можно передать массив
            var interval = "1m";

            // Act
            var result = _service.GetType().GetMethod("ParseInterval", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .Invoke(_service, [interval]);

            //проверка правильности возвращаемого значени€
            Assert.That(result, Is.EqualTo(KlineInterval.OneMinute));
        }

        [Test]
        public void ParseInterval_InvalidInput_ShouldThrowException()
        {
            // можно передать массив
            var invalidInterval = "invalid";

            //проверка исключени€
            Assert.Throws<System.Reflection.TargetInvocationException>(() =>
                _service.GetType().GetMethod("ParseInterval", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .Invoke(_service, [invalidInterval]));
        }

        [Test]
        public void CalculatePriceDifference_ShouldReturnCorrectDifferences()
        {
            // ћожно передать массив
            var timestamp = DateTime.UtcNow;
            var klines1 = new List<IBinanceKline>
            {
                Mock.Of<IBinanceKline>(k => k.OpenTime == timestamp && k.ClosePrice == 100)
            };
            var klines2 = new List<IBinanceKline>
            {
                Mock.Of<IBinanceKline>(k => k.OpenTime == timestamp && k.ClosePrice == 105)
            };

            // Act
            var result = _service.CalculatePriceDifference(klines1, klines2);

            //—колько записей вернул метод
            Assert.That(result, Has.Count.EqualTo(1));
            //¬ерность вычисленных значений разницы
            Assert.That(result[0].Difference, Is.EqualTo(5));
        }

        [Test]
        public void GetFutures_ShouldReturnCorrectData()
        {
            // можно передать массив
            var priceDifferences = new List<PriceDifferenceResult>
            {
                new PriceDifferenceResult
                {
                    Price1 = 100,
                    Price2 = 105,
                    Time = DateTime.UtcNow,
                    Difference = 5
                }
            };

            // Act
            var result = _service.GetFutures(priceDifferences, "BTCUSDT", "ETHUSDT", "1m");

            // оличество элементов в возвращЄнном списке
            Assert.That(result.Count, Is.EqualTo(1));
            //“очность значений символов, цен, интервала и разницы
            Assert.That(result[0].symbol1, Is.EqualTo("BTCUSDT"));
            Assert.That(result[0].symbol2, Is.EqualTo("ETHUSDT"));
        }
    }
}
