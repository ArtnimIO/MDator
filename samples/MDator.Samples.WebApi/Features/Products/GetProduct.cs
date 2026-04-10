using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;
using MDator.Samples.WebApi.ExceptionHandling;

namespace MDator.Samples.WebApi.Features.Products;

public record GetProductQuery(Guid Id) : IRequest<GetProductResult>;

public record GetProductResult(Product? Product, string? Error = null);

public sealed class GetProductHandler(IProductRepository repo) : IRequestHandler<GetProductQuery, GetProductResult>
{
    public async Task<GetProductResult> Handle(GetProductQuery request, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new ProductNotFoundException(request.Id);
        return new GetProductResult(product);
    }
}

public static class GetProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductQuery(id));
            return result.Product is not null
                ? Results.Ok(result.Product)
                : Results.NotFound(new { error = result.Error });
        });
    }
}
