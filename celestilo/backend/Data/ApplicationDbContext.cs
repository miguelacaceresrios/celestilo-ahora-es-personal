using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace backend.Data;

/// <summary>
/// Database context for managing application identity and user authentication.
/// Extends IdentityDbContext to provide user, role, and authentication management functionality.
/// Includes seed data for default roles (Admin and User).
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    /// <summary>
    /// Configures the model that was discovered by convention from the entity types
    /// exposed in DbSet properties on the derived context.
    /// Seeds initial role data.
    /// </summary>
    /// <param name="builder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Seed de roles
        SeedRoles(builder);
    }

    /// <summary>
    /// Seeds the database with initial role data.
    /// Creates Admin and User roles if they don't already exist.
    /// </summary>
    /// <param name="builder">The model builder used to configure the entity types.</param>
    private void SeedRoles(ModelBuilder builder)
    {
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = "2", Name = "User", NormalizedName = "USER" }
        );
    }
}

