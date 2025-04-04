using Microsoft.AspNetCore.Mvc;
using FuturesService.Services.Interface;
using FuturesService.Models;
using Newtonsoft.Json;

namespace FuturesService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FuturesController : ControllerBase
    {
        private readonly IFuturesDataService _futuresDataService;
        private readonly ILogger<FuturesController> _logger;
        private readonly IRabbitMQService _rabbitMQService;

        public FuturesController(IFuturesDataService futuresDataService, ILogger<FuturesController> logger, IRabbitMQService rabbitMQService)
        {
            _futuresDataService = futuresDataService ?? throw new ArgumentNullException(nameof(futuresDataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rabbitMQService = rabbitMQService;
        }

        [HttpGet("GetPriceDifference")]
        public async Task<IActionResult> GetPriceDifference(
            [FromQuery] string symbol1,
            [FromQuery] string symbol2,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime,
            [FromQuery] string interval = "1h")
        {
            try
            {
                var klines1 = await _futuresDataService.GetHistoricalKlinesAsync(symbol1, interval, startTime, endTime);
                var klines2 = await _futuresDataService.GetHistoricalKlinesAsync(symbol2, interval, startTime, endTime);

                if (klines1 == null || klines2 == null)
                {
                    _logger.LogWarning($"No data found for symbols: {symbol1}, {symbol2} for the specified time period.");
                    return NotFound("No data found for the specified period.");
                }

                var priceDifferences = _futuresDataService.CalculatePriceDifference(klines1, klines2);

                if (priceDifferences == null || priceDifferences.Count == 0)
                {
                    _logger.LogWarning($"No price differences calculated for symbols: {symbol1}, {symbol2} for the specified time period.");
                    return NotFound("No price differences found for the specified period.");
                }

                var futuresDataList = _futuresDataService.GetFutures(priceDifferences, symbol1, symbol2, interval);

                _rabbitMQService.Publish(futuresDataList, "futures.exchange", "futures.data");
                _logger.LogInformation($"Published FuturesPriceDifference to RabbitMQ: Symbol1={symbol1}, Symbol2={symbol2}, Time={startTime} - {endTime}");

                return Ok(JsonConvert.SerializeObject(futuresDataList, Formatting.Indented));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the price difference request.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}