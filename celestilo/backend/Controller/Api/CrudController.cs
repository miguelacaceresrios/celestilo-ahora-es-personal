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
        // Validación temprana del modelo
        if (updatedProduct == null || id != updatedProduct.Id)return BadRequest("Invalid product data");

        // Verificar existencia sin cargar toda la entidad
        var exists = await _context.Products.AnyAsync(p => p.Id == id);
        if (!exists) return NotFound();

        // Actualizar directamente sin tracking
        _context.Products.Update(updatedProduct);

        try
        {
            await _context.SaveChangesAsync();
            return Ok(updatedProduct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Verificar si el producto fue eliminado durante la operación
            if (!await _context.Products.AnyAsync(p => p.Id == id))
                return NotFound();
            throw;
        }
    }
}