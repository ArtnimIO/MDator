namespace MDator.SourceGenerator;

/// <summary>
/// A request/notification type discovered via <c>[assembly: KnownRequest(...)]</c>
/// on a referenced assembly. We know the message type and its response type but
/// not the concrete handler, processor or exception handler types — those live in
/// the originating assembly and are already registered via its module initializer.
/// </summary>
internal sealed record CrossAssemblyRequestInfo(
    HandlerKind Kind,
    TypeRef MessageType,
    TypeRef? ResponseType,
    int MessageTypeDepth = 0);
