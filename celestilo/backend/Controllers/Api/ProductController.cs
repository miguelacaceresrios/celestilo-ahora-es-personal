using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.DTOs;
using backend.Services.Interfaces;
namespace backend.Controllers.Api;

/// <summary>
/// Controller responsible for managing product operations.
/// Most operations require Admin role, except for read operations which are publicly accessible.
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class ProductController(IProductService productService) : ControllerBase
{
    /// <summary>
    /// Retrieves all products from the system.
    /// </summary>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 200 OK containing a collection of <see cref="ProductDto"/> objects.
    /// </returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductsAsync()
    {
        var products = await productService.GetProductsAsync();
        return Ok(products);
    }

    /// <summary>
    /// Retrieves a specific product by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 404 NotFound if the product with the specified ID does not exist.
    /// - 200 OK containing the <see cref="ProductDto"/> object if found.
    /// </returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductAsync(int id)
    {
        var product = await productService.GetProductByIdAsync(id);

        if (product is null) return NotFound(new { error = $"Product with ID {id} not found" });

        return Ok(product);
    }

    /// <summary>
    /// Creates a new product in the system.
    /// Requires Admin role.
    /// </summary>
    /// <param name="productDto">The product data transfer object containing product information.</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 400 BadRequest if the model state is invalid.
    /// - 500 InternalServerError if an error occurs during product creation.
    /// - 201 Created with the created <see cref="ProductDto"/> object and location header.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> AddProductAsync([FromBody] CreateProductDto productDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (createdProduct, success) = await productService.AddProductAsync(productDto);

        if (!success) return StatusCode(500, new { error = "An error occurred while creating the product" });

        return CreatedAtAction(nameof(GetProductAsync), new { id = createdProduct!.Id }, createdProduct);
    }

    /// <summary>
    /// Updates an existing product by its ID.
    /// Requires Admin role.
    /// </summary>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="productDto">The product data transfer object containing updated product information.</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 400 BadRequest if the model state is invalid.
    /// - 404 NotFound if the product with the specified ID does not exist.
    /// - 500 InternalServerError if an error occurs during product update.
    /// - 200 OK with the updated <see cref="ProductDto"/> object upon successful update.
    /// </returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProductAsync(int id, [FromBody] UpdateProductDto productDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (product, success) = await productService.UpdateProductAsync(id, productDto);

        if (!success)
        {
            if (product is null) return NotFound(new { error = $"Product with ID {id} not found" });

            return StatusCode(500, new { error = "An error occurred while updating the product" });
        }

        return Ok(product);
    }

    /// <summary>
    /// Deletes a product from the system by its ID.
    /// Requires Admin role.
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete.</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> with:
    /// - 404 NotFound if the product with the specified ID does not exist.
    /// - 500 InternalServerError if an error occurs during product deletion.
    /// - 204 NoContent upon successful deletion.
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductAsync(int id)
    {
        var success = await productService.DeleteProductAsync(id);

        if (!success)
        {
            var product = await productService.GetProductByIdAsync(id);
            if (product is null) return NotFound(new { error = $"Product with ID {id} not found" });

            return StatusCode(500, new { error = "An error occurred while deleting the product" });
        }

        return NoContent();
    }
}