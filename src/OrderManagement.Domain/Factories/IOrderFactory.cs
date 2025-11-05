using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Factories;

public interface IOrderFactory
{
    Task<Order> CreateAsync(string customerName, string customerEmail, List<CreateOrderItemRequest> items);
}

public record CreateOrderItemRequest(Guid ProductId, int Quantity);