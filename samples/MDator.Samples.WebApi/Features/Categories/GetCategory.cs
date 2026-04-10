using MDator;
using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;

namespace MDator.Samples.WebApi.Features.Categories;

public record GetCategoryQuery(Guid Id) : IRequest<Category?>;

public sealed class GetCategoryHandler(ICategoryRepository repo) : IRequestHandler<GetCategoryQuery, Category?>
{
    public Task<Category?> Handle(GetCategoryQuery request, CancellationToken ct)
        => repo.GetByIdAsync(request.Id, ct);
}

public static class GetCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var category = await mediator.Send(new GetCategoryQuery(id));
            return category is not null ? Results.Ok(category) : Results.NotFound();
        });
    }
}
