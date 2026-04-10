using MDator.Samples.Domain.Models;
using MDator.Samples.WebApi.Features.Products;

namespace MDator.Samples.WebApi.Processors;

public sealed class CreateProductPostProcessor(ILogger<CreateProductPostProcessor> logger)
    : IRequestPostProcessor<CreateProductCommand, Product>
{
    public Task Process(CreateProductCommand request, Product response, CancellationToken ct)
    {
        logger.LogDebug("Product created: Id={ProductId}, Name={Name}, Sku={Sku}",
            response.Id, response.Name, response.Sku);
        return Task.CompletedTask;
    }
}
