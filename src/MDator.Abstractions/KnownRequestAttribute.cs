using System;

namespace MDator;

/// <summary>
/// Adds a specific closed request type to the compile-time dispatcher. Use this
/// for requests whose closed generic form is not syntactically referenced anywhere
/// in the consuming assembly — for example, open-generic request handlers that
/// will only ever be invoked with a runtime-chosen <c>T</c>.
/// </summary>
/// <remarks>
/// Without this attribute, such requests fall through to the strongly-typed DI
/// fallback path, which is fine but slower than the generated switch.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class KnownRequestAttribute(Type requestType) : Attribute
{
    public Type RequestType { get; } = requestType;
}
