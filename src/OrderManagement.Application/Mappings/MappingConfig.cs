using Mapster;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Mappings;

public class MappingConfig
{
    public static void Configure()
    {
        // Order to OrderDto mapping
        TypeAdapterConfig<Order, OrderDto>.NewConfig()
            .Map(dest => dest.OrderItems, src => src.OrderItems);

        // OrderItem to OrderItemDto mapping
        TypeAdapterConfig<OrderItem, Application.DTOs.OrderItemDto>.NewConfig()
            .Map(dest => dest.ProductName, src => src.Product.Name)
            .Map(dest => dest.TotalPrice, src => src.GetTotalPrice());

        // Product to ProductDto mapping
        TypeAdapterConfig<Product, ProductDto>.NewConfig();
    }
}