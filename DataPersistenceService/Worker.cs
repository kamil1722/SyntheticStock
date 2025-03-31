
using DataWorkService.Models;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataWorkService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        private IConnection _connection = null!;
        private IModel _channel = null!;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            InitializeRabbitMq();
        }

        private void InitializeRabbitMq()
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _configuration["RabbitMQ:HostName"],
                    Port = _configuration.GetValue<int>("RabbitMQ:Port"),
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: "futures.exchange", type: ExchangeType.Direct);
                _channel.QueueDeclare(queue: "futures.data", durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueBind(queue: "futures.data", exchange: "futures.exchange", routingKey: "futures.data");

                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false); // Обрабатываем по одному сообщению
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

                    SaveDataToPostgres(message);

                    _channel.BasicAck(ea.DeliveryTag, false); // Подтверждаем получение сообщения
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    _channel.BasicNack(ea.DeliveryTag, false, true); // Отправляем сообщение обратно в очередь
                }
            };

            _channel.BasicConsume(queue: "futures.data", autoAck: false, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void SaveDataToPostgres(string message)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("Connection string is null");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                try
                {
                    var futuresDataList = JsonConvert.DeserializeObject<List<FuturesPriceDifference>>(message) ?? throw new Exception("Parameter futuresData cannot be null");

                    //Валидация (предполагается, что метод IsValid существует и проверяет данные)
                    if (!IsValid(futuresDataList))
                    {
                        _logger.LogError("Invalid message format received: " + message);
                        throw new Exception("Invalid message format");
                    }

                    // SQL запрос для пакетной вставки
                    using (var writer = conn.BeginBinaryImport("COPY \"FuturesPriceDifferences\" (\"symbol1\", \"symbol2\", \"time\", \"difference\", \"interval\") FROM STDIN (FORMAT BINARY)"))
                    {
                        foreach (var futuresData in futuresDataList)
                        {
                            writer.StartRow();
                            writer.Write(futuresData.symbol1, NpgsqlDbType.Text);
                            writer.Write(futuresData.symbol2, NpgsqlDbType.Text);
                            writer.Write(futuresData.time, NpgsqlDbType.TimestampTz);
                            writer.Write(futuresData.difference, NpgsqlDbType.Numeric);
                            writer.Write(futuresData.interval, NpgsqlDbType.Text);
                        }

                        writer.Complete(); // Ensure data is fully written
                    }

                    _logger.LogInformation($"Bulk inserted {futuresDataList.Count} rows into PostgreSQL");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving data to PostgreSQL: " + ex.Message);
                    throw; // Re-throw to Nack the message
                }
            }
        }

        //Валидация
        private bool IsValid(object obj)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(obj);
            return Validator.TryValidateObject(obj, context, results, true);
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
