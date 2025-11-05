using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<List<Product>> GetAllAsync();
    Task<List<Product>> GetByIdsAsync(List<Guid> ids);
    Task AddAsync(Product product);
    void Update(Product product);
    void Delete(Product product);
}