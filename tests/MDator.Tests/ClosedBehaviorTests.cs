using Microsoft.Extensions.DependencyInjection;

namespace MDator.Tests;

public record ComputeQuery(int X) : IRequest<int>;

public sealed class ComputeHandler : IRequestHandler<ComputeQuery, int>
{
    public Task<int> Handle(ComputeQuery request, CancellationToken ct) => Task.FromResult(request.X * 2);
}

/// <summary>
/// A concrete (non-generic) closed pipeline behavior — discovered by the
/// generator's class-scan path, registered under
/// <c>IPipelineBehavior&lt;ComputeQuery, int&gt;</c>, and picked up by the
/// per-request runtime enumeration fallback.
/// </summary>
public sealed class DoubleItBehavior : IPipelineBehavior<ComputeQuery, int>
{
    public async Task<int> Handle(ComputeQuery request, RequestHandlerDelegate<int> next, CancellationToken ct)
    {
        var r = await next();
        return r + 1; // tweak so we can observe it
    }
}

public class ClosedBehaviorTests
{
    [Fact]
    public async Task Closed_behavior_is_picked_up_via_runtime_enumeration()
    {
        var sp = TestServices.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new ComputeQuery(5));
        // handler returns 10, closed behavior adds 1
        Assert.Equal(11, result);
    }

    [Fact]
    public async Task FuseOnly_suppresses_runtime_enumeration()
    {
        var sp = TestServices.Build(cfg => cfg.FuseOnly = true);
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new ComputeQuery(5));
        // handler returns 10, closed behavior NOT run, open behaviors pass-through
        Assert.Equal(10, result);
    }
}
