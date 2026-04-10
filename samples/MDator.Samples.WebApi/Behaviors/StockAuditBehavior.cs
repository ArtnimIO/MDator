using MDator.Samples.WebApi.Features.Stock;

namespace MDator.Samples.WebApi.Behaviors;

public sealed class StockAuditBehavior(ILogger<StockAuditBehavior> logger)
    : IPipelineBehavior<AdjustStockCommand, AdjustStockResult>
{
    public async Task<AdjustStockResult> Handle(
        AdjustStockCommand request,
        RequestHandlerDelegate<AdjustStockResult> next,
        CancellationToken ct)
    {
        logger.LogInformation("Stock adjustment requested: ProductId={ProductId}, Delta={Delta}",
            request.ProductId, request.Delta);

        var result = await next();

        logger.LogInformation("Stock adjustment completed: ProductId={ProductId}, NewQuantity={NewQuantity}",
            request.ProductId, result.NewQuantity);

        return result;
    }
}
