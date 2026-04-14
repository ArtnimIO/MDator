using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MDator.SourceGenerator;

internal static class SymbolExtensions
{
    /// <summary>
    /// Like <see cref="SymbolDisplayFormat.FullyQualifiedFormat"/> but preserves
    /// the <c>?</c> suffix on nullable reference types so the generated code
    /// matches the original nullability annotations (avoids CS8631).
    /// </summary>
    private static readonly SymbolDisplayFormat NullableFullyQualifiedFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public static TypeRef ToTypeRef(this ITypeSymbol symbol)
    {
        var globalName = symbol.ToDisplayString(NullableFullyQualifiedFormat);
        var simpleName = symbol.ToDisplayString(new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));

        var isOpen = false;
        var arity = 0;
        if (symbol is not INamedTypeSymbol nts) return new TypeRef(globalName, simpleName, isOpen, arity);
        arity = nts.OriginalDefinition.Arity;
        if (nts.IsUnboundGenericType || Enumerable.Any(nts.TypeArguments, ta => ta.TypeKind == TypeKind.TypeParameter)) isOpen = true;
        return new TypeRef(globalName, simpleName, isOpen, arity);
    }

    /// <summary>
    /// Walks up the inheritance chain and yields every interface the type implements
    /// (including inherited). Used to classify a handler class.
    /// </summary>
    public static IEnumerable<INamedTypeSymbol> AllInterfaces(this INamedTypeSymbol symbol) => symbol.AllInterfaces;

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
