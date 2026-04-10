using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;

namespace MDator.Samples.WebApi.Features.Categories;

public record CreateCategoryCommand(string Name, string? Description) : IRequest<Category>;

public sealed class CreateCategoryHandler(ICategoryRepository repo) : IRequestHandler<CreateCategoryCommand, Category>
{
    public async Task<Category> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var category = new Category { Name = request.Name, Description = request.Description };
        await repo.AddAsync(category, ct);
        return category;
    }
}

public static class CreateCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateCategoryCommand command, IMediator mediator) =>
        {
            var category = await mediator.Send(command);
            return Results.Created($"/categories/{category.Id}", category);
        });
    }
}
