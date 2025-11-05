using FluentValidation;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    private readonly IProductRepository _productRepository;

    public CreateOrderDtoValidator(IProductRepository productRepository)
    {
        _productRepository = productRepository;

        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .WithMessage("Customer name is required")
            .MaximumLength(100)
            .WithMessage("Customer name cannot exceed 100 characters");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty()
            .WithMessage("Customer email is required")
            .EmailAddress()
            .WithMessage("Valid email address is required")
            .MaximumLength(200)
            .WithMessage("Customer email cannot exceed 200 characters");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one item is required");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderItemDtoValidator());

        RuleFor(x => x)
            .MustAsync(HaveSufficientStock)
            .WithMessage("Insufficient stock for one or more items");
    }

    private async Task<bool> HaveSufficientStock(CreateOrderDto dto, CancellationToken cancellationToken)
    {
        if (dto.Items == null || !dto.Items.Any())
            return false;

        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _productRepository.GetByIdsAsync(productIds);

        foreach (var item in dto.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null || !product.HasSufficientStock(item.Quantity))
                return false;
        }

        return true;
    }
}

public class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero");
    }
}