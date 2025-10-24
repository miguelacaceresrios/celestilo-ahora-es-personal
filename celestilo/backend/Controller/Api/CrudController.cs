using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CrudController : Controller
{
    private readonly ProductDbContext _context;
    public CrudController(ProductDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products.ToListAsync();
        
        return Ok(products);
    }
    [HttpPost]
    public async Task<IActionResult> AddProduct([FromBody] Product product)
    {
        await _context.Products.AddAsync(product);

        await _context.SaveChangesAsync();
        
        return Ok(product);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
    {

        if (updatedProduct == null || id != updatedProduct.Id)return BadRequest("Invalid product data");

        var exists = await _context.Products.AnyAsync(p => p.Id == id);

        if (!exists) return NotFound();

        _context.Products.Update(updatedProduct);

            await _context.SaveChangesAsync();
            return Ok(updatedProduct);
        
       
    }
}