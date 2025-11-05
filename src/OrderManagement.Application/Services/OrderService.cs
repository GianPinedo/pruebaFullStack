using FluentValidation;
using Mapster;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Events;
using OrderManagement.Application.Exceptions;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Factories;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderFactory _orderFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IValidator<CreateOrderDto> _validator;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderFactory orderFactory,
        IUnitOfWork unitOfWork,
        IMessagePublisher messagePublisher,
        IValidator<CreateOrderDto> validator)
    {
        _orderRepository = orderRepository;
        _orderFactory = orderFactory;
        _unitOfWork = unitOfWork;
        _messagePublisher = messagePublisher;
        _validator = validator;
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto createOrderDto)
    {
        // Validate the request
        var validationResult = await _validator.ValidateAsync(createOrderDto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new Exceptions.ValidationException($"Validation failed: {errors}");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Create order using factory
            var itemRequests = createOrderDto.Items.Select(i => 
                new CreateOrderItemRequest(i.ProductId, i.Quantity)).ToList();
            
            var order = await _orderFactory.CreateAsync(
                createOrderDto.CustomerName, 
                createOrderDto.CustomerEmail, 
                itemRequests);

            // Save order
            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Publish domain events
            var domainEvents = order.DomainEvents.ToList();
            order.ClearDomainEvents();

            foreach (var domainEvent in domainEvents)
            {
                if (domainEvent is Domain.Events.OrderCreatedDomainEvent orderCreatedEvent)
                {
                    var integrationEvent = new OrderCreatedEvent
                    {
                        OrderId = orderCreatedEvent.OrderId,
                        OrderNumber = orderCreatedEvent.OrderNumber,
                        CustomerName = orderCreatedEvent.CustomerName,
                        CustomerEmail = orderCreatedEvent.CustomerEmail,
                        TotalAmount = orderCreatedEvent.TotalAmount,
                        Items = orderCreatedEvent.Items.Select(i => new OrderCreatedEventItem
                        {
                            ProductId = i.ProductId,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice
                        }).ToList(),
                        CreatedAt = orderCreatedEvent.OccurredOn
                    };

                    await _messagePublisher.PublishAsync(integrationEvent, "order-created-queue");
                }
            }

            await _unitOfWork.CommitTransactionAsync();

            // Return the created order
            var orderWithItems = await _orderRepository.GetByIdAsync(order.Id);
            return orderWithItems!.Adapt<OrderDto>();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        return order?.Adapt<OrderDto>();
    }

    public async Task<PagedResult<OrderDto>> GetPagedAsync(GetOrdersQuery query)
    {
        var orders = await _orderRepository.GetPagedAsync(query.Page, query.PageSize, query.Status);
        var totalCount = await _orderRepository.GetCountAsync(query.Status);

        return new PagedResult<OrderDto>
        {
            Items = orders.Adapt<List<OrderDto>>(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<OrderDto> CancelAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            throw new NotFoundException("Order", id);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            order.Cancel();
            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();

            // Publish cancellation events
            var domainEvents = order.DomainEvents.ToList();
            order.ClearDomainEvents();

            foreach (var domainEvent in domainEvents)
            {
                if (domainEvent is Domain.Events.OrderCancelledDomainEvent orderCancelledEvent)
                {
                    var integrationEvent = new OrderCancelledEvent
                    {
                        OrderId = orderCancelledEvent.OrderId,
                        OrderNumber = orderCancelledEvent.OrderNumber,
                        CustomerEmail = orderCancelledEvent.CustomerEmail,
                        CancelledAt = orderCancelledEvent.OccurredOn
                    };

                    await _messagePublisher.PublishAsync(integrationEvent, "order-cancelled-queue");
                }
            }

            await _unitOfWork.CommitTransactionAsync();
            return order.Adapt<OrderDto>();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}