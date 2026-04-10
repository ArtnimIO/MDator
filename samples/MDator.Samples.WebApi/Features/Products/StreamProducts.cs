using System.Runtime.CompilerServices;
using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;

namespace MDator.Samples.WebApi.Features.Products;

public record StreamProductsQuery() : IStreamRequest<Product>;

public sealed class StreamProductsHandler(IProductRepository repo)
    : IStreamRequestHandler<StreamProductsQuery, Product>
{
    public async IAsyncEnumerable<Product> Handle(
        StreamProductsQuery request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var products = await repo.GetAllAsync(take: int.MaxValue, ct: ct);
        foreach (var product in products)
        {
            yield return product;
        }
    }
}

public static class StreamProductsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/stream", (IMediator mediator) =>
        {
            async IAsyncEnumerable<Product> Stream([EnumeratorCancellation] CancellationToken ct = default)
            {
                await foreach (var product in mediator.CreateStream(new StreamProductsQuery(), ct))
                {
                    yield return product;
                }
            }
            return Stream();
        });
    }
}
