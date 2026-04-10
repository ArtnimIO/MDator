using MDator;
using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;

namespace MDator.Samples.WebApi.Features.Categories;

public record GetCategoryQuery(Guid Id) : IRequest<GetCategoryResult>;

public record GetCategoryResult(Category? Category);

public sealed class GetCategoryHandler(ICategoryRepository repo) : IRequestHandler<GetCategoryQuery, GetCategoryResult>
{
    public async Task<GetCategoryResult> Handle(GetCategoryQuery request, CancellationToken ct)
    {
        var category = await repo.GetByIdAsync(request.Id, ct);
        return new GetCategoryResult(category);
    }
}

public static class GetCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCategoryQuery(id));
            return result.Category is not null ? Results.Ok(result.Category) : Results.NotFound();
        });
    }
}
