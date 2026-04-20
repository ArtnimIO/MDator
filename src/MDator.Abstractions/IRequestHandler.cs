using System.Threading;
using System.Threading.Tasks;

namespace MDator;

/// <summary>
/// Handles a request with a response.
/// </summary>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the provided request asynchronously.
    /// </summary>
    /// <param name="request">The request instance containing the required data to process.</param>
    /// <param name="cancellationToken">The token used to manage task cancellation.</param>
    /// <returns>A task representing the asynchronous operation, containing the response of the specified type.</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Handles a request with no response.
/// </summary>
public interface IRequestHandler<in TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Processes the given request asynchronously using the specified cancellation token.
    /// </summary>
    /// <param name="request">The request data to be handled.</param>
    /// <param name="cancellationToken">The cancellation token to signal task cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle(TRequest request, CancellationToken cancellationToken);
}
