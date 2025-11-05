using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Factories;
using OrderManagement.Domain.Repositories;
using FluentValidation;
using OrderManagement.Application.Validators;

namespace OrderManagement.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IOrderFactory> _orderFactoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMessagePublisher> _messagePublisherMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly IValidator<CreateOrderDto> _validator;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _orderFactoryMock = new Mock<IOrderFactory>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _messagePublisherMock = new Mock<IMessagePublisher>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _validator = new CreateOrderDtoValidator(_productRepositoryMock.Object);

        _orderService = new OrderService(
            _orderRepositoryMock.Object,
            _orderFactoryMock.Object,
            _unitOfWorkMock.Object,
            _messagePublisherMock.Object,
            _validator);
    }

    [Fact]
    public async Task Create_ValidOrder_ShouldReturnOrderDto()
    {
        // Arrange
        var product = new Product("Test Product", 100.00m, 10);
        var createOrderDto = new CreateOrderDto
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 2 }
            }
        };

        var order = new Order("John Doe", "john@example.com", new List<OrderItem>
        {
            new(product.Id, 2, 100.00m)
        });

        // Setup mocks
        _productRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<Product> { product });

        _orderFactoryMock.Setup(x => x.CreateAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<List<CreateOrderItemRequest>>()))
            .ReturnsAsync(order);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _orderService.CreateAsync(createOrderDto);

        // Assert
        result.Should().NotBeNull();
        result.CustomerName.Should().Be("John Doe");
        result.CustomerEmail.Should().Be("john@example.com");
        result.Status.Should().Be(OrderStatus.Pending);
        result.OrderItems.Should().HaveCount(1);

        // Verify interactions
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        _messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<object>(), "order-created-queue"), Times.Once);
    }

    [Fact]
    public async Task Create_InsufficientStock_ShouldThrowValidationException()
    {
        // Arrange
        var product = new Product("Test Product", 100.00m, 1); // Only 1 in stock
        var createOrderDto = new CreateOrderDto
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 5 } // Requesting 5
            }
        };

        _productRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<Product> { product });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Application.Exceptions.ValidationException>(
            () => _orderService.CreateAsync(createOrderDto));

        exception.Message.Should().Contain("Validation failed");
        exception.Message.Should().Contain("Insufficient stock");
    }

    [Fact]
    public async Task Cancel_ValidOrderId_ShouldReturnCancelledOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order("John Doe", "john@example.com", new List<OrderItem>
        {
            new(Guid.NewGuid(), 2, 100.00m)
        });

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _orderService.CancelAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);

        // Verify interactions
        _orderRepositoryMock.Verify(x => x.Update(It.IsAny<Order>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }
}