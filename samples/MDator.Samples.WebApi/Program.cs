using FluentValidation;
using MDator;
using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;
using MDator.Samples.WebApi.Behaviors;
using MDator.Samples.WebApi.Infrastructure;
using MDator.Samples.WebApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Products = MDator.Samples.WebApi.Features.Products;
using Categories = MDator.Samples.WebApi.Features.Categories;
using Stock = MDator.Samples.WebApi.Features.Stock;

[assembly: MDator.OpenBehavior(typeof(LoggingBehavior<,>), Order = 0)]
[assembly: MDator.OpenBehavior(typeof(ValidationBehavior<,>), Order = 1)]
[assembly: MDator.OpenBehavior(typeof(TransactionBehavior<,>), Order = 2)]

var builder = WebApplication.CreateBuilder(args);

// EF Core (in-memory)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("MDatorSamples"));

// Repositories
builder.Services.AddScoped<IProductRepository, EfProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EfCategoryRepository>();
builder.Services.AddScoped<IStockAlertRepository, EfStockAlertRepository>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// MDator
builder.Services.AddMDator();

var app = builder.Build();

// Seed data
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var electronics = new Category { Name = "Electronics", Description = "Electronic devices and components" };
    var tools = new Category { Name = "Tools", Description = "Hand and power tools" };
    db.Categories.AddRange(electronics, tools);

    db.Products.AddRange(
        new Product { Name = "Wireless Mouse", Sku = "ELEC-001", Price = 29.99m, StockQuantity = 150, CategoryId = electronics.Id, Description = "Ergonomic wireless mouse" },
        new Product { Name = "USB-C Hub", Sku = "ELEC-002", Price = 49.99m, StockQuantity = 75, CategoryId = electronics.Id, Description = "7-port USB-C hub" },
        new Product { Name = "Mechanical Keyboard", Sku = "ELEC-003", Price = 89.99m, StockQuantity = 3, CategoryId = electronics.Id, Description = "RGB mechanical keyboard" },
        new Product { Name = "Cordless Drill", Sku = "TOOL-001", Price = 79.99m, StockQuantity = 40, CategoryId = tools.Id, Description = "18V cordless drill" },
        new Product { Name = "Socket Set", Sku = "TOOL-002", Price = 34.99m, StockQuantity = 0, CategoryId = tools.Id, Description = "72-piece socket set" }
    );

    await db.SaveChangesAsync();
}

// Product endpoints
var products = app.MapGroup("/products").WithTags("Products");
Products.CreateProductEndpoint.Map(products);
Products.UpdateProductEndpoint.Map(products);
Products.DeleteProductEndpoint.Map(products);
Products.GetProductEndpoint.Map(products);
Products.ListProductsEndpoint.Map(products);
Products.SearchProductsEndpoint.Map(products);
Products.StreamProductsEndpoint.Map(products);

// Category endpoints
var categories = app.MapGroup("/categories").WithTags("Categories");
Categories.CreateCategoryEndpoint.Map(categories);
Categories.GetCategoryEndpoint.Map(categories);
Categories.ListCategoriesEndpoint.Map(categories);

// Stock endpoints
var stock = app.MapGroup("/stock").WithTags("Stock");
Stock.AdjustStockEndpoint.Map(stock);
Stock.GetStockAlertsEndpoint.Map(stock);
Stock.StreamStockAlertsEndpoint.Map(stock);

app.Run();

public partial class Program;
