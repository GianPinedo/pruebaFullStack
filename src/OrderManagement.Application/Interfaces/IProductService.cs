using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(Guid id);
}