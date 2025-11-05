using System.Text;
using System.Text.Json;
using OrderManagement.Application.Interfaces;
using RabbitMQ.Client;

namespace OrderManagement.Infrastructure.Messaging;

public class RabbitMQPublisher : IMessagePublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQPublisher(IConnection connection)
    {
        _connection = connection;
        _channel = _connection.CreateModel();
        
        // Declare queues
        _channel.QueueDeclare("order-created-queue", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueDeclare("order-cancelled-queue", durable: true, exclusive: false, autoDelete: false);
    }

    public async Task PublishAsync<T>(T message, string routingKey = "") where T : class
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = Guid.NewGuid().ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        _channel.BasicPublish(
            exchange: "",
            routingKey: routingKey,
            basicProperties: properties,
            body: body);

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}