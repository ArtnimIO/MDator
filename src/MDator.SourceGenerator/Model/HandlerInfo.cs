namespace MDator.SourceGenerator;

internal enum HandlerKind
{
    RequestWithResponse,
    RequestVoid,
    Notification,
    Stream,
    PreProcessor,
    PostProcessor,
    ExceptionHandler,
    ExceptionAction,
}

/// <summary>
/// A handler-like type discovered in the consuming compilation, plus enough
/// information for the emitter to generate dispatch and DI registration code.
/// <see cref="ExceptionDepth"/> is only meaningful when <see cref="Kind"/> is
/// <see cref="HandlerKind.ExceptionHandler"/> or <see cref="HandlerKind.ExceptionAction"/>;
/// it records how many inheritance steps the exception type is from
/// <c>System.Exception</c> so we can emit catch blocks in specificity order.
/// </summary>
internal sealed record HandlerInfo(
    HandlerKind Kind,
    TypeRef HandlerType,
    TypeRef MessageType,
    TypeRef? ResponseType,
    TypeRef? ExceptionType,
    bool HandlerIsOpenGeneric,
    int ExceptionDepth = 0);
