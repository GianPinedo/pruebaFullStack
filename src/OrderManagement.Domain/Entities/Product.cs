using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    private Product() { } // EF Core

    public Product(string name, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));
        
        if (price <= 0)
            throw new ArgumentException("Product price must be greater than zero", nameof(price));
        
        if (stock < 0)
            throw new ArgumentException("Product stock cannot be negative", nameof(stock));

        Name = name;
        Price = price;
        Stock = stock;
    }

    public void UpdateStock(int newStock)
    {
        if (newStock < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(newStock));
        
        Stock = newStock;
        SetUpdatedAt();
    }

    public void ReduceStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        
        if (Stock < quantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {Stock}, Requested: {quantity}");
        
        Stock -= quantity;
        SetUpdatedAt();
    }

    public bool HasSufficientStock(int quantity) => Stock >= quantity;
}