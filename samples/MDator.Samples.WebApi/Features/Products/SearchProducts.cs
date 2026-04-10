using MDator;
using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;

namespace MDator.Samples.WebApi.Features.Products;

public record SearchProductsQuery(string Term) : IRequest<IReadOnlyList<Product>>;

public sealed class SearchProductsHandler(IProductRepository repo)
    : IRequestHandler<SearchProductsQuery, IReadOnlyList<Product>>
{
    public Task<IReadOnlyList<Product>> Handle(SearchProductsQuery request, CancellationToken ct)
        => repo.SearchAsync(request.Term, ct);
}

public static class SearchProductsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/search", async (string term, IMediator mediator) =>
        {
            var products = await mediator.Send(new SearchProductsQuery(term));
            return Results.Ok(products);
        });
    }
}
