using DataWorkService.Service;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace DataWorkService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPostgreService _postgreService;

        private IConnection _connection = null!;
        private IModel _channel = null!;

        public Worker(ILogger<Worker> logger, IConfiguration configuration,
            IPostgreService postgreService)
        {
            _logger = logger;
            _configuration = configuration;
            _postgreService = postgreService;

            InitializeRabbitMq();
        }

        private void InitializeRabbitMq()
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _configuration["RABBITMQ_HOST"],
                    Port =  int.Parse((_configuration["RABBITMQ_PORT"] ?? throw new Exception()))
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // ќбъ€вл€ем обменник (exchange) с именем "futures.exchange" и типом "direct".
                _channel.ExchangeDeclare(exchange: "futures.exchange", type: ExchangeType.Direct);
                // ќбъ€вл€ем очередь (queue) с именем "futures.data".
                _channel.QueueDeclare(queue: "futures.data", durable: true, exclusive: false, autoDelete: false, arguments: null);
                // —оздаем прив€зку (binding) между очередью "futures.data" и обменником "futures.exchange", использу€ ключ маршрутизации "futures.data".
                _channel.QueueBind(queue: "futures.data", exchange: "futures.exchange", routingKey: "futures.data");

                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false); // ќбрабатываем по одному сообщению
                _logger.LogInformation("RabbitMQ connection established");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error initializing RabbitMQ listener: {ex.Message}");
                throw;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"Received message: {message}");

                    _postgreService.SaveDataToPostgres(message);

                    _channel.BasicAck(ea.DeliveryTag, false); // ѕодтверждаем получение сообщени€
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    _channel.BasicNack(ea.DeliveryTag, false, true); // ќтправл€ем сообщение обратно в очередь
                }
            };

            _channel.BasicConsume(queue: "futures.data", autoAck: false, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override void Dispose()
        {
            if (_channel.IsOpen)
            {
                try
                {
                    _channel.Close();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing RabbitMQ channel");
                }
            }

            if (_connection.IsOpen)
            {
                try
                {
                    _connection.Close();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing RabbitMQ connection");
                }
            }

            base.Dispose();
        }
    }
}
