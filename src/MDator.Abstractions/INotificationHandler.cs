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
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}
