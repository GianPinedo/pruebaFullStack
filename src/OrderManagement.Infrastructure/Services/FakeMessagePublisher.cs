using Microsoft.Extensions.Logging;
using OrderManagement.Application.Interfaces;

namespace OrderManagement.Infrastructure.Services;

public class FakeMessagePublisher : IMessagePublisher
{
    private readonly ILogger<FakeMessagePublisher> _logger;

    public FakeMessagePublisher(ILogger<FakeMessagePublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<T>(T message, string routingKey = "") where T : class
    {
        _logger.LogInformation("FAKE MESSAGE PUBLISHER: Simulating message publish to queue '{RoutingKey}'. Message: {@Message}", 
            routingKey, message);
        
        return Task.CompletedTask;
    }
}