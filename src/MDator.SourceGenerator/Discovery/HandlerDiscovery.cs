using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MDator.SourceGenerator;

internal static class HandlerDiscovery
{
    /// <summary>
    /// Examines a class symbol and yields a <see cref="HandlerInfo"/> for every
    /// MDator handler/processor/exception interface it implements. A single class
    /// can legitimately implement several (e.g. pre + post processor).
    /// </summary>
    public static IEnumerable<HandlerInfo> Classify(INamedTypeSymbol type)
    {
        if (type.IsAbstract || type.TypeKind != TypeKind.Class) yield break;

        var handlerIsOpen = type.IsGenericType && type.TypeParameters.Length > 0;
        var handlerRef = type.ToTypeRef();

        foreach (var iface in type.AllInterfaces)
        {
            if (iface.IsMDatorInterface("IRequestHandler`2"))
            {
                yield return new HandlerInfo(
                    HandlerKind.RequestWithResponse,
                    handlerRef,
                    iface.TypeArguments[0].ToTypeRef(),
                    iface.TypeArguments[1].ToTypeRef(),
                    null,
                    handlerIsOpen);
            }
            else if (iface.IsMDatorInterface("IRequestHandler`1"))
            {
                yield return new HandlerInfo(
                    HandlerKind.RequestVoid,
                    handlerRef,
                    iface.TypeArguments[0].ToTypeRef(),
                    null,
                    null,
                    handlerIsOpen);
            }
            else if (iface.IsMDatorInterface("INotificationHandler`1"))
            {
                yield return new HandlerInfo(
                    HandlerKind.Notification,
                    handlerRef,
                    iface.TypeArguments[0].ToTypeRef(),
                    null,
                    null,
                    handlerIsOpen);
            }
            else if (iface.IsMDatorInterface("IStreamRequestHandler`2"))
            {
                yield return new HandlerInfo(
                    HandlerKind.Stream,
                    handlerRef,
                    iface.TypeArguments[0].ToTypeRef(),
                    iface.TypeArguments[1].ToTypeRef(),
                    null,
                    handlerIsOpen);
            }
            else if (iface.IsMDatorInterface("IRequestPreProcessor`1"))
            {
                yield return new HandlerInfo(
                    HandlerKind.PreProcessor,
                    handlerRef,
                    iface.TypeArguments[0].ToTypeRef(),
                    null,
                    null,
                    handlerIsOpen);
            }
            else if (iface.IsMDatorInterface("IRequestPostProcessor`2"))
            {
                yield return new HandlerInfo(
                    HandlerKind.PostProcessor,
                    handlerRef,
                    iface.TypeArguments[0].ToTypeRef(),
                    iface.TypeArguments[1].ToTypeRef(),
                    null,
                    handlerIsOpen);
            }
            else if (iface.IsMDatorInterface("IRequestExceptionHandler`3"))
            {
                yield return new HandlerInfo(
                    HandlerKind.ExceptionHandler,
                    handlerRef,
                    iface.TypeArguments[0].ToTypeRef(),
                    iface.TypeArguments[1].ToTypeRef(),
                    iface.TypeArguments[2].ToTypeRef(),
                    handlerIsOpen,
                    ExceptionDepth: InheritanceDepth(iface.TypeArguments[2]));
            }
            else if (iface.IsMDatorInterface("IRequestExceptionAction`2"))
            {
                yield return new HandlerInfo(
                    HandlerKind.ExceptionAction,
                    handlerRef,
                    iface.TypeArguments[0].ToTypeRef(),
                    null,
                    iface.TypeArguments[1].ToTypeRef(),
                    handlerIsOpen,
                    ExceptionDepth: InheritanceDepth(iface.TypeArguments[1]));
            }
        }
    }

    private static int InheritanceDepth(ITypeSymbol type)
    {
        var depth = 0;
        var t = type.BaseType;
        while (t is not null)
        {
            depth++;
            t = t.BaseType;
        }
        return depth;
    }
}
