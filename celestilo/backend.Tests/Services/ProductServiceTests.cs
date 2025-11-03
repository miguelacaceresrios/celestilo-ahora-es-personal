using backend.Services;
using backend.Data;
using backend.Model;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Tests.Services;

/// <summary>
/// Unit tests for the <see cref="ProductService"/> class.
/// Tests cover CRUD operations, edge cases, and data validation scenarios using in-memory database.
/// </summary>
public class ProductServiceTests
{
    /// <summary>
    /// Creates an isolated in-memory database context for testing.
    /// Each test receives a unique database instance to prevent test interference.
    /// </summary>
    /// <returns>A new <see cref="ProductDbContext"/> configured with an in-memory database.</returns>
    private static ProductDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>().UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}").Options;

        return new ProductDbContext(options);
    }

    /// <summary>
    /// Creates a mock logger instance for dependency injection in test scenarios.
    /// </summary>
    /// <returns>A mocked <see cref="ILogger{T}"/> instance for <see cref="ProductService"/>.</returns>
    private static ILogger<ProductService> CreateMockLogger()
    {
        return Mock.Of<ILogger<ProductService>>();
    }

    /// <summary>
    /// Verifies that <see cref="ProductService.GetProductsAsync"/> returns all products from the database.
    /// </summary>
    [Fact]
    public async Task GetAllProducts_ShouldReturnAllProducts()
    {
        // Arrange: Set up test data and service instance
        using var context = CreateInMemoryContext();
        var logger = CreateMockLogger();

        await context.Products.AddRangeAsync(new List<Product>
            {
                new () { Id = 1, Name = "Laptop", Description = "High-quality gaming laptop", Price = 1000, Stock = 5, CategoryId = 1 },
                new () { Id = 2, Name = "Mouse", Description = "Ergonomic wireless mouse", Price = 25, Stock = 10, CategoryId = 1 },
                new () { Id = 3, Name = "Keyboard", Description = "RGB mechanical keyboard", Price = 50, Stock = 8, CategoryId = 1 }
            });
        await context.SaveChangesAsync();

        var productService = new ProductService(context, logger);

        // Act: Execute the method under test
        var result = await productService.GetProductsAsync();

        // Assert: Verify the results
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.First().Name.Should().Be("Laptop");
        result.First().Should().BeOfType<ProductDto>();
    }
    /// <summary>
    /// Verifies that <see cref="ProductService.GetProductsAsync"/> returns an empty collection
    /// when no products exist in the database.
    /// </summary>
    [Fact]
    public async Task GetAllProducts_NoProducts_ShouldReturnEmptyList()
    {
        // Arrange: Set up empty database
        using var context = CreateInMemoryContext();
        var logger = CreateMockLogger();

        var productService = new ProductService(context, logger);

        // Act: Execute the method under test
        var result = await productService.GetProductsAsync();

        // Assert: Verify empty result
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        result.Should().HaveCount(0);
    }

    /// <summary>
    /// Verifies that <see cref="ProductService.GetProductByIdAsync"/> returns the correct product
    /// when provided with a valid product identifier.
    /// </summary>
    [Fact]
    public async Task GetProductById_WithValidId_ShouldReturnProduct()
    {
        // Arrange: Set up test data with a known product
        using var context = CreateInMemoryContext();
        var logger = CreateMockLogger();

        var expectedProduct = new Product
        {
            Id = 1,
            Name = "Laptop Gaming",
            Description = "Latest generation gaming laptop",
            Price = 1500,
            Stock = 3,
            CategoryId = 1
        };

        await context.Products.AddAsync(expectedProduct);
        await context.SaveChangesAsync();

        var productService = new ProductService(context, logger);

        // Act: Retrieve product by ID
        var result = await productService.GetProductByIdAsync(1);

        // Assert: Verify correct product is returned
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Laptop Gaming");
        result.Price.Should().Be(1500);
        result.Should().BeOfType<ProductDto>();
    }

    /// <summary>
    /// Verifies that <see cref="ProductService.GetProductByIdAsync"/> returns null
    /// when attempting to retrieve a product with a non-existent identifier.
    /// </summary>
    [Fact]
    public async Task GetProductById_WithInvalidId_ShouldReturnNull()
    {
        // Arrange: Set up empty database
        using var context = CreateInMemoryContext();
        var logger = CreateMockLogger();

        var productService = new ProductService(context, logger);

        // Act: Attempt to retrieve non-existent product
        var result = await productService.GetProductByIdAsync(999);

        // Assert: Verify null is returned for non-existent product
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that <see cref="ProductService.AddProductAsync"/> successfully creates a new product
    /// with valid input data and persists it to the database.
    /// </summary>
    [Fact]
    public async Task AddProductAsync_WithValidData_ShouldCreateProduct()
    {
        // Arrange: Set up service and prepare product data
        using var context = CreateInMemoryContext();
        var logger = CreateMockLogger();
        var productService = new ProductService(context, logger);

        var newProduct = new CreateProductDto
        {
            Name = "Monitor 4K",
            Description = "27-inch 4K monitor with HDR",
            Price = 400,
            Stock = 7,
            CategoryId = 1
        };

        // Act: Create the product
        var (result, success) = await productService.AddProductAsync(newProduct);

        // Assert: Verify creation was successful
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Monitor 4K");
        result.Should().BeOfType<ProductDto>();

        // Verify product was persisted to database
        var savedProduct = await context.Products.FindAsync(result.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be("Monitor 4K");
    }

    /// <summary>
    /// Verifies that <see cref="ProductService.AddProductAsync"/> successfully creates products
    /// with various input data combinations using parameterized test scenarios.
    /// </summary>
    /// <param name="name">The product name to test.</param>
    /// <param name="price">The product price to test.</param>
    /// <param name="stock">The product stock quantity to test.</param>
    [Theory]
    [InlineData("Laptop", 1000, 5)]
    [InlineData("Mouse", 25, 10)]
    [InlineData("Mechanical Keyboard", 150, 3)]
    public async Task AddProductAsync_WithDifferentData_ShouldCreateCorrectly(string name, decimal price, int stock)
    {
        // Arrange: Set up service and prepare product data with test parameters
        using var context = CreateInMemoryContext();
        var logger = CreateMockLogger();
        var productService = new ProductService(context, logger);

        var product = new CreateProductDto
        {
            Name = name,
            Description = $"Description for {name}",
            Price = price,
            Stock = stock,
            CategoryId = 1
        };

        // Act: Create the product with test data
        var (result, success) = await productService.AddProductAsync(product);

        // Assert: Verify product was created with correct values
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Name.Should().Be(name);
        result.Price.Should().Be(price);
        result.Stock.Should().Be(stock);
    }

}
