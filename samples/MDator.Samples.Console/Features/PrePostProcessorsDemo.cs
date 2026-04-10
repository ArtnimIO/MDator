using MDator;
using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;
using MDator.Samples.Console.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MDator.Samples.Console.Features;

// --- Pre-Processor ---

public sealed class AuditPreProcessor : IRequestPreProcessor<CreateProductCommand>
{
    public Task Process(CreateProductCommand request, CancellationToken ct)
    {
        System.Console.ForegroundColor = ConsoleColor.DarkCyan;
        System.Console.WriteLine($"  [PreProcessor] About to create product: Name={request.Name}, Sku={request.Sku}, Price={request.Price:C}");
        System.Console.ResetColor();
        return Task.CompletedTask;
    }
}

// --- Post-Processor ---

public sealed class AuditPostProcessor : IRequestPostProcessor<CreateProductCommand, Product>
{
    public Task Process(CreateProductCommand request, Product response, CancellationToken ct)
    {
        System.Console.ForegroundColor = ConsoleColor.DarkGreen;
        System.Console.WriteLine($"  [PostProcessor] Product created successfully: Id={response.Id}, Name={response.Name}");
        System.Console.ResetColor();
        return Task.CompletedTask;
    }
}

// --- Demo ---

public static class PrePostProcessorsDemo
{
    public static async Task RunAsync()
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine("--- 7. Pre/Post Processors ---");
        System.Console.ResetColor();
        System.Console.WriteLine("Demonstrates IRequestPreProcessor (runs before handler) and IRequestPostProcessor (runs after handler).");
        System.Console.WriteLine("Processors run inside the innermost behavior scope.\n");

        var services = new ServiceCollection();
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddMDator();
        await using var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("[CreateProduct with Pre/Post Processors]");
        System.Console.ResetColor();
        System.Console.WriteLine("  Watch the pipeline order:\n");

        var product = await mediator.Send(new CreateProductCommand("Doohickey", "DHK-001", 14.99m));

        System.Console.WriteLine($"\n  Final result: {product.Name} ({product.Id})");
        System.Console.WriteLine("\n  Pipeline order: Open Behaviors → PreProcessor → Handler → PostProcessor → Open Behaviors (exit)");
    }
}
