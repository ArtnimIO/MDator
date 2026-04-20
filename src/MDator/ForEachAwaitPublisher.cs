namespace MDator;

/// <summary>
/// Invokes handlers sequentially, awaiting each one before starting the next.
/// The first exception propagates immediately and remaining handlers are not run.
/// Matches MediatR's default behaviour.
/// </summary>
public sealed class ForEachAwaitPublisher : INotificationPublisher
{
    /// <inheritdoc />
    public async Task Publish(
        IReadOnlyList<NotificationHandlerExecutor> handlerExecutors,
        INotification notification,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < handlerExecutors.Count; i++)
        {
            await handlerExecutors[i].HandlerCallback(notification, cancellationToken).ConfigureAwait(false);
        }
    }
}
