using FluentValidation;
using MDator;
using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;

namespace MDator.Samples.WebApi.Features.Products;

public record CreateProductCommand(string Name, string Sku, string? Description, decimal Price, int StockQuantity, Guid? CategoryId)
    : IRequest<Product>;

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateProductHandler(IProductRepository repo, IPublisher publisher)
    : IRequestHandler<CreateProductCommand, Product>
{
    public async Task<Product> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product
        {
            Name = request.Name,
            Sku = request.Sku,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId
        };

        await repo.AddAsync(product, ct);
        await publisher.Publish(new Notifications.ProductCreatedNotification(product.Id, product.Name), ct);
        return product;
    }
}

public static class CreateProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateProductCommand command, IMediator mediator) =>
        {
            var product = await mediator.Send(command);
            return Results.Created($"/products/{product.Id}", product);
        });
    }
}
