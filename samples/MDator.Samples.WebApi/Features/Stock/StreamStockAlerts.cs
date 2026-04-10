using System.Runtime.CompilerServices;
using MDator;
using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;

namespace MDator.Samples.WebApi.Features.Stock;

public record StreamStockAlertsQuery() : IStreamRequest<StockAlert>;

public sealed class StreamStockAlertsHandler(IStockAlertRepository repo)
    : IStreamRequestHandler<StreamStockAlertsQuery, StockAlert>
{
    public async IAsyncEnumerable<StockAlert> Handle(
        StreamStockAlertsQuery request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var alerts = await repo.GetRecentAsync(100, ct);
        foreach (var alert in alerts)
        {
            yield return alert;
        }
    }
}

public static class StreamStockAlertsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/alerts/stream", (IMediator mediator) =>
        {
            async IAsyncEnumerable<StockAlert> Stream([EnumeratorCancellation] CancellationToken ct = default)
            {
                await foreach (var alert in mediator.CreateStream(new StreamStockAlertsQuery(), ct))
                {
                    yield return alert;
                }
            }
            return Stream();
        });
    }
}
