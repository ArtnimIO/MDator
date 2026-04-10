using MDator.Samples.WebApi.Features.Products;

namespace MDator.Samples.WebApi.Processors;

public sealed class CreateProductTimingPreProcessor(ILogger<CreateProductTimingPreProcessor> logger)
    : IRequestPreProcessor<CreateProductCommand>
{
    public Task Process(CreateProductCommand request, CancellationToken ct)
    {
        logger.LogDebug("Processing CreateProductCommand at {Timestamp}: Name={Name}, Sku={Sku}",
            DateTime.UtcNow, request.Name, request.Sku);
        return Task.CompletedTask;
    }
}
