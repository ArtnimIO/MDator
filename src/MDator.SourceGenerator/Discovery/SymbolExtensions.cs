using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MDator.SourceGenerator;

internal static class SymbolExtensions
{
    public static TypeRef ToTypeRef(this ITypeSymbol symbol)
    {
        var globalName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var simpleName = symbol.ToDisplayString(new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));

        var isOpen = false;
        var arity = 0;
        if (symbol is INamedTypeSymbol nts)
        {
            arity = nts.OriginalDefinition.Arity;
            if (nts.IsUnboundGenericType) isOpen = true;
            else
            {
                foreach (var ta in nts.TypeArguments)
                {
                    if (ta.TypeKind == TypeKind.TypeParameter) { isOpen = true; break; }
                }
            }
        }
        return new TypeRef(globalName, simpleName, isOpen, arity);
    }

    /// <summary>
    /// Walks up the inheritance chain and yields every interface the type implements
    /// (including inherited). Used to classify a handler class.
    /// </summary>
    public static IEnumerable<INamedTypeSymbol> AllInterfaces(this INamedTypeSymbol symbol)
    {
        foreach (var i in symbol.AllInterfaces) yield return i;
    }

    /// <summary>
    /// Does <paramref name="candidate"/> match <paramref name="metadataName"/>
    /// (e.g. "MDator.IRequestHandler`2") as its original-definition metadata name?
    /// </summary>
    public static bool IsMDatorInterface(this INamedTypeSymbol candidate, string metadataName)
    {
        var original = candidate.OriginalDefinition;
        var ns = original.ContainingNamespace;
        if (ns is null || ns.IsGlobalNamespace || ns.Name != "MDator") return false;
        return original.MetadataName == metadataName;
    }
}
