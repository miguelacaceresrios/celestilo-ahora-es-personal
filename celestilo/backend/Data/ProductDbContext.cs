using Microsoft.EntityFrameworkCore;
using backend.Model;
namespace backend.Data;

/// <summary>
/// Database context for managing product-related data.
/// Provides access to the Products entity set.
/// </summary>
public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the Products entity set, which represents all products in the database.
    /// </summary>
    public DbSet<Product> Products { get; set; }
}