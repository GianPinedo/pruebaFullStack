using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<List<Order>> GetAllAsync();
    Task<List<Order>> GetPagedAsync(int page, int pageSize, OrderStatus? status = null);
    Task<int> GetCountAsync(OrderStatus? status = null);
    Task AddAsync(Order order);
    void Update(Order order);
    void Delete(Order order);
}