using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class CrudController(ProductDbContext context) : Controller
{
    //Get: api/crud
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await context.Products.ToListAsync();

        return Ok(products);
    }
    // Solo los usuarios con rol "Admin" pueden agregar productos
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddProduct([FromBody] Product product)
    {
        await context.Products.AddAsync(product);

        await context.SaveChangesAsync();

        return Ok(product);
    }
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
    {

        if (updatedProduct == null || id != updatedProduct.Id) return BadRequest("Invalid product data");

        var exists = await context.Products.AnyAsync(p => p.Id == id);

        if (!exists) return NotFound();

        context.Products.Update(updatedProduct);

        await context.SaveChangesAsync();
        return Ok(updatedProduct);


    }
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await context.Products.FindAsync(id);

        if (product is null) return NotFound();

        context.Products.Remove(product);

        await context.SaveChangesAsync();

        return Ok();
    }
}