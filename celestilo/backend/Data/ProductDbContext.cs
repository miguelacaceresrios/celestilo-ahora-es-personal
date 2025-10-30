
using Microsoft.EntityFrameworkCore;
using backend.Model;
namespace backend.Data;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}