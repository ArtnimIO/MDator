using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;
using MDator.Samples.Console.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MDator.Samples.Console.Features;

// --- Requests ---

public record CreateProductCommand(string Name, string Sku, decimal Price) : IRequest<Product>;

public record GetProductByIdQuery(Guid Id) : IRequest<Product>;

public record DeleteProductCommand(Guid Id) : IRequest;

// --- Handlers ---

public sealed class CreateProductHandler(IProductRepository repo) : IRequestHandler<CreateProductCommand, Product>
{
    public async Task<Product> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product { Name = request.Name, Sku = request.Sku, Price = request.Price };
        await repo.AddAsync(product, ct);
        return product;
    }
}

public sealed class GetProductByIdHandler(IProductRepository repo) : IRequestHandler<GetProductByIdQuery, Product>
{
    public async Task<Product> Handle(GetProductByIdQuery request, CancellationToken ct)
        => await repo.GetByIdAsync(request.Id, ct) ?? throw new KeyNotFoundException($"Product {request.Id} not found.");
}

public sealed class DeleteProductHandler(IProductRepository repo) : IRequestHandler<DeleteProductCommand>
{
    public Task Handle(DeleteProductCommand request, CancellationToken ct)
        => repo.DeleteAsync(request.Id, ct);
}

// --- Demo ---

public static class BasicRequestsDemo
{
    public static async Task RunAsync()
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine("--- 1. Basic Requests ---");
        System.Console.ResetColor();
        System.Console.WriteLine("Demonstrates Send<TResponse>() for commands/queries and Send() for void requests.\n");

        var services = new ServiceCollection();
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddMDator();
        await using var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        // Command returning a response
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("[CreateProduct Command]");
        System.Console.ResetColor();
        var product = await mediator.Send(new CreateProductCommand("Widget", "WDG-001", 9.99m));
        System.Console.WriteLine($"  Created: {product.Name} (Id: {product.Id}, Price: {product.Price:C})");

        // Query
        System.Console.WriteLine();
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("[GetProductById Query]");
        System.Console.ResetColor();
        var fetched = await mediator.Send(new GetProductByIdQuery(product.Id));
        System.Console.WriteLine($"  Fetched: {fetched.Name}");

        // Void request
        System.Console.WriteLine();
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("[DeleteProduct Void Request]");
        System.Console.ResetColor();
        await mediator.Send(new DeleteProductCommand(product.Id));
        try
        {
            await mediator.Send(new GetProductByIdQuery(product.Id));
        }
        catch (KeyNotFoundException)
        {
            System.Console.WriteLine("  After delete: not found (deleted)");
        }
    }
}
