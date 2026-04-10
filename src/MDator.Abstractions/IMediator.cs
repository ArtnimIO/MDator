namespace MDator;

/// <summary>
/// The union of <see cref="ISender"/> and <see cref="IPublisher"/>. Users typically
/// depend on <c>IMediator</c> when they want both request/response and pub/sub, or
/// on one of the narrower interfaces when they want to restrict their code to a
/// single responsibility.
/// </summary>
public interface IMediator : ISender, IPublisher;
