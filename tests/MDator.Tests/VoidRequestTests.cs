using Microsoft.Extensions.DependencyInjection;

namespace MDator.Tests;

public sealed class Log
{
    public List<string> Entries { get; } = new();
}

public record PingCommand(string Message) : IRequest;

public sealed class PingHandler : IRequestHandler<PingCommand>
{
    private readonly Log _log;
    public PingHandler(Log log) => _log = log;

    public Task Handle(PingCommand request, CancellationToken cancellationToken)
    {
        _log.Entries.Add($"ping:{request.Message}");
        return Task.CompletedTask;
    }
}

public class VoidRequestTests
{
    [Fact]
    public async Task Send_void_invokes_handler()
    {
        var sp = TestServices.Build();
        var log = sp.GetRequiredService<Log>();
        var mediator = sp.GetRequiredService<IMediator>();
        await mediator.Send(new PingCommand("hello"));

        // Assembly-level open behaviors wrap the void pipeline too.
        Assert.Contains("ping:hello", log.Entries);
        Assert.Contains("log:before", log.Entries);
        Assert.Contains("log:after", log.Entries);
        Assert.Contains("time:before", log.Entries);
        Assert.Contains("time:after", log.Entries);
    }
}
