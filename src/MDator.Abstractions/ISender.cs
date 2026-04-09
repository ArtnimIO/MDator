using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MDator;

/// <summary>
/// Sends a request to a single handler. The request/response path.
/// </summary>
public interface ISender
{
    /// <summary>
    /// Send a request expecting a response of type <typeparamref name="TResponse"/>.
    /// </summary>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a request with no response.
    /// </summary>
    Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest;

    /// <summary>
    /// Send a request whose runtime type is not known statically. Slower than the
    /// generic overloads because it has to dispatch via a type switch; prefer the
    /// generic variants where possible.
    /// </summary>
    Task<object?> Send(object request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Begin a streaming request.
    /// </summary>
    IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Begin a streaming request whose runtime type is not known statically.
    /// </summary>
    IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default);
}
