using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.DTOs;
using backend.Services;
namespace backend.Controllers.Api;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class ProductController(IProductService productService) : ControllerBase
{
    // GET: api/product
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts()
    {
        var products = await productService.GetProductsAsync();
        return Ok(products);
    }

    // GET: api/product/{id}
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await productService.GetProductByIdAsync(id);

        if (product is null)
            return NotFound(new { error = $"Product with ID {id} not found" });

        return Ok(product);
    }

    // POST: api/product
    [HttpPost]
    public async Task<IActionResult> AddProduct([FromBody] CreateProductDto productDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (createdProduct, success) = await productService.AddProductAsync(productDto);

        if (!success)
            return StatusCode(500, new { error = "An error occurred while creating the product" });

        return CreatedAtAction(nameof(GetProduct), new { id = createdProduct!.Id }, createdProduct);
    }

    // PUT: api/product/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto productDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (product, success) = await productService.UpdateProductAsync(id, productDto);

        if (!success)
        {
            if (product is null)
                return NotFound(new { error = $"Product with ID {id} not found" });

            return StatusCode(500, new { error = "An error occurred while updating the product" });
        }

        return Ok(product);
    }

    // DELETE: api/product/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
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