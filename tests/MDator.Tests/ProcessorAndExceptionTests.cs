using MDator;
using Microsoft.Extensions.DependencyInjection;

namespace MDator.Tests;

public record SaveItemCommand(string Name) : IRequest<string>;

public sealed class SaveItemHandler : IRequestHandler<SaveItemCommand, string>
{
    private readonly Log _log;
    public SaveItemHandler(Log log) => _log = log;
    public Task<string> Handle(SaveItemCommand request, CancellationToken ct)
    {
        _log.Entries.Add("save-handler");
        if (request.Name == "boom")
            throw new InvalidOperationException("nope");
        return Task.FromResult($"saved:{request.Name}");
    }
}

public sealed class SaveItemPre : IRequestPreProcessor<SaveItemCommand>
{
    private readonly Log _log;
    public SaveItemPre(Log log) => _log = log;
    public Task Process(SaveItemCommand request, CancellationToken ct)
    {
        _log.Entries.Add("pre");
        return Task.CompletedTask;
    }
}

public sealed class SaveItemPost : IRequestPostProcessor<SaveItemCommand, string>
{
    private readonly Log _log;
    public SaveItemPost(Log log) => _log = log;
    public Task Process(SaveItemCommand request, string response, CancellationToken ct)
    {
        _log.Entries.Add($"post:{response}");
        return Task.CompletedTask;
    }
}

public sealed class SaveItemExceptionHandler : IRequestExceptionHandler<SaveItemCommand, string, InvalidOperationException>
{
    private readonly Log _log;
    public SaveItemExceptionHandler(Log log) => _log = log;
    public Task Handle(SaveItemCommand request, InvalidOperationException exception, RequestExceptionHandlerState<string> state, CancellationToken ct)
    {
        _log.Entries.Add($"ex-handler:{exception.Message}");
        state.SetHandled("recovered");
        return Task.CompletedTask;
    }
}

public sealed class SaveItemExceptionAction : IRequestExceptionAction<SaveItemCommand, Exception>
{
    private readonly Log _log;
    public SaveItemExceptionAction(Log log) => _log = log;
    public Task Execute(SaveItemCommand request, Exception exception, CancellationToken ct)
    {
        _log.Entries.Add($"ex-action:{exception.GetType().Name}");
        return Task.CompletedTask;
    }
}

public class ProcessorAndExceptionTests
{
    [Fact]
    public async Task Happy_path_runs_pre_handler_post()
    {
        var sp = TestServices.Build();
        var log = sp.GetRequiredService<Log>();
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SaveItemCommand("widget"));

        Assert.Equal("saved:widget", result);
        Assert.Contains("pre", log.Entries);
        Assert.Contains("save-handler", log.Entries);
        Assert.Contains("post:saved:widget", log.Entries);
    }

    [Fact]
    public async Task Exception_handler_converts_throw_to_response()
    {
        var sp = TestServices.Build();
        var log = sp.GetRequiredService<Log>();
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SaveItemCommand("boom"));

        Assert.Equal("recovered", result);
        Assert.Contains("ex-handler:nope", log.Entries);
        Assert.Contains("ex-action:InvalidOperationException", log.Entries);
    }
}
