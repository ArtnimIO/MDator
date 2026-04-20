using System.Threading;
using System.Threading.Tasks;

namespace MDator;

/// <summary>
/// Runs before the request handler. Executed inside the innermost pipeline scope,
/// so any pipeline behavior can observe its effects.
/// </summary>
public interface IRequestPreProcessor<in TRequest>
{
    /// <summary>
    /// Processes the given request and handles any pre-processing logic before the main request handling occurs.
    /// </summary>
    /// <param name="request">The request object to be processed.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Process(TRequest request, CancellationToken cancellationToken);
}
