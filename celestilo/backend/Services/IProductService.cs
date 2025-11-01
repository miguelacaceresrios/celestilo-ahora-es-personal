using backend.DTOs;
namespace backend.Services;

/// <summary>
/// Interface defining product service operations for managing products.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves all products from the system.
    /// </summary>
    /// <returns>A collection of <see cref="ProductDto"/> objects representing all products.</returns>
    Task<IEnumerable<ProductDto>> GetProductsAsync();
    
    /// <summary>
    /// Retrieves a specific product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>
    /// The <see cref="ProductDto"/> object if found, otherwise null.
    /// </returns>
    Task<ProductDto?> GetProductByIdAsync(int id);
    
    /// <summary>
    /// Creates a new product in the system.
    /// </summary>
    /// <param name="product">The product data transfer object containing product information.</param>
    /// <returns>
    /// A tuple containing:
    /// - The created <see cref="ProductDto"/> object if successful, otherwise null.
    /// - A boolean indicating if the operation succeeded.
    /// </returns>
    Task<(ProductDto?, bool Success)> AddProductAsync(CreateProductDto product);
    
    /// <summary>
    /// Updates an existing product in the system.
    /// </summary>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="updatedProduct">The product data transfer object containing updated product information.</param>
    /// <returns>
    /// A tuple containing:
    /// - The updated <see cref="ProductDto"/> object if successful, otherwise null.
    /// - A boolean indicating if the operation succeeded.
    /// </returns>
    Task<(ProductDto?, bool Success)> UpdateProductAsync(int id, UpdateProductDto updatedProduct);
    
    /// <summary>
    /// Deletes a product from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete.</param>
    /// <returns>
    /// True if the product was successfully deleted, otherwise false.
    /// </returns>
    Task<bool> DeleteProductAsync(int id);
}