using System.Diagnostics;
using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;
using MDator.Samples.Console.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

[assembly: MDator.OpenBehavior(typeof(MDator.Samples.Console.Features.DemoLoggingBehavior<,>), Order = 0)]
[assembly: MDator.OpenBehavior(typeof(MDator.Samples.Console.Features.DemoTimingBehavior<,>), Order = 1)]

namespace MDator.Samples.Console.Features;

// --- Request ---

public record GetAllProductsQuery() : IRequest<IReadOnlyList<Product>>;

// --- Handler ---

public sealed class GetAllProductsHandler(IProductRepository repo)
    : IRequestHandler<GetAllProductsQuery, IReadOnlyList<Product>>
{
    public Task<IReadOnlyList<Product>> Handle(GetAllProductsQuery request, CancellationToken ct)
        => repo.GetAllAsync(ct: ct);
}

// --- Open Behaviors ---

public sealed class DemoLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        System.Console.ForegroundColor = ConsoleColor.Blue;
        System.Console.WriteLine($"  [Logging] → Entering {typeof(TRequest).Name}");
        System.Console.ResetColor();
        var response = await next();
        System.Console.ForegroundColor = ConsoleColor.Blue;
        System.Console.WriteLine($"  [Logging] ← Exiting {typeof(TRequest).Name}");
        System.Console.ResetColor();
        return response;
    }
}

public sealed class DemoTimingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        System.Console.ForegroundColor = ConsoleColor.Magenta;
        System.Console.WriteLine($"  [Timing] Start {typeof(TRequest).Name}");
        System.Console.ResetColor();
        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();
        System.Console.ForegroundColor = ConsoleColor.Magenta;
        System.Console.WriteLine($"  [Timing] {typeof(TRequest).Name} took {sw.ElapsedMilliseconds}ms");
        System.Console.ResetColor();
        return response;
    }
}

// --- Closed Behavior (only wraps CreateProductCommand from BasicRequestsDemo) ---

public sealed class ProductCommandAuditBehavior : IPipelineBehavior<CreateProductCommand, Product>
{
    public async Task<Product> Handle(CreateProductCommand request, RequestHandlerDelegate<Product> next, CancellationToken ct)
    {
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"  [Audit] Product command: creating '{request.Name}'");
        System.Console.ResetColor();
        var result = await next();
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"  [Audit] Product created with Id {result.Id}");
        System.Console.ResetColor();
        return result;
    }
}

// --- Demo ---

public static class PipelineBehaviorsDemo
{
    public static async Task RunAsync()
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine("--- 3. Pipeline Behaviors ---");
        System.Console.ResetColor();
        System.Console.WriteLine("Demonstrates open behaviors (Logging Order=0, Timing Order=1) and a closed behavior.\n");
        System.Console.WriteLine("Open behaviors wrap ALL requests. Closed behaviors wrap only their specific request type.\n");

        var services = new ServiceCollection();
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddMDator();
        await using var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        // Open behaviors wrap this query
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("[GetAllProducts — open behaviors only]");
        System.Console.ResetColor();
        await mediator.Send(new GetAllProductsQuery());

        System.Console.WriteLine();

        // Open + closed behaviors wrap this command
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("[CreateProduct — open + closed behaviors]");
        System.Console.ResetColor();
        await mediator.Send(new CreateProductCommand("Gizmo", "GZM-001", 19.99m));
    }
}
