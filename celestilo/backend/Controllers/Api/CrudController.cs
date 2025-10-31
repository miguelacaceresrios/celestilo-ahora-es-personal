using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using backend.Data;
using backend.Model;
namespace backend.Controllers.Api;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class CrudController(ProductDbContext context, ILogger<CrudController> logger) : ControllerBase
{
    //Get: api/crud
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts()
    {
        var correlationId = Guid.NewGuid().ToString();

        logger.LogInformation("[{CorrelationId}] Fetching all products", correlationId);

        try
        {
            var products = await context.Products.ToListAsync();

            logger.LogInformation("[{CorrelationId}] Successfully retrieved {ProductCount} products", correlationId, products.Count);

            return Ok(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Failed to retrieve products", correlationId);

            return StatusCode(500, new { error = "An error occurred while retrieving products" });
        }
    }
    // Post: api/crud
    [HttpPost]
    public async Task<IActionResult> AddProduct([FromBody] Product product)
    {
        var correlationId = Guid.NewGuid().ToString();

        logger.LogInformation("[{CorrelationId}] Received request to create new product: {ProductName}", correlationId, product.Name ?? "null");

        if (!ModelState.IsValid)
        {
            logger.LogWarning("[{CorrelationId}] Product creation failed due to invalid model state", correlationId);

            return BadRequest(ModelState);
        }

        try
        {
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            logger.LogInformation("[{CorrelationId}] Product created successfully with ID: {ProductId}", correlationId, product.Id);

            return CreatedAtAction(nameof(GetProducts), product);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Database error while creating product: {ProductName}", correlationId, product.Name);
            return StatusCode(500, new { error = "A database error occurred while creating the product" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Unexpected error while creating product", correlationId);
            return StatusCode(500, new { error = "An unexpected error occurred while creating the product" });

        }
    }
    // Put: api/crud/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
    {
        var correlationId = Guid.NewGuid().ToString();

        logger.LogInformation("[{CorrelationId}] Received request to update product with ID: {ProductId}", correlationId, id);
        if (updatedProduct is null || id != updatedProduct.Id)
        {
            logger.LogWarning("[{CorrelationId}] Product update failed: ID mismatch. Route ID: {RouteId}, Body ID: {BodyId}", correlationId, id, updatedProduct?.Id ?? 0);
            return BadRequest(new { error = "Product ID mismatch" });
        }

        if (!ModelState.IsValid)
        {
            logger.LogWarning("[{CorrelationId}] Product update failed due to invalid model state for ID: {ProductId}", correlationId, id);
            return BadRequest(ModelState);
        }

        try
        {
            var existingProduct = await context.Products.FindAsync(id);

            if (existingProduct is null)
            {
                logger.LogWarning("[{CorrelationId}] Product update failed: Product not found with ID: {ProductId}", correlationId, id);
                return NotFound(new { error = $"Product with ID {id} not found" });
            }

            context.Entry(existingProduct).CurrentValues.SetValues(updatedProduct);
            await context.SaveChangesAsync();


            logger.LogInformation("[{CorrelationId}] Product with ID: {ProductId} updated successfully", correlationId, id);
            return Ok(existingProduct);

        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Concurrency error while updating product with ID: {ProductId}", correlationId, id);
            return StatusCode(409, new { error = "The product was modified by another user. Please refresh and try again" });
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Database error while updating product with ID: {ProductId}", correlationId, id);
            return StatusCode(500, new { error = "A database error occurred while updating the product" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Unexpected error while updating product with ID: {ProductId}", correlationId, id);
            return StatusCode(500, new { error = "An unexpected error occurred while updating the product" });
        }

    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var correlationId = Guid.NewGuid().ToString();

        logger.LogInformation("[{CorrelationId}] Received request to delete product with ID: {ProductId}", correlationId, id);

        try
        {
            var product = await context.Products.FindAsync(id);

            if (product is null)
            {
                logger.LogWarning("[{CorrelationId}] Product deletion failed: Product not found with ID: {ProductId}", correlationId, id);
                return NotFound(new { error = $"Product with ID {id} not found" });
            }

            context.Products.Remove(product);

            await context.SaveChangesAsync();

            logger.LogInformation("[{CorrelationId}] Producto con ID: {Id} eliminado exitosamente", correlationId, id);


            logger.LogInformation("[{CorrelationId}] Product with ID: {ProductId} deleted successfully", correlationId, id);

            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Database error while deleting product with ID: {ProductId}",correlationId, id);
            return StatusCode(500, new { error = "A database error occurred while deleting the product" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{CorrelationId}] Unexpected error while deleting product with ID: {ProductId}",correlationId, id);
            return StatusCode(500, new { error = "An unexpected error occurred while deleting the product" });
        }
    }
}