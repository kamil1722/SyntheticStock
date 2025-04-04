using System.ComponentModel.DataAnnotations;

namespace DataWorkService.Models
{
    public class FuturesPriceDifference
    {
        [Key]
        public int id { get; set; }

        [Required]
        public string symbol1 { get; set; } = null!; // Добавляем nullable suppression operator

        [Required]
        public string symbol2 { get; set; } = null!; // Добавляем nullable suppression operator

        [Required]
        public decimal price1 { get; set; }

        [Required]
        public decimal price2 { get; set; }

        [Required]
        public DateTime time { get; set; }

        public decimal difference { get; set; }

        [Required]
        public string interval { get; set; } = null!; // Добавляем nullable suppression operator
    }
}