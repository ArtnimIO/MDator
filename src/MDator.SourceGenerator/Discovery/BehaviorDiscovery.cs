using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MDator.SourceGenerator;

internal static class BehaviorDiscovery
{
    /// <summary>
    /// Classifies a class symbol as a closed <see cref="IPipelineBehavior{TRequest, TResponse}"/>
    /// or <see cref="IStreamPipelineBehavior{TRequest, TResponse}"/> implementation.
    /// Open behaviors are discovered separately via
    /// <c>[assembly: OpenBehavior]</c> — a class that happens to be an open
    /// generic IPipelineBehavior but isn't declared via attribute is still
    /// registered for DI but not fused into pipelines.
    /// </summary>
    public static IEnumerable<BehaviorInfo> ClassifyClosed(INamedTypeSymbol type)
    {
        if (type.IsAbstract || type.TypeKind != TypeKind.Class) yield break;

        // Open-generic behaviors opt in via [assembly: MDator.OpenBehavior(...)]. A
        // class that just happens to be an open-generic IPipelineBehavior is
        // typically the OPEN DEFINITION of a behavior the user ALSO declares via
        // attribute — yielding it again here would cause double registration and
        // double fusion. Skip open-generic classes from the class-scan path.
        if (type.IsGenericType && type.TypeParameters.Length > 0) yield break;

        var behaviorRef = type.ToTypeRef();

        foreach (var iface in type.AllInterfaces)
        {
            if (iface.IsMDatorInterface("IPipelineBehavior`2"))
            {
                yield return new BehaviorInfo(
                    BehaviorKind.Request,
                    behaviorRef,
                    iface.TypeArguments[0].ToTypeRef(),
                    iface.TypeArguments[1].ToTypeRef(),
                    IsOpenGeneric: false,
                    Order: int.MaxValue);
            }
            else if (iface.IsMDatorInterface("IStreamPipelineBehavior`2"))
            {
                yield return new BehaviorInfo(
                    BehaviorKind.Stream,
                    behaviorRef,
                    iface.TypeArguments[0].ToTypeRef(),
                    iface.TypeArguments[1].ToTypeRef(),
                    IsOpenGeneric: false,
                    Order: int.MaxValue);
            }
        }
    }
}
