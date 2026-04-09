using System.Collections.Generic;
using System.Threading;

namespace MDator;

/// <summary>
/// The next step in a stream request pipeline.
/// </summary>
public delegate IAsyncEnumerable<TResponse> StreamHandlerDelegate<TResponse>();

/// <summary>
/// Wraps a stream request handler for cross-cutting concerns.
/// </summary>
public interface IStreamPipelineBehavior<in TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    IAsyncEnumerable<TResponse> Handle(
        TRequest request,
        StreamHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
