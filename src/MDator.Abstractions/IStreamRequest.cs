namespace MDator;

/// <summary>
/// Marks a request whose handler produces an asynchronous stream of
/// <typeparamref name="TResponse"/> values.
/// </summary>
public interface IStreamRequest<out TResponse>;
