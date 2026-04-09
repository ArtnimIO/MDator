using MDator;
using Microsoft.Extensions.DependencyInjection;

// Declare open-generic behaviors at the assembly level so the MDator generator
// fuses them into every request's pipeline at compile time.
[assembly: MDator.OpenBehavior(typeof(MDator.Tests.LoggingBehavior<,>), Order = 0)]
[assembly: MDator.OpenBehavior(typeof(MDator.Tests.TimingBehavior<,>), Order = 1)]

namespace MDator.Tests;

public record EchoQuery(string Message) : IRequest<string>;

public sealed class EchoHandler : IRequestHandler<EchoQuery, string>
{
    private readonly Log _log;
    public EchoHandler(Log log) => _log = log;
    public Task<string> Handle(EchoQuery request, CancellationToken ct)
    {
        _log.Entries.Add("handler");
        return Task.FromResult(request.Message);
    }
}

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Log _log;
    public LoggingBehavior(Log log) => _log = log;
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        _log.Entries.Add("log:before");
        var r = await next();
        _log.Entries.Add("log:after");
        return r;
    }
}

public sealed class TimingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Log _log;
    public TimingBehavior(Log log) => _log = log;
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        _log.Entries.Add("time:before");
        var r = await next();
        _log.Entries.Add("time:after");
        return r;
    }
}

public class PipelineBehaviorTests
{
    [Fact]
    public async Task Open_generic_behaviors_compose_around_handler_in_declared_order()
    {
        var sp = TestServices.Build();
        var log = sp.GetRequiredService<Log>();
        var mediator = sp.GetRequiredService<IMediator>();
        var r = await mediator.Send(new EchoQuery("hi"));

        Assert.Equal("hi", r);
        // Order = 0 outer, Order = 1 inner; emitted reversed so Order-0 is outermost.
        Assert.Equal(
            new[] { "log:before", "time:before", "handler", "time:after", "log:after" },
            log.Entries);
    }
}
