using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Email;
using OrderManagement.Infrastructure.Messaging;
using OrderManagement.Infrastructure.Repositories;
using OrderManagement.Infrastructure.Services;
using RabbitMQ.Client;
using Serilog;

namespace OrderManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Add repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

                // Use fake implementations for local development without Docker
        services.AddScoped<IMessagePublisher, FakeMessagePublisher>();

        // Use fake email implementation for local development
        services.AddScoped<IEmailSender, FakeEmailSender>();

        // Add Health Checks (flexible for local development)
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database")
            .AddCheck("rabbitmq", () =>
            {
                try
                {
                    var factory = new ConnectionFactory()
                    {
                        HostName = configuration["RabbitMQ:Host"] ?? "localhost",
                        Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                        UserName = configuration["RabbitMQ:Username"] ?? "guest",
                        Password = configuration["RabbitMQ:Password"] ?? "guest"
                    };
                    using var connection = factory.CreateConnection();
                    return HealthCheckResult.Healthy("RabbitMQ is accessible");
                }
                catch
                {
                    return HealthCheckResult.Degraded("RabbitMQ not accessible - using fake implementation");
                }
            })
            .AddCheck("mailhog", () =>
            {
                try
                {
                    var smtpHost = configuration["Email:SmtpHost"] ?? "localhost";
                    var smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "1025");
                    
                    using var tcpClient = new System.Net.Sockets.TcpClient();
                    tcpClient.Connect(smtpHost, smtpPort);
                    return HealthCheckResult.Healthy("MailHog is accessible");
                }
                catch
                {
                    return HealthCheckResult.Degraded("MailHog not accessible - using fake implementation");
                }
            });

        return services;
    }

    public static IServiceCollection AddSerilog(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        services.AddSerilog();
        return services;
    }
}