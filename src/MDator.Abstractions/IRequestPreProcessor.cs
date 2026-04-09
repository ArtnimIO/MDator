using System.Threading;
using System.Threading.Tasks;

namespace MDator;

/// <summary>
/// Runs before the request handler. Executed inside the innermost pipeline scope,
/// so any pipeline behavior can observe its effects.
/// </summary>
public interface IRequestPreProcessor<in TRequest>
{
    Task Process(TRequest request, CancellationToken cancellationToken);
}
