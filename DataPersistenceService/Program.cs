using DataPersistenceService;  // Assuming this is the correct namespace where Worker class is
using Microsoft.EntityFrameworkCore;
using DataPersistenceService.Data;
//Add configuration info

public class Program
{
    public static async Task Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)

               .ConfigureServices(services =>
               {
                   var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
                   if (configuration != null)
                   {
                       services.AddHostedService<Worker>(); // Add the  background service
                       services.AddDbContext<DbContextSyntheticStock>(options => //Add DbContext
                                   options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
                   }
                   
               })
           .Build();

        await host.RunAsync();
    }
}