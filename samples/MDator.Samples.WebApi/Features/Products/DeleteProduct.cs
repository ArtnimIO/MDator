using MDator;
using MDator.Samples.Domain.Interfaces;

namespace MDator.Samples.WebApi.Features.Products;

public record DeleteProductCommand(Guid Id) : IRequest;

public sealed class DeleteProductHandler(IProductRepository repo) : IRequestHandler<DeleteProductCommand>
{
    public Task Handle(DeleteProductCommand request, CancellationToken ct)
        => repo.DeleteAsync(request.Id, ct);
}

public static class DeleteProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new DeleteProductCommand(id));
            return Results.NoContent();
        });
    }
}
