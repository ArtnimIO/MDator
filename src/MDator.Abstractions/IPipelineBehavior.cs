using System.Threading;
using System.Threading.Tasks;

namespace MDator;

/// <summary>
/// A delegate representing the next step in a request pipeline. Calling it advances
/// to the next behavior (or finally the request handler).
/// </summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Wraps a request handler, allowing cross-cutting concerns (logging, validation,
/// caching, transactions, etc.) to be composed around it.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : notnull
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
