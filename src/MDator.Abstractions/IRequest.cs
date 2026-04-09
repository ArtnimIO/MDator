namespace MDator;

/// <summary>
/// Marker interface used by <see cref="IRequest"/> and <see cref="IRequest{TResponse}"/>
/// so both can be accepted by a single non-generic parameter slot.
/// </summary>
public interface IBaseRequest { }

/// <summary>
/// Marks a request that does not return a value. Handled by an
/// <see cref="IRequestHandler{TRequest}"/>.
/// </summary>
public interface IRequest : IBaseRequest { }

/// <summary>
/// Marks a request that returns a <typeparamref name="TResponse"/>. Handled by an
/// <see cref="IRequestHandler{TRequest, TResponse}"/>.
/// </summary>
public interface IRequest<out TResponse> : IBaseRequest { }
