namespace MDator.SourceGenerator;

internal enum BehaviorKind
{
    Request,
    Stream,
}

/// <summary>
/// A pipeline behavior discovered in the consuming compilation or declared via
/// <c>[assembly: OpenBehavior(...)]</c>.
/// </summary>
internal sealed record BehaviorInfo(
    BehaviorKind Kind,
    TypeRef BehaviorType,
    TypeRef? ClosedRequestType,
    TypeRef? ClosedResponseType,
    bool IsOpenGeneric,
    int Order);
