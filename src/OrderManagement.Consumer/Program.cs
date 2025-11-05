using OrderManagement.Application;
using OrderManagement.Consumer.Workers;
using OrderManagement.Infrastructure;
using RabbitMQ.Client;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Add Serilog
builder.Services.AddSerilog(builder.Configuration);

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add RabbitMQ connection for consumer
builder.Services.AddSingleton<IConnection>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<Program>>();
    var config = sp.GetRequiredService<IConfiguration>();
    
    var factory = new ConnectionFactory()
    {
        HostName = config["RabbitMQ:Host"] ?? "localhost",
        Port = int.Parse(config["RabbitMQ:Port"] ?? "5672"),
        UserName = config["RabbitMQ:Username"] ?? "guest",
        Password = config["RabbitMQ:Password"] ?? "guest"
    };

    try
    {
        var connection = factory.CreateConnection("OrderConsumer");
        logger.LogInformation("Connected to RabbitMQ successfully");
        return connection;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to connect to RabbitMQ");
        throw;
    }
});

// Add the background service
builder.Services.AddHostedService<OrderCreatedConsumer>();

var host = builder.Build();

Log.Information("Starting Order Management Consumer...");

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Consumer terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}