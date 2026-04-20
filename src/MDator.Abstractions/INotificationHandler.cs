using System.Threading;
using System.Threading.Tasks;

namespace MDator;

/// <summary>
/// Handles a single <typeparamref name="TNotification"/>. Multiple handlers may exist
/// for the same notification type; the active <see cref="INotificationPublisher"/>
/// decides how they are invoked.
/// </summary>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles a notification of type <typeparamref name="TNotification"/>.
    /// </summary>
    /// <param name="notification">
    /// The notification instance to process.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe for cancellation requests.
    /// </param>
    /// <return>
    /// A <see cref="Task"/> that completes when the notification handling is finished.
    /// </return>
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}
