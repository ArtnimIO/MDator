using System.Threading;
using System.Threading.Tasks;

namespace MDator;

/// <summary>
/// Publishes notifications to zero or more handlers.
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Publish a notification whose runtime type is not known statically.
    /// </summary>
    Task Publish(object notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish a notification of known type <typeparamref name="TNotification"/>.
    /// </summary>
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}
