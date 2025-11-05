using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Domain.Factories;

public class OrderFactory : IOrderFactory
{
    private readonly IProductRepository _productRepository;

    public OrderFactory(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Order> CreateAsync(string customerName, string customerEmail, List<CreateOrderItemRequest> items)
    {
        if (items == null || !items.Any())
            throw new ArgumentException("Order must have at least one item", nameof(items));

        var orderItems = new List<OrderItem>();

        // Validate and create order items
        foreach (var itemRequest in items)
        {
            var product = await _productRepository.GetByIdAsync(itemRequest.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {itemRequest.ProductId} not found");

            if (!product.HasSufficientStock(itemRequest.Quantity))
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}. Available: {product.Stock}, Requested: {itemRequest.Quantity}");

            var orderItem = new OrderItem(itemRequest.ProductId, itemRequest.Quantity, product.Price);
            orderItems.Add(orderItem);

            // Reduce stock
            product.ReduceStock(itemRequest.Quantity);
        }

        return new Order(customerName, customerEmail, orderItems);
    }
}