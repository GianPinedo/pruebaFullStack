using OrderManagement.Domain.Common;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Events;

namespace OrderManagement.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }

    private readonly List<OrderItem> _orderItems = new();
    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Order() { } // EF Core

    internal Order(string customerName, string customerEmail, List<OrderItem> items)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name cannot be empty", nameof(customerName));
        
        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new ArgumentException("Customer email cannot be empty", nameof(customerEmail));
        
        if (items == null || !items.Any())
            throw new ArgumentException("Order must have at least one item", nameof(items));

        CustomerName = customerName;
        CustomerEmail = customerEmail;
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        
        foreach (var item in items)
        {
            item.SetOrderId(Id);
            _orderItems.Add(item);
        }
        
        CalculateTotalAmount();
        GenerateOrderNumber();

        // Raise domain event
        var orderCreatedEvent = new OrderCreatedDomainEvent(
            Id,
            OrderNumber,
            CustomerName,
            CustomerEmail,
            TotalAmount,
            OrderItems.Select(oi => new OrderItemDto(oi.ProductId, oi.Quantity, oi.UnitPrice)).ToList()
        );
        
        _domainEvents.Add(orderCreatedEvent);
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled");
        
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed order");

        Status = OrderStatus.Cancelled;
        SetUpdatedAt();

        // Raise domain event for cancellation
        var orderCancelledEvent = new OrderCancelledDomainEvent(Id, OrderNumber, CustomerEmail);
        _domainEvents.Add(orderCancelledEvent);
    }

    public void Complete()
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Order is already completed");
        
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot complete a cancelled order");

        Status = OrderStatus.Completed;
        SetUpdatedAt();
    }

    private void CalculateTotalAmount()
    {
        TotalAmount = _orderItems.Sum(item => item.GetTotalPrice());
    }

    private void GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var shortGuid = Guid.NewGuid().ToString("N")[..8];
        OrderNumber = $"ORD-{timestamp}-{shortGuid}";
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

// Helper DTO for domain event
public record OrderItemDto(Guid ProductId, int Quantity, decimal UnitPrice);