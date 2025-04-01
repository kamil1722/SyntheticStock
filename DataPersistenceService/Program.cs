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
                   // ���������� Worker
                   services.AddHostedService<Worker>();
                   // ���������� DbContext
                   services.AddDbContext<DbContextSyntheticStock>(options =>
                       options.UseNpgsql(hostContext.Configuration.GetConnectionString("DefaultConnection"))); // ���������� hostContext
               })
           .Build();

        await host.RunAsync();
    }
}