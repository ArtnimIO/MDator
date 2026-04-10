using MDator;
using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;

namespace MDator.Samples.WebApi.Features.Products;

public record ListProductsQuery(int Skip = 0, int Take = 50) : IRequest<IReadOnlyList<Product>>;

public sealed class ListProductsHandler(IProductRepository repo)
    : IRequestHandler<ListProductsQuery, IReadOnlyList<Product>>
{
    public Task<IReadOnlyList<Product>> Handle(ListProductsQuery request, CancellationToken ct)
        => repo.GetAllAsync(request.Skip, request.Take, ct);
}

public static class ListProductsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", async (int? skip, int? take, IMediator mediator) =>
        {
            var products = await mediator.Send(new ListProductsQuery(skip ?? 0, take ?? 50));
            return Results.Ok(products);
        });
    }
}
