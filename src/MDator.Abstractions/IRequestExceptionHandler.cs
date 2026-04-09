using System;
using System.Threading;
using System.Threading.Tasks;

namespace MDator;

/// <summary>
/// Mutable state passed to an <see cref="IRequestExceptionHandler{TRequest, TResponse, TException}"/>.
/// Call <see cref="SetHandled(TResponse)"/> to swallow the exception and replace it
/// with a synthesized response.
/// </summary>
public sealed class RequestExceptionHandlerState<TResponse>
{
    public bool Handled { get; private set; }
    public TResponse? Response { get; private set; }

    public void SetHandled(TResponse response)
    {
        Handled = true;
        Response = response;
    }
}

/// <summary>
/// Catches an exception thrown by a handler or downstream behavior and may convert
/// it into a response. Handlers are ordered by exception type specificity at compile
/// time — most-derived first.
/// </summary>
public interface IRequestExceptionHandler<in TRequest, TResponse, in TException>
    where TRequest : notnull
    where TException : Exception
{
    Task Handle(
        TRequest request,
        TException exception,
        RequestExceptionHandlerState<TResponse> state,
        CancellationToken cancellationToken);
}

/// <summary>
/// Observes an exception without being able to convert it into a response.
/// Typically used for logging. Always runs even if an
/// <see cref="IRequestExceptionHandler{TRequest, TResponse, TException}"/> marks
/// the exception handled.
/// </summary>
public interface IRequestExceptionAction<in TRequest, in TException>
    where TRequest : notnull
    where TException : Exception
{
    Task Execute(TRequest request, TException exception, CancellationToken cancellationToken);
}
