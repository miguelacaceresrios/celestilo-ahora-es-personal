using DotNetEnv;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddControllers();

builder.Services.AddDbContext<ProductDbContext>(options => options.UseSqlServer(Environment.GetEnvironmentVariable("CONNECTION_STRING")));

var app = builder.Build();

app.MapControllers();



app.Run();
