using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using backend.Configuration;
using backend.Data;
using backend.Services;

/// <summary>
/// Main entry point for the ASP.NET Core application.
/// Configures services, middleware, and database seeding.
/// </summary>

// Load environment variables from .env file
Env.Load();
var builder = WebApplication.CreateBuilder(args);

// Configure database contexts
builder.Services.AddDbContext<ProductDbContext>(options => options.UseSqlServer(Environment.GetEnvironmentVariable("CONNECTION_STRING")));
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Environment.GetEnvironmentVariable("CONNECTION_STRING")));

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password configuration
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User configuration
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT authentication
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!;
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
    };
});

// Register application services
builder.Services.AddControllers();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<IUserManagementService, UserManagementService>();

builder.Services.AddSingleton<JwtSettings>();
builder.Services.AddLogging();

var app = builder.Build();

// Seed admin user on application startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedAdminUser(services);
}

// Configure middleware pipeline
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>
/// Seeds the database with a default admin user if one does not exist.
/// Creates the Admin and User roles if they don't exist.
/// </summary>
/// <param name="serviceProvider">The service provider to resolve required services.</param>
async Task SeedAdminUser(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Create roles if they don't exist
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    if (!await roleManager.RoleExistsAsync("User"))
        await roleManager.CreateAsync(new IdentityRole("User"));

    // Create admin user if it doesn't exist
    var adminEmail = "admin@example.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser is null)
    {
        adminUser = new IdentityUser
        {
            UserName = "admin",
            Email = adminEmail,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(adminUser, "Admin123!");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}