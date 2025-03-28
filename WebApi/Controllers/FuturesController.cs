using Microsoft.AspNetCore.Mvc;
using FuturesService.Services;

namespace FuturesService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FuturesController : ControllerBase
    {
        private readonly FuturesDataService _futuresDataService;
        //private readonly RabbitMQPublisher _rabbitMQPublisher;
        private readonly ILogger<FuturesController> _logger;

        public FuturesController(
            FuturesDataService futuresDataService,
            //RabbitMQPublisher rabbitMQPublisher,
            ILogger<FuturesController> logger)
        {
            _futuresDataService = futuresDataService ?? throw new ArgumentNullException(nameof(futuresDataService));
            //_rabbitMQPublisher = rabbitMQPublisher ?? throw new ArgumentNullException(nameof(rabbitMQPublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("price_difference")]
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

                //foreach (var priceDifference in priceDifferences)
                //{
                //    _rabbitMQPublisher.Publish(priceDifference);
                //}

                _logger.LogInformation($"Published {priceDifferences.Count} price differences to RabbitMQ.");
                return Ok(priceDifferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the price difference request.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}