namespace OrderManagement.Application.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<OrderCreatedEventItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class OrderCreatedEventItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}