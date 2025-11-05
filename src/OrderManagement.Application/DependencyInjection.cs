using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Mappings;
using OrderManagement.Application.Services;
using OrderManagement.Application.Validators;
using OrderManagement.Domain.Factories;

namespace OrderManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Configure Mapster
        MappingConfig.Configure();

        // Register services
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderFactory, OrderFactory>();

        // Register validators
        services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>();

        return services;
    }
}