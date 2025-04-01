using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using FuturesService.Services;
using CryptoExchange.Net.Authentication;
using Binance.Net.Objects.Options;
using FuturesService.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    // Add other logging providers as needed
});

// 2. Configure HttpClient
builder.Services.AddHttpClient();

// 3. Configure BinanceRestClient
builder.Services.AddTransient<IBinanceRestClient, BinanceRestClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<BinanceRestClient>();

    var apiKey = configuration["BINANCE_APISECRET"];
    var apiSecret = configuration["BINANCE_APISECRET"];

    if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
    {
        logger.LogError("Binance API ключи отсутствуют в конфигурации. Проверьте секцию 'Binance' и переменные окружения.");
        throw new InvalidOperationException("Binance API ключи отсутствуют в конфигурации.");
    }

    var options = new BinanceRestOptions
    {
        ApiCredentials = new ApiCredentials(apiKey, apiSecret)
    };

    var httpClient = provider.GetRequiredService<HttpClient>();
    return new BinanceRestClient(httpClient, loggerFactory, Microsoft.Extensions.Options.Options.Create(options));
});

// 4. Configure FuturesDataService (assuming IFuturesDataService is correctly implemented)
builder.Services.AddScoped<IFuturesDataService, FuturesDataService>();

// 5. Configure RabbitMQPublisher   //Add RabbitMQ
builder.Services.AddSingleton<IRabbitMQService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<RabbitMQService>>();

    return new RabbitMQService(configuration, logger); 
});

// 6. Add controllers  //Add Controllers
builder.Services.AddControllers();

// 7. Configure Swagger/OpenAPI  //Add OpenApi/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.      //Configuring Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   //Add Swagger
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();