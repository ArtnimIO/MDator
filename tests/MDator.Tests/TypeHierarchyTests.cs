using Microsoft.Extensions.DependencyInjection;

namespace MDator.Tests;

public record BaseQuery(string Value) : IRequest<string>;
public record DerivedQuery(string Value, int Extra) : BaseQuery(Value);

public sealed class BaseQueryHandler : IRequestHandler<BaseQuery, string>
{
    public Task<string> Handle(BaseQuery request, CancellationToken cancellationToken)
        => Task.FromResult($"base:{request.Value}");
}

public sealed class DerivedQueryHandler : IRequestHandler<DerivedQuery, string>
{
    public Task<string> Handle(DerivedQuery request, CancellationToken cancellationToken)
        => Task.FromResult($"derived:{request.Value}:{request.Extra}");
}

public class TypeHierarchyTests
{
    [Fact]
    public async Task Send_derived_routes_to_derived_handler()
    {
        var sp = TestServices.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new DerivedQuery("hello", 42));

        Assert.Equal("derived:hello:42", result);
    }

    [Fact]
    public async Task Send_base_routes_to_base_handler()
    {
        var sp = TestServices.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new BaseQuery("world"));

        Assert.Equal("base:world", result);
    }

    [Fact]
    public async Task Send_object_derived_routes_to_derived_handler()
    {
        var sp = TestServices.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send((object)new DerivedQuery("hello", 42));

        Assert.Equal("derived:hello:42", result);
    }
}
