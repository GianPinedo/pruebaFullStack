using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(CreateOrderDto createOrderDto);
    Task<OrderDto?> GetByIdAsync(Guid id);
    Task<PagedResult<OrderDto>> GetPagedAsync(GetOrdersQuery query);
    Task<OrderDto> CancelAsync(Guid id);
}