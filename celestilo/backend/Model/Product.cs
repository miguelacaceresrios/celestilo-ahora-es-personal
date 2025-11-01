using System.ComponentModel.DataAnnotations;

namespace backend.Model;

/// <summary>
/// Represents a product entity in the system.
/// This entity is used for storing product information in the database.
/// </summary>
public class Product
{
    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description of the product.
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    [Required]
    public decimal Price { get; set; }
    
    /// <summary>
    /// Gets or sets the stock quantity of the product.
    /// </summary>
    [Required]
    public int Stock { get; set; }
    
    /// <summary>
    /// Gets or sets the category identifier that the product belongs to.
    /// </summary>
    [Required]
    public int CategoryId { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when the product was registered in the system.
    /// Defaults to the current date and time when not specified.
    /// </summary>
    [Required]
    public DateTime? RegistrationDate { get; set; } = DateTime.Now;
}