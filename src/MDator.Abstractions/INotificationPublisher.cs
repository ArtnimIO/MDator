using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MDator;

/// <summary>
/// A single pending invocation of a notification handler: the already-closed-over
/// handler plus the notification instance. The publisher strategy decides how to
/// schedule these.
/// </summary>
public readonly struct NotificationHandlerExecutor
{
    public NotificationHandlerExecutor(object handlerInstance, System.Func<INotification, CancellationToken, Task> callback)
    {
        HandlerInstance = handlerInstance;
        HandlerCallback = callback;
    }

    /// <summary>
    /// The handler instance. Exposed so publishers can do identity-based dedupe
    /// when the same handler is subscribed to multiple base notification types.
    /// </summary>
    public object HandlerInstance { get; }

    /// <summary>
    /// Invokes the handler with the notification.
    /// </summary>
    public System.Func<INotification, CancellationToken, Task> HandlerCallback { get; }
}

/// <summary>
/// Strategy for dispatching a notification to its handlers. Swap between
/// sequential, parallel-wait, and parallel-no-wait semantics without touching
/// handler code.
/// </summary>
public interface INotificationPublisher
{
    Task Publish(
        IReadOnlyList<NotificationHandlerExecutor> handlerExecutors,
        INotification notification,
        CancellationToken cancellationToken);
}
