using MDator;
using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;
using MDator.Samples.Console.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MDator.Samples.Console.Features;

// --- Custom Exception ---

public class InsufficientStockException(Guid productId, int requested, int available)
    : Exception($"Cannot reduce stock for product {productId}: requested {requested}, available {available}")
{
    public Guid ProductId { get; } = productId;
    public int Requested { get; } = requested;
    public int Available { get; } = available;
}

// --- Request / Response ---

public record ReduceStockCommand(Guid ProductId, int Quantity) : IRequest<ReduceStockResult>;

public record ReduceStockResult(bool Success, string Message);

// --- Handler ---

public sealed class ReduceStockHandler(IProductRepository repo) : IRequestHandler<ReduceStockCommand, ReduceStockResult>
{
    public async Task<ReduceStockResult> Handle(ReduceStockCommand request, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(request.ProductId, ct);
        if (product is null)
            throw new KeyNotFoundException($"Product {request.ProductId} not found.");

        if (product.StockQuantity < request.Quantity)
            throw new InsufficientStockException(request.ProductId, request.Quantity, product.StockQuantity);

        product.StockQuantity -= request.Quantity;
        await repo.UpdateAsync(product, ct);
        return new ReduceStockResult(true, $"Reduced stock to {product.StockQuantity}");
    }
}

// --- Exception Handler (converts exception to response) ---

public sealed class InsufficientStockExceptionHandler
    : IRequestExceptionHandler<ReduceStockCommand, ReduceStockResult, InsufficientStockException>
{
    public Task Handle(
        ReduceStockCommand request,
        InsufficientStockException exception,
        RequestExceptionHandlerState<ReduceStockResult> state,
        CancellationToken ct)
    {
        System.Console.ForegroundColor = ConsoleColor.DarkYellow;
        System.Console.WriteLine($"  [ExceptionHandler] Caught InsufficientStockException, converting to error result");
        System.Console.ResetColor();
        state.SetHandled(new ReduceStockResult(false, exception.Message));
        return Task.CompletedTask;
    }
}

// --- Exception Action (observes all exceptions, e.g. for logging) ---

public sealed class StockExceptionLoggingAction
    : IRequestExceptionAction<ReduceStockCommand, Exception>
{
    public Task Execute(ReduceStockCommand request, Exception exception, CancellationToken ct)
    {
        System.Console.ForegroundColor = ConsoleColor.DarkRed;
        System.Console.WriteLine($"  [ExceptionAction] Logged: {exception.GetType().Name} — {exception.Message}");
        System.Console.ResetColor();
        return Task.CompletedTask;
    }
}

// --- Demo ---

public static class ExceptionHandlingDemo
{
    public static async Task RunAsync()
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine("--- 6. Exception Handling ---");
        System.Console.ResetColor();
        System.Console.WriteLine("Demonstrates IRequestExceptionHandler (converts to response) and IRequestExceptionAction (observes).\n");

        var services = new ServiceCollection();
        var repo = new InMemoryProductRepository();
        await repo.AddAsync(new Product { Name = "Widget", Sku = "WDG-001", Price = 9.99m, StockQuantity = 5 });
        var product = (await repo.GetAllAsync()).First();
        services.AddSingleton<IProductRepository>(repo);
        services.AddMDator();
        await using var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        // Successful reduction
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("[Reduce stock by 3 (available: 5)]");
        System.Console.ResetColor();
        var result = await mediator.Send(new ReduceStockCommand(product.Id, 3));
        System.Console.WriteLine($"  Result: Success={result.Success}, {result.Message}\n");

        // Insufficient stock — exception handler converts to error result
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("[Reduce stock by 10 (available: 2)]");
        System.Console.ResetColor();
        result = await mediator.Send(new ReduceStockCommand(product.Id, 10));
        System.Console.WriteLine($"  Result: Success={result.Success}, {result.Message}");
    }
}
