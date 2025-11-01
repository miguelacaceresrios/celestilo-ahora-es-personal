using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

/// <summary>
/// Data Transfer Object representing product information for API responses.
/// Used when returning product data to clients.
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the product.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description of the product.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Gets or sets the stock quantity of the product.
    /// </summary>
    public int Stock { get; set; }
    
    /// <summary>
    /// Gets or sets the category identifier that the product belongs to.
    /// </summary>
    public int CategoryId { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when the product was registered in the system.
    /// </summary>
    public DateTime? RegistrationDate { get; set; }
}

/// <summary>
/// Data Transfer Object for creating a new product.
/// Used as the request model for product creation endpoints.
/// </summary>
public class CreateProductDto
{
    /// <summary>
    /// Gets or sets the name of the product.
    /// Must be between 2 and 200 characters.
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the product.
    /// Must be between 10 and 1000 characters.
    /// </summary>
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the product.
    /// Must be greater than 0.01.
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the stock quantity of the product.
    /// Must be zero or greater.
    /// </summary>
    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    /// <summary>
    /// Gets or sets the category identifier that the product belongs to.
    /// </summary>
    [Required]
    public int CategoryId { get; set; }
}

/// <summary>
/// Data Transfer Object for updating an existing product.
/// Used as the request model for product update endpoints.
/// </summary>
public class UpdateProductDto
{
    /// <summary>
    /// Gets or sets the name of the product.
    /// Must be between 2 and 200 characters.
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the product.
    /// Must be between 10 and 1000 characters.
    /// </summary>
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the product.
    /// Must be greater than 0.01.
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the stock quantity of the product.
    /// Must be zero or greater.
    /// </summary>
    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    /// <summary>
    /// Gets or sets the category identifier that the product belongs to.
    /// </summary>
    [Required]
    public int CategoryId { get; set; }
}

