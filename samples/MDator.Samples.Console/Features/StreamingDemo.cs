using System.Runtime.CompilerServices;
using MDator;
using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;
using MDator.Samples.Console.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MDator.Samples.Console.Features;

// --- Request ---

public record StreamAllProductsQuery() : IStreamRequest<Product>;

// --- Handler ---

public sealed class StreamAllProductsHandler(IProductRepository repo)
    : IStreamRequestHandler<StreamAllProductsQuery, Product>
{
    public async IAsyncEnumerable<Product> Handle(
        StreamAllProductsQuery request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var products = await repo.GetAllAsync(take: int.MaxValue, ct: ct);
        foreach (var product in products)
        {
            await Task.Delay(100, ct); // Simulate async work
            yield return product;
        }
    }
}

// --- Demo ---

public static class StreamingDemo
{
    public static async Task RunAsync()
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine("--- 4. Streaming ---");
        System.Console.ResetColor();
        System.Console.WriteLine("Demonstrates CreateStream() with IAsyncEnumerable<T>.\n");
        System.Console.WriteLine("Products stream in one at a time (100ms delay each to simulate async work):\n");

        var services = new ServiceCollection();
        var repo = new InMemoryProductRepository();
        // Seed 20 products
        for (var i = 1; i <= 20; i++)
        {
            await repo.AddAsync(new Product
            {
                Name = $"Product {i}",
                Sku = $"PRD-{i:D3}",
                Price = i * 5.50m,
                StockQuantity = i * 10
            });
        }
        services.AddSingleton<IProductRepository>(repo);
        services.AddMDator();
        await using var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        var count = 0;
        await foreach (var product in mediator.CreateStream(new StreamAllProductsQuery()))
        {
            count++;
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.Write($"  [{count:D2}] ");
            System.Console.ResetColor();
            System.Console.WriteLine($"{product.Name} — {product.Sku} — {product.Price:C} — Stock: {product.StockQuantity}");
        }

        System.Console.WriteLine($"\nStreamed {count} products.");
    }
}
