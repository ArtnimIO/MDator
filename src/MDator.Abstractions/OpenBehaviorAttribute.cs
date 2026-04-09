using System;

namespace MDator;

/// <summary>
/// Declares an open-generic pipeline behavior to be fused into every request
/// pipeline by the MDator source generator.
/// </summary>
/// <remarks>
/// <para>
/// This is the compile-time equivalent of MediatR's
/// <c>cfg.AddOpenBehavior(typeof(LoggingBehavior&lt;,&gt;))</c>. Because the MDator
/// source generator runs before runtime DI registration, open behaviors have to
/// be declared in a place the generator can see. An assembly-level attribute is
/// the cleanest such place.
/// </para>
/// <para>
/// Example:
/// <code>
/// [assembly: MDator.OpenBehavior(typeof(LoggingBehavior&lt;,&gt;), Order = 0)]
/// [assembly: MDator.OpenBehavior(typeof(ValidationBehavior&lt;,&gt;), Order = 1)]
/// </code>
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class OpenBehaviorAttribute : Attribute
{
    /// <summary>
    /// The open-generic behavior type, e.g. <c>typeof(LoggingBehavior&lt;,&gt;)</c>.
    /// Must be an open generic implementing <see cref="IPipelineBehavior{TRequest, TResponse}"/>
    /// or <see cref="IStreamPipelineBehavior{TRequest, TResponse}"/>.
    /// </summary>
    public Type BehaviorType { get; }

    /// <summary>
    /// Order in the pipeline. Lower values run outermost (closer to the caller).
    /// Defaults to 0.
    /// </summary>
    public int Order { get; set; }

    public OpenBehaviorAttribute(Type behaviorType)
    {
        BehaviorType = behaviorType;
    }
}
