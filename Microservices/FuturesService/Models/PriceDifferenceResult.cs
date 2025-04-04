
namespace FuturesService.Models
{
    public class PriceDifferenceResult
    {
        public decimal Price1 { get; set; }
        public decimal Price2 { get; set; }
        public DateTime Time { get; set; }
        public decimal Difference { get; set; }
    }
}