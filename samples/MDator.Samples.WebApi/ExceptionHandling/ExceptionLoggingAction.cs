using MDator;
using MDator.Samples.WebApi.Features.Products;

namespace MDator.Samples.WebApi.ExceptionHandling;

public sealed class GetProductExceptionLoggingAction(ILogger<GetProductExceptionLoggingAction> logger)
    : IRequestExceptionAction<GetProductQuery, Exception>
{
    public Task Execute(GetProductQuery request, Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "Exception while handling GetProductQuery(Id={ProductId}): {Message}",
            request.Id, exception.Message);
        return Task.CompletedTask;
    }
}
