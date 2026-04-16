using System;

namespace MDator;

/// <summary>
/// Marks a request or notification type as known so that consuming assemblies can
/// include it in their compile-time dispatch switch. The generator emits this
/// attribute automatically for every handler it discovers, enabling cross-assembly
/// compile-time dispatch without manual configuration.
/// </summary>
/// <remarks>
/// You can also apply this attribute manually for requests whose closed generic
/// form is never syntactically referenced in the consuming assembly — for example,
/// open-generic request handlers that will only ever be invoked with a
/// runtime-chosen <c>T</c>. Without this attribute (and without a same-assembly
/// handler), such requests fall through to <c>RuntimeDispatch</c>.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class KnownRequestAttribute(Type requestType) : Attribute
{
    public Type RequestType { get; } = requestType;
}
