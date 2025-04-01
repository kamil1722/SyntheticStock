using Microsoft.EntityFrameworkCore;
using DataWorkService.Data;
using DataWorkService;

public class Program
{
    public static async Task Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
               .ConfigureServices((hostContext, services) => // Use hostContext for configuration
               {
                   // Добавление Worker
                   services.AddHostedService<Worker>();
                   // Добавление DbContext
                   services.AddDbContext<DbContextSyntheticStock>(options =>
                       options.UseNpgsql(hostContext.Configuration.GetConnectionString("DefaultConnection"))); // Используем hostContext
               })
           .Build();

        await host.RunAsync();
    }
}