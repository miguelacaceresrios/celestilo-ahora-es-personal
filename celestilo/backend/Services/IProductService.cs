using backend.DTOs;
namespace backend.Services;


public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<(ProductDto?, bool Success)> AddProductAsync(CreateProductDto product);
    Task<(ProductDto?, bool Success)> UpdateProductAsync(int id, UpdateProductDto updatedProduct);
    Task<bool> DeleteProductAsync(int id);
}