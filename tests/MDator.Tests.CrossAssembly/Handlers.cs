using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MDator.Tests.CrossAssembly;

// ── Request with response ───────────────────────────────────────────

public record CrossItem(int Id, string Name);

public record CrossGetItemQuery(int Id) : IRequest<CrossItem>;

public sealed class CrossGetItemHandler : IRequestHandler<CrossGetItemQuery, CrossItem>
{
    public Task<CrossItem> Handle(CrossGetItemQuery request, CancellationToken cancellationToken)
        => Task.FromResult(new CrossItem(request.Id, $"Item-{request.Id}"));
}

// ── Void request ────────────────────────────────────────────────────

public record CrossDeleteCommand(int Id) : IRequest;

public sealed class CrossDeleteHandler : IRequestHandler<CrossDeleteCommand>
{
    public static int DeleteCount;

    public Task Handle(CrossDeleteCommand request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref DeleteCount);
        return Task.CompletedTask;
    }
}

// ── Streaming ───────────────────────────────────────────────────────

public record CrossStreamItems(int Count) : IStreamRequest<CrossItem>;

public sealed class CrossStreamHandler : IStreamRequestHandler<CrossStreamItems, CrossItem>
{
    public async IAsyncEnumerable<CrossItem> Handle(
        CrossStreamItems request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var i = 0; i < request.Count; i++)
        {
            yield return new CrossItem(i, $"Stream-{i}");
            await Task.Yield();
        }
    }
}

// ── Notification ────────────────────────────────────────────────────

public record CrossItemCreated(int Id) : INotification;

public sealed class CrossItemCreatedHandler : INotificationHandler<CrossItemCreated>
{
    public static int NotifyCount;

    public Task Handle(CrossItemCreated notification, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref NotifyCount);
        return Task.CompletedTask;
    }
}
