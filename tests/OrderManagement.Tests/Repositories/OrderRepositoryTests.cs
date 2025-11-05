using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Repositories;

namespace OrderManagement.Tests.Repositories;

public class OrderRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly OrderRepository _repository;

    public OrderRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new OrderRepository(_context);
    }

    [Fact]
    public async Task AddAsync_And_GetByIdAsync_ShouldSaveAndRetrieveOrder()
    {
        // Arrange
        var product = new Product("Test Product", 100.00m, 10);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var orderItems = new List<OrderItem>
        {
            new(product.Id, 2, 100.00m)
        };

        var order = new Order("John Doe", "john@example.com", orderItems);

        // Act
        await _repository.AddAsync(order);
        await _context.SaveChangesAsync();

        var retrievedOrder = await _repository.GetByIdAsync(order.Id);

        // Assert
        retrievedOrder.Should().NotBeNull();
        retrievedOrder!.Id.Should().Be(order.Id);
        retrievedOrder.CustomerName.Should().Be("John Doe");
        retrievedOrder.CustomerEmail.Should().Be("john@example.com");
        retrievedOrder.Status.Should().Be(OrderStatus.Pending);
        retrievedOrder.OrderItems.Should().HaveCount(1);
        retrievedOrder.OrderItems.First().ProductId.Should().Be(product.Id);
        retrievedOrder.OrderItems.First().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedAsync_WithStatusFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var product = new Product("Test Product", 100.00m, 10);
        await _context.Products.AddAsync(product);

        var orderItems = new List<OrderItem> { new(product.Id, 1, 100.00m) };
        
        var pendingOrder = new Order("John Doe", "john@example.com", orderItems);
        var completedOrder = new Order("Jane Doe", "jane@example.com", orderItems);
        completedOrder.Complete();

        await _repository.AddAsync(pendingOrder);
        await _repository.AddAsync(completedOrder);
        await _context.SaveChangesAsync();

        // Act
        var pendingOrders = await _repository.GetPagedAsync(1, 10, OrderStatus.Pending);
        var completedOrders = await _repository.GetPagedAsync(1, 10, OrderStatus.Completed);

        // Assert
        pendingOrders.Should().HaveCount(1);
        pendingOrders.First().Status.Should().Be(OrderStatus.Pending);

        completedOrders.Should().HaveCount(1);
        completedOrders.First().Status.Should().Be(OrderStatus.Completed);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}