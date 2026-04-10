using Microsoft.Extensions.DependencyInjection;

namespace MDator.Tests;

public record GetUserQuery(int Id) : IRequest<User>;
public record User(int Id, string Name);

public sealed class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    public Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
        => Task.FromResult(new User(request.Id, $"user-{request.Id}"));
}

public class BasicRequestTests
{
    [Fact]
    public async Task Send_resolves_handler_and_returns_response()
    {
        var sp = TestServices.Build();

        var mediator = sp.GetRequiredService<IMediator>();
        var user = await mediator.Send(new GetUserQuery(42));

        Assert.Equal(42, user.Id);
        Assert.Equal("user-42", user.Name);
    }

    [Fact]
    public async Task Send_object_path_works()
    {
        var sp = TestServices.Build();

        var mediator = sp.GetRequiredService<IMediator>();
        var result = await mediator.Send((object)new GetUserQuery(7));
        var user = Assert.IsType<User>(result);
        Assert.Equal(7, user.Id);
    }
}
