using MDator;
using MDator.Samples.WebApi.Features.Products;

namespace MDator.Samples.WebApi.ExceptionHandling;

public class ProductNotFoundException(Guid productId)
    : Exception($"Product with Id '{productId}' was not found.")
{
    public Guid ProductId { get; } = productId;
}

public sealed class ProductNotFoundExceptionHandler
    : IRequestExceptionHandler<GetProductQuery, GetProductResult, ProductNotFoundException>
{
    public Task Handle(
        GetProductQuery request,
        ProductNotFoundException exception,
        RequestExceptionHandlerState<GetProductResult> state,
        CancellationToken ct)
    {
        state.SetHandled(new GetProductResult(null, exception.Message));
        return Task.CompletedTask;
    }
}
