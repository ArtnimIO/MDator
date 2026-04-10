using FluentValidation;
using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;
using MDator.Samples.WebApi.ExceptionHandling;

namespace MDator.Samples.WebApi.Features.Products;

public record UpdateProductCommand(Guid Id, string Name, string Sku, string? Description, decimal Price, Guid? CategoryId)
    : IRequest<Product>;

public sealed class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

public sealed class UpdateProductHandler(IProductRepository repo) : IRequestHandler<UpdateProductCommand, Product>
{
    public async Task<Product> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new ProductNotFoundException(request.Id);

        product.Name = request.Name;
        product.Sku = request.Sku;
        product.Description = request.Description;
        product.Price = request.Price;
        product.CategoryId = request.CategoryId;

        await repo.UpdateAsync(product, ct);
        return product;
    }
}

public static class UpdateProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest body, IMediator mediator) =>
        {
            var product = await mediator.Send(new UpdateProductCommand(id, body.Name, body.Sku, body.Description, body.Price, body.CategoryId));
            return Results.Ok(product);
        });
    }

    public record UpdateProductRequest(string Name, string Sku, string? Description, decimal Price, Guid? CategoryId);
}
