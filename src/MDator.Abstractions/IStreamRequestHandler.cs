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
    IAsyncEnumerable<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
