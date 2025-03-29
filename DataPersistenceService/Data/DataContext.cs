using DataWorkService.Models;
using Microsoft.EntityFrameworkCore;

namespace DataWorkService.Data
{
    public class DbContextSyntheticStock : DbContext
    {
        public DbContextSyntheticStock(DbContextOptions<DbContextSyntheticStock> options) : base(options)
        {
        }

        public DbSet<FuturesPriceDifference> FuturesPriceDifferences { get; set; } = null!;

        // сюда добавляем необходимые модели таблиц
    }
}
