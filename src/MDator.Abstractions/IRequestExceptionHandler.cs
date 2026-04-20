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
    /// <summary>
    /// Indicates whether the exception has been handled by the request exception handler.
    /// When set to <c>true</c>, the exception is considered handled, and a synthesized response
    /// will replace the original exception.
    /// </summary>
    public bool Handled { get; private set; }

    /// <summary>
    /// Represents the synthesized response to replace the original exception
    /// when the exception is handled. This property contains the value that is
    /// assigned through <see cref="SetHandled(TResponse)"/> during the handling
    /// of the exception.
    /// </summary>
    public TResponse? Response { get; private set; }

    /// <summary>
    /// Marks the exception as handled and sets a synthesized response to replace the original exception response.
    /// </summary>
    /// <param name="response">The synthesized response to replace the original exception response.</param>
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
    where TException : Exception
{
    /// <summary>
    /// Handles an exception thrown during the processing of a request and may convert it into a response.
    /// </summary>
    /// <param name="request">The request that was being processed when the exception occurred.</param>
    /// <param name="exception">The exception thrown during the processing of the request.</param>
    /// <param name="state">
    /// The mutable state used to determine whether the exception should be handled and to provide a response
    /// to replace the original exception outcome.
    /// </param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
    where TException : Exception
{
    /// <summary>
    /// Executes an action based on the provided request and exception. Primarily used for logging or other side effects
    /// without altering the exception handling flow.
    /// </summary>
    /// <param name="request">The original request that caused the exception.</param>
    /// <param name="exception">The exception that occurred during the execution of the request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Execute(TRequest request, TException exception, CancellationToken cancellationToken);
}
