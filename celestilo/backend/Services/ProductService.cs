using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Model;
namespace backend.Services;
public class ProductService(ProductDbContext context, ILogger<ProductService> logger) : IProductService
{
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
    public async Task<(ProductDto?, bool Success)> AddProductAsync(CreateProductDto productDto)
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
    public async Task<(ProductDto?, bool Success)> UpdateProductAsync(int id, UpdateProductDto productDto)
    {
        var correlationId = Guid.NewGuid().ToString();

        logger.LogInformation("[{CorrelationId}] Received request to update product with ID: {ProductId}", correlationId, id);

        if (productDto is null || id != productDto.Id)
        {
            logger.LogWarning("[{CorrelationId}] Product update failed: ID mismatch or null product. Route ID: {RouteId}, Body ID: {BodyId}", correlationId, id, productDto?.Id ?? 0);
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