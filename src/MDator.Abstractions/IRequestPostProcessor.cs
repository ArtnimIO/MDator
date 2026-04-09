using System.Threading;
using System.Threading.Tasks;

namespace MDator;

/// <summary>
/// Runs after the request handler has produced a response, only if the handler
/// completed successfully.
/// </summary>
public interface IRequestPostProcessor<in TRequest, in TResponse>
{
    Task Process(TRequest request, TResponse response, CancellationToken cancellationToken);
}
