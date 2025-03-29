public interface IRabbitMQService
{
    void Publish(object data, string exchange, string routingKey);
}