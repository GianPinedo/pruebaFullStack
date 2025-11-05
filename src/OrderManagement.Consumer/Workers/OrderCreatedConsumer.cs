using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Events;
using OrderManagement.Application.Interfaces;
using Polly;
using Polly.Extensions.Http;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OrderManagement.Consumer.Workers;

public class OrderCreatedConsumer : BackgroundService
{
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IAsyncPolicy _retryPolicy;

    public OrderCreatedConsumer(
        ILogger<OrderCreatedConsumer> logger,
        IServiceProvider serviceProvider,
        IConnection connection)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _connection = connection;
        _channel = _connection.CreateModel();

        // Configure retry policy with exponential backoff
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} for message processing after {Delay}ms. Error: {Error}",
                        retryCount, timespan.TotalMilliseconds, outcome.Exception?.Message);
                });

        SetupConsumer();
    }

    private void SetupConsumer()
    {
        // Declare the queue (idempotent)
        _channel.QueueDeclare("order-created-queue", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueDeclare("order-cancelled-queue", durable: true, exclusive: false, autoDelete: false);

        // Set prefetch count for better load balancing
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        _logger.LogInformation("OrderCreatedConsumer setup completed");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderCreatedConsumer started");

        // Setup consumer for order created events
        var orderCreatedConsumer = new EventingBasicConsumer(_channel);
        orderCreatedConsumer.Received += async (model, ea) =>
        {
            await ProcessOrderCreatedEvent(ea, stoppingToken);
        };

        _channel.BasicConsume(queue: "order-created-queue", autoAck: false, consumer: orderCreatedConsumer);

        // Setup consumer for order cancelled events
        var orderCancelledConsumer = new EventingBasicConsumer(_channel);
        orderCancelledConsumer.Received += async (model, ea) =>
        {
            await ProcessOrderCancelledEvent(ea, stoppingToken);
        };

        _channel.BasicConsume(queue: "order-cancelled-queue", autoAck: false, consumer: orderCancelledConsumer);

        // Keep the service running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessOrderCreatedEvent(BasicDeliverEventArgs ea, CancellationToken stoppingToken)
    {
        var messageId = ea.BasicProperties.MessageId ?? Guid.NewGuid().ToString();
        
        try
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            _logger.LogInformation("Processing OrderCreated event. MessageId: {MessageId}", messageId);

            var orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message);
            if (orderCreatedEvent == null)
            {
                _logger.LogError("Failed to deserialize OrderCreatedEvent. MessageId: {MessageId}", messageId);
                _channel.BasicNack(ea.DeliveryTag, false, false); // Dead letter
                return;
            }

            await _retryPolicy.ExecuteAsync(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                await emailSender.SendOrderConfirmationAsync(
                    orderCreatedEvent.CustomerEmail,
                    orderCreatedEvent.CustomerName,
                    orderCreatedEvent.OrderNumber,
                    orderCreatedEvent.TotalAmount);

                _logger.LogInformation("Order confirmation email sent successfully for order {OrderNumber}. MessageId: {MessageId}",
                    orderCreatedEvent.OrderNumber, messageId);
            });

            _channel.BasicAck(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process OrderCreated event after retries. MessageId: {MessageId}", messageId);
            _channel.BasicNack(ea.DeliveryTag, false, false); // Send to dead letter queue
        }
    }

    private async Task ProcessOrderCancelledEvent(BasicDeliverEventArgs ea, CancellationToken stoppingToken)
    {
        var messageId = ea.BasicProperties.MessageId ?? Guid.NewGuid().ToString();
        
        try
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            _logger.LogInformation("Processing OrderCancelled event. MessageId: {MessageId}", messageId);

            var orderCancelledEvent = JsonSerializer.Deserialize<OrderCancelledEvent>(message);
            if (orderCancelledEvent == null)
            {
                _logger.LogError("Failed to deserialize OrderCancelledEvent. MessageId: {MessageId}", messageId);
                _channel.BasicNack(ea.DeliveryTag, false, false);
                return;
            }

            await _retryPolicy.ExecuteAsync(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                // Extract customer name from email (simple approach)
                var customerName = orderCancelledEvent.CustomerEmail.Split('@')[0];

                await emailSender.SendOrderCancellationAsync(
                    orderCancelledEvent.CustomerEmail,
                    customerName,
                    orderCancelledEvent.OrderNumber);

                _logger.LogInformation("Order cancellation email sent successfully for order {OrderNumber}. MessageId: {MessageId}",
                    orderCancelledEvent.OrderNumber, messageId);
            });

            _channel.BasicAck(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process OrderCancelled event after retries. MessageId: {MessageId}", messageId);
            _channel.BasicNack(ea.DeliveryTag, false, false);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        base.Dispose();
    }
}