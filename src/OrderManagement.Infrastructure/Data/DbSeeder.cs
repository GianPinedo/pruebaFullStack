using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Products.AnyAsync())
            return;

        var products = new List<Product>
        {
            new Product("Gaming Laptop", 1299.99m, 25),
            new Product("Wireless Mouse", 49.99m, 100),
            new Product("Mechanical Keyboard", 129.99m, 75),
            new Product("4K Monitor", 399.99m, 30),
            new Product("USB-C Hub", 79.99m, 150),
            new Product("Webcam HD", 89.99m, 60),
            new Product("Bluetooth Headphones", 199.99m, 40),
            new Product("External SSD 1TB", 149.99m, 80),
            new Product("Smartphone", 799.99m, 50),
            new Product("Tablet", 499.99m, 35)
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}