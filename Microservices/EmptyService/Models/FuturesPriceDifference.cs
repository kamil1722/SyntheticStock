using System.ComponentModel.DataAnnotations;

namespace FuturesService.Models
{
    public class FuturesPriceDifference
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Symbol1 { get; set; } = null!; // Добавляем nullable suppression operator

        [Required]
        public string Symbol2 { get; set; } = null!; // Добавляем nullable suppression operator

        [Required]
        public DateTime Time { get; set; }

        public decimal Difference { get; set; }

        [Required]
        public string Interval { get; set; } = null!; // Добавляем nullable suppression operator
    }
}