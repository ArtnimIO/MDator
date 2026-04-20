using System.Threading;
using System.Threading.Tasks;

namespace MDator;

/// <summary>
/// Runs after the request handler has produced a response, only if the handler
/// completed successfully.
/// </summary>
/// <typeparam name="TRequest">The type of the request being processed.</typeparam>
/// <typeparam name="TResponse">The type of the response being processed.</typeparam>
public interface IRequestPostProcessor<in TRequest, in TResponse>
{
    /// <summary>
    /// Performs a post-processing step on a request and its corresponding response.
    /// </summary>
    /// <param name="request">The request object passed to the post-processor.</param>
    /// <param name="response">The response object generated from the request.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation of the post-processor.</returns>
    Task Process(TRequest request, TResponse response, CancellationToken cancellationToken);
}
