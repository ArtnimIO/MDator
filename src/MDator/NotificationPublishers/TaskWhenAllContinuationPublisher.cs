using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MDator;

/// <summary>
/// Starts every handler (synchronously up to the first await), then returns a
/// task that waits for all of them. Handlers race until their first await point,
/// after which the continuation scheduler owns them. Matches MediatR's
/// <c>TaskWhenAllPublisher</c> non-wait mode.
/// </summary>
public sealed class TaskWhenAllContinuationPublisher : INotificationPublisher
{
    public Task Publish(
        IReadOnlyList<NotificationHandlerExecutor> handlerExecutors,
        INotification notification,
        CancellationToken cancellationToken)
    {
        var tasks = new Task[handlerExecutors.Count];
        for (var i = 0; i < handlerExecutors.Count; i++)
        {
            tasks[i] = Task.Run(() => handlerExecutors[i].HandlerCallback(notification, cancellationToken), cancellationToken);
        }
        return Task.WhenAll(tasks);
    }
}
