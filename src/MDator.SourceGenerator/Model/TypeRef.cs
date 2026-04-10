namespace MDator.SourceGenerator;

/// <summary>
/// A value-equatable reference to a type. We never hold onto an
/// <c>INamedTypeSymbol</c> past the transform step — that would hold the
/// compilation in memory and break incremental generator caching — so instead we
/// flatten everything we need about a type into strings and primitives.
/// </summary>
/// <param name="GlobalName">Fully qualified closed form, e.g. <c>global::My.App.GetUserQuery</c>.</param>
/// <param name="SimpleName">Short name with no qualification, used to derive identifiers.</param>
/// <param name="IsOpenGeneric">True when the type is a generic definition (e.g. the
/// <c>LoggingBehavior&lt;TRequest, TResponse&gt;</c> definition itself, not a closed construction).</param>
/// <param name="TypeArity">Number of type parameters on the original definition —
/// used to build the unbound <c>typeof(X&lt;,&gt;)</c> form for open generic DI
/// registration.</param>
internal sealed record TypeRef(
    string GlobalName,
    string SimpleName,
    bool IsOpenGeneric,
    int TypeArity = 0) : IEquatable<TypeRef>
{
    public string Identifier
    {
        get
        {
            var chars = SimpleName.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                var c = chars[i];
                if (!char.IsLetterOrDigit(c) && c != '_') chars[i] = '_';
            }
            return new string(chars);
        }
    }

    /// <summary>
    /// The fully-qualified name with any <c>&lt;...&gt;</c> suffix stripped, e.g.
    /// <c>global::My.App.LoggingBehavior</c>. Used as a starting point when the
    /// emitter needs to splice in its own type arguments.
    /// </summary>
    public string GlobalNameWithoutGenerics
    {
        get
        {
            var idx = GlobalName.IndexOf('<');
            return idx < 0 ? GlobalName : GlobalName.Substring(0, idx);
        }
    }

    /// <summary>
    /// The unbound generic form for use in <c>typeof</c> expressions:
    /// <c>global::My.App.LoggingBehavior&lt;,&gt;</c>. Returns
    /// <see cref="GlobalName"/> when the type is non-generic.
    /// </summary>
    public string GlobalNameUnbound
    {
        get
        {
            if (TypeArity == 0) return GlobalName;
            var commas = TypeArity == 1 ? string.Empty : new string(',', TypeArity - 1);
            return GlobalNameWithoutGenerics + "<" + commas + ">";
        }
    }
}
