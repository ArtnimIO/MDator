using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;

namespace MDator.Samples.WebApi.Features.Stock;

public record GetStockAlertsQuery(int Count = 20) : IRequest<IReadOnlyList<StockAlert>>;

public sealed class GetStockAlertsHandler(IStockAlertRepository repo)
    : IRequestHandler<GetStockAlertsQuery, IReadOnlyList<StockAlert>>
{
    public Task<IReadOnlyList<StockAlert>> Handle(GetStockAlertsQuery request, CancellationToken ct)
        => repo.GetRecentAsync(request.Count, ct);
}

public static class GetStockAlertsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/alerts", async (int? count, IMediator mediator) =>
        {
            var alerts = await mediator.Send(new GetStockAlertsQuery(count ?? 20));
            return Results.Ok(alerts);
        });
    }
}
