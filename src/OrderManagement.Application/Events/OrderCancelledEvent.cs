namespace OrderManagement.Application.Events;

public class OrderCancelledEvent
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; }
}