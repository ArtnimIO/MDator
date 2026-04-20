namespace MDator;

/// <summary>
/// Invokes all handlers concurrently and awaits them with <see cref="Task.WhenAll(Task[])"/>.
/// Exceptions from all handlers are aggregated.
/// </summary>
public sealed class TaskWhenAllPublisher : INotificationPublisher
{
    /// <inheritdoc />
    public Task Publish(
        IReadOnlyList<NotificationHandlerExecutor> handlerExecutors,
        INotification notification,
        CancellationToken cancellationToken)
    {
        var tasks = new Task[handlerExecutors.Count];
        for (var i = 0; i < handlerExecutors.Count; i++)
        {
            tasks[i] = handlerExecutors[i].HandlerCallback(notification, cancellationToken);
        }
        return Task.WhenAll(tasks);
    }
}
