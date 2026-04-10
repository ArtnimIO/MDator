using MDator.Samples.Domain.Interfaces;
using MDator.Samples.WebApi.ExceptionHandling;
using MDator.Samples.WebApi.Notifications;

namespace MDator.Samples.WebApi.Features.Stock;

public record AdjustStockCommand(Guid ProductId, int Delta) : IRequest<AdjustStockResult>;

public record AdjustStockResult(Guid ProductId, int NewQuantity);

public sealed class AdjustStockHandler(IProductRepository repo, IPublisher publisher)
    : IRequestHandler<AdjustStockCommand, AdjustStockResult>
{
    public async Task<AdjustStockResult> Handle(AdjustStockCommand request, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(request.ProductId, ct)
            ?? throw new ProductNotFoundException(request.ProductId);

        var newQuantity = product.StockQuantity + request.Delta;
        if (newQuantity < 0)
            throw new InvalidOperationException(
                $"Stock adjustment would result in negative quantity ({newQuantity}) for product '{product.Name}'.");

        product.StockQuantity = newQuantity;
        await repo.UpdateAsync(product, ct);

        await publisher.Publish(
            new StockLevelChangedNotification(product.Id, product.Name, newQuantity), ct);

        return new AdjustStockResult(product.Id, newQuantity);
    }
}

public static class AdjustStockEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/{productId:guid}/adjust", async (Guid productId, AdjustStockRequest body, IMediator mediator) =>
        {
            var result = await mediator.Send(new AdjustStockCommand(productId, body.Delta));
            return Results.Ok(result);
        });
    }

    public record AdjustStockRequest(int Delta);
}
