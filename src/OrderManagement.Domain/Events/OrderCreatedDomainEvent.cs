using OrderManagement.Domain.Common;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Events;

public class OrderCreatedDomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public string CustomerName { get; }
    public string CustomerEmail { get; }
    public decimal TotalAmount { get; }
    public List<OrderItemDto> Items { get; }

    public OrderCreatedDomainEvent(
        Guid orderId,
        string orderNumber,
        string customerName,
        string customerEmail,
        decimal totalAmount,
        List<OrderItemDto> items)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        TotalAmount = totalAmount;
        Items = items;
    }
}