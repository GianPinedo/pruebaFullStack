using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Events;

public class OrderCancelledDomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public string CustomerEmail { get; }

    public OrderCancelledDomainEvent(Guid orderId, string orderNumber, string customerEmail)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        CustomerEmail = customerEmail;
    }
}