using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;

public class RabbitMQService : IRabbitMQService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQService> _logger;

    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Publish(object data, string exchange, string routingKey)
    {
        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:HostName"],
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? throw new Exception("Parameter Port cannot be null")),
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Direct);
            _channel.QueueDeclare(queue: "futures.data", durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: "futures.data", exchange: exchange, routingKey: routingKey);

            string message = JsonConvert.SerializeObject(data);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: exchange,
                                 routingKey: routingKey,
                                 basicProperties: null,
                                 body: body);

            _logger.LogInformation($" [x] Sent data to RabbitMQ exchange: {exchange}, routingKey: {routingKey}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error publishing to RabbitMQ exchange: {exchange}, routingKey: {routingKey}");
        }
        finally
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
