using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Validators;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Tests.Validators;

public class CreateOrderDtoValidatorTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly CreateOrderDtoValidator _validator;

    public CreateOrderDtoValidatorTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _validator = new CreateOrderDtoValidator(_productRepositoryMock.Object);
    }

    [Fact]
    public void Validate_ValidDto_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var product = new Product("Test Product", 100.00m, 10);
        var dto = new CreateOrderDto
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 2 }
            }
        };

        _productRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<Product> { product });

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyCustomerName_ShouldHaveValidationError(string customerName)
    {
        // Arrange
        var dto = new CreateOrderDto
        {
            CustomerName = customerName,
            CustomerEmail = "john@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CustomerName)
            .WithErrorMessage("Customer name is required");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("")]
    [InlineData("@example.com")]
    [InlineData("john@")]
    public void Validate_InvalidEmail_ShouldHaveValidationError(string email)
    {
        // Arrange
        var dto = new CreateOrderDto
        {
            CustomerName = "John Doe",
            CustomerEmail = email,
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CustomerEmail);
    }

    [Fact]
    public void Validate_EmptyItems_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new CreateOrderDto
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Items = new List<CreateOrderItemDto>()
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("At least one item is required");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidQuantity_ShouldHaveValidationError(int quantity)
    {
        // Arrange
        var dto = new CreateOrderDto
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = quantity }
            }
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor("Items[0].Quantity")
            .WithErrorMessage("Quantity must be greater than zero");
    }

    [Fact]
    public async Task Validate_InsufficientStock_ShouldHaveValidationError()
    {
        // Arrange
        var product = new Product("Test Product", 100.00m, 1); // Only 1 in stock
        var dto = new CreateOrderDto
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

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Insufficient stock"));
    }
}