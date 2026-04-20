using System.Collections.Generic;
using System.Threading;

namespace MDator;

/// <summary>
/// Handles a streaming request, producing an <see cref="IAsyncEnumerable{T}"/>
/// of responses.
/// </summary>
public interface IStreamRequestHandler<in TRequest, out TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    /// <summary>
    /// Processes the given request and streams the responses asynchronously.
    /// </summary>
    /// <param name="request">The request object containing the necessary data for processing.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of response objects.</returns>
    IAsyncEnumerable<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
