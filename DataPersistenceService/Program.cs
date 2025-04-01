using Microsoft.EntityFrameworkCore;
using DataWorkService.Data;
using DataWorkService;
using DataWorkService.Service;

public class Program
{
    public static async Task Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
               .ConfigureServices((hostContext, services) => // Use hostContext for configuration
               {
                   // Добавление Worker
                   services.AddHostedService<Worker>();
                   // Добавление PostgreService
                   services.AddSingleton<IPostgreService, PostgreService>();
                   // Добавление DbContext
                   services.AddDbContext<DbContextSyntheticStock>(); // Используем hostContext
               })
           .Build();

        await host.RunAsync();
    }
}