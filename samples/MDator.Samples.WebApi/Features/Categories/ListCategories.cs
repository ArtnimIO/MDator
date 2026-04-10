using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;

namespace MDator.Samples.WebApi.Features.Categories;

public record ListCategoriesQuery() : IRequest<IReadOnlyList<Category>>;

public sealed class ListCategoriesHandler(ICategoryRepository repo)
    : IRequestHandler<ListCategoriesQuery, IReadOnlyList<Category>>
{
    public Task<IReadOnlyList<Category>> Handle(ListCategoriesQuery request, CancellationToken ct)
        => repo.GetAllAsync(ct);
}

public static class ListCategoriesEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", async (IMediator mediator) =>
        {
            var categories = await mediator.Send(new ListCategoriesQuery());
            return Results.Ok(categories);
        });
    }
}
