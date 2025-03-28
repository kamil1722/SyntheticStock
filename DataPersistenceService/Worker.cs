using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using DataPersistenceService.Models;
using DataPersistenceService.Data;

namespace DataPersistenceService
{
    public class Worker : BackgroundService
    {
        public required IConnection _connection;
        public required IModel _channel;

        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        private readonly string _queueName;
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _hostname = _configuration["RabbitMQ:Hostname"] ??
                 throw new ArgumentNullException("RabbitMQ:Hostname", "RabbitMQ:Hostname configuration is missing.");
            _queueName = _configuration["RabbitMQ:QueueName"] ??
                throw new ArgumentNullException("RabbitMQ:QueueName", "RabbitMQ:QueueName configuration is missing.");
            _username = _configuration["RabbitMQ:Username"] ??
                throw new ArgumentNullException("RabbitMQ:Username", "RabbitMQ:Username configuration is missing.");
            _password = _configuration["RabbitMQ:Password"] ??
                throw new ArgumentNullException("RabbitMQ:Password", "RabbitMQ:Password configuration is missing.");

            _serviceProvider = serviceProvider;

            InitializeRabbitMQ();
        }
        private void InitializeRabbitMQ()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password
                };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing RabbitMQ connection");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //RabbitMQ check state   
            if (_connection.IsOpen)
            {
                _logger.LogError("RabbitMQ connection is not established.  Service will not start.");
                return;
            }
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            try
            {
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    // ALWAYS USE A SCOPE HERE!
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        // Get a fresh DbContext instance from the scope
                        var dbContext = scope.ServiceProvider.GetRequiredService<DbContextSyntheticStock>();// get fresh DbContext instance

                        try
                        { // Json options
                            var options = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            };
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);
                            _logger.LogInformation($"Received message: {message}");

                            var data = JsonSerializer.Deserialize<FuturesPriceDifference>(message, options);// Deserialize data
                            if (data != null)
                            {
                                dbContext.FuturesPriceDifferences.Add(data);

                                await dbContext.SaveChangesAsync();
                                _logger.LogInformation($"The item {data.Id} was successfully saved.");
                                _channel?.BasicAck(ea.DeliveryTag, false);// Remove the message
                            }
                            else
                            {
                                _logger.LogWarning($"Can not be serialized message.");
                                _channel?.BasicNack(ea.DeliveryTag, false, false);  // reject the message    
                            }
                        }
                        catch (Exception ex) // try cach block
                        {
                            _logger.LogError(ex, $"Error processing message from RabbitMQ");
                            _channel?.BasicNack(ea.DeliveryTag, false, false);//reject the message
                        }
                    }
                    ;
                };
                _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
                _logger.LogInformation("Consuming");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error consuming the data");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }

            _channel.Close();
            _connection.Close();

        }

    }

}