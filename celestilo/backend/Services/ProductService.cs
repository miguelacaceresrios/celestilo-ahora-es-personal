using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Model;
using backend.Services.Interfaces;
namespace backend.Services;

/// <summary>
/// Service implementation for managing product operations including CRUD operations.
/// Provides logging and error handling for all product-related database operations.
/// </summary>
public class ProductService(ProductDbContext context, ILogger<ProductService> logger) : IProductService
{
    /// <summary>
    /// Retrieves all products from the database.
    /// Uses AsNoTracking() for read-only query optimization.
    /// </summary>
    /// <returns>A collection of <see cref="ProductDto"/> objects representing all products.</returns>
    public async Task<IEnumerable<ProductDto>> GetProductsAsync()
    {
        var correlationId = Guid.NewGuid().ToString();

        logger.LogInformation("[{CorrelationId}] Fetching all products", correlationId);
        try
        {
            var products = await context.Products.AsNoTracking().ToListAsync();

            logger.LogInformation("[{CorrelationId}] Successfully retrieved {ProductCount} products", correlationId, products.Count);

            return products.Select(p => MapToDto(p));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Failed to retrieve products", correlationId);

            return [];
        }
    }

    /// <summary>
    /// Retrieves a specific product by its unique identifier.
    /// Uses AsNoTracking() for read-only query optimization.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>
    /// The <see cref="ProductDto"/> object if found, otherwise null.
    /// </returns>
    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var correlationId = Guid.NewGuid().ToString();

        logger.LogInformation("[{CorrelationId}] Fetching product with ID: {ProductId}", correlationId, id);

        try
        {
            var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                logger.LogWarning("[{CorrelationId}] Product not found with ID: {ProductId}", correlationId, id);
                return null;
            }

            logger.LogInformation("[{CorrelationId}] Successfully retrieved product with ID: {ProductId}", correlationId, id);

            return MapToDto(product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Failed to retrieve product with ID: {ProductId}", correlationId, id);
            return null;
        }
    }

    /// <summary>
    /// Creates a new product in the database.
    /// Automatically sets the registration date to the current date and time.
    /// </summary>
    /// <param name="productDto">The product data transfer object containing product information.</param>
    /// <returns>
    /// A tuple containing:
    /// - The created <see cref="ProductDto"/> object if successful, otherwise null.
    /// - A boolean indicating if the operation succeeded.
    /// </returns>
    public async Task<(ProductDto?, bool success)> AddProductAsync(CreateProductDto productDto)
    {
        var correlationId = Guid.NewGuid().ToString();

        logger.LogInformation("[{CorrelationId}] Received request to create new product: {ProductName}", correlationId, productDto?.Name ?? "null");

        if (productDto is null)
        {
            logger.LogWarning("[{CorrelationId}] Product creation failed due to invalid model state", correlationId);

            return (null, false);
        }

        try
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock,
                CategoryId = productDto.CategoryId,
                RegistrationDate = DateTime.Now
            };

            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            logger.LogInformation("[{CorrelationId}] Product created successfully with ID: {ProductId}", correlationId, product.Id);

            return (MapToDto(product), true);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Database error while creating product: {ProductName}", correlationId, productDto.Name);
            return (null, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Unexpected error while creating product", correlationId);
            return (null, false);

        }
    }

    /// <summary>
    /// Updates an existing product in the database.
    /// Handles concurrency exceptions and database update errors.
    /// </summary>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="productDto">The product data transfer object containing updated product information.</param>
    /// <returns>
    /// A tuple containing:
    /// - The updated <see cref="ProductDto"/> object if successful, otherwise null.
    /// - A boolean indicating if the operation succeeded.
    /// </returns>
    public async Task<(ProductDto?, bool success)> UpdateProductAsync(int id, UpdateProductDto productDto)
    {
        var correlationId = Guid.NewGuid().ToString();

        logger.LogInformation("[{CorrelationId}] Received request to update product with ID: {ProductId}", correlationId, id);

        if (productDto is null)
        {
            logger.LogWarning("[{CorrelationId}] Product update failed: null product. Route ID: {RouteId}", correlationId, id);
            return (null, false);
        }

        try
        {
            var existingProduct = await context.Products.FindAsync(id);

            if (existingProduct is null)
            {
                logger.LogWarning("[{CorrelationId}] Product update failed: Product not found with ID: {ProductId}", correlationId, id);
                return (null, false);
            }

            existingProduct.Name = productDto.Name;
            existingProduct.Description = productDto.Description;
            existingProduct.Price = productDto.Price;
            existingProduct.Stock = productDto.Stock;
            existingProduct.CategoryId = productDto.CategoryId;

            await context.SaveChangesAsync();


            logger.LogInformation("[{CorrelationId}] Product with ID: {ProductId} updated successfully", correlationId, id);
            return (MapToDto(existingProduct), true);

        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Concurrency error while updating product with ID: {ProductId}", correlationId, id);
            return (null, false);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Database error while updating product with ID: {ProductId}", correlationId, id);
            return (null, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Unexpected error while updating product with ID: {ProductId}", correlationId, id);
            return (null, false);
        }
    }

    /// <summary>
    /// Deletes a product from the database by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete.</param>
    /// <returns>
    /// True if the product was successfully deleted, otherwise false.
    /// </returns>
    public async Task<bool> DeleteProductAsync(int id)
    {
        var correlationId = Guid.NewGuid().ToString();

        logger.LogInformation("[{CorrelationId}] Received request to delete product with ID: {ProductId}", correlationId, id);

        try
        {
            var product = await context.Products.FindAsync(id);

            if (product is null)
            {
                logger.LogWarning("[{CorrelationId}] Product deletion failed: Product not found with ID: {ProductId}", correlationId, id);
                return false;
            }

            context.Products.Remove(product);

            await context.SaveChangesAsync();

            logger.LogInformation("[{CorrelationId}] Product with ID: {ProductId} deleted successfully", correlationId, id);

            return true;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Database error while deleting product with ID: {ProductId}", correlationId, id);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Unexpected error while deleting product with ID: {ProductId}", correlationId, id);
            return false;
        }

    }

    /// <summary>
    /// Maps a <see cref="Product"/> entity to a <see cref="ProductDto"/> data transfer object.
    /// </summary>
    /// <param name="product">The product entity to map.</param>
    /// <returns>A <see cref="ProductDto"/> object containing the product information.</returns>
    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            RegistrationDate = product.RegistrationDate
        };
    }
}