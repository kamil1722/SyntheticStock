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
                   // ���������� Worker
                   services.AddHostedService<Worker>();
                   // ���������� PostgreService
                   services.AddSingleton<IPostgreService, PostgreService>();
                   // ���������� DbContext
                   services.AddDbContext<DbContextSyntheticStock>(); // ���������� hostContext
               })
           .Build();

        await host.RunAsync();
    }
}