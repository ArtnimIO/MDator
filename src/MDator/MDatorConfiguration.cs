using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace MDator;

/// <summary>
/// Runtime configuration passed to <c>AddMDator(cfg =&gt; ...)</c>. The source generator
/// is authoritative for handler and open-behavior discovery; this object exists for
/// the parts that genuinely have to be chosen at runtime: lifetime defaults,
/// notification publisher strategy, and dynamically-added closed behaviors.
/// </summary>
public sealed class MDatorConfiguration
{
    /// <summary>
    /// Default lifetime for handlers, pre/post processors and closed behaviors
    /// registered by the generator. <see cref="ServiceLifetime.Transient"/> matches
    /// MediatR's default.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

    /// <summary>
    /// When <c>true</c>, generated pipelines only execute behaviors declared via
    /// <see cref="OpenBehaviorAttribute"/> (and closed behaviors referenced by type
    /// in the consuming assembly). The per-request
    /// <c>sp.GetServices&lt;IPipelineBehavior&lt;,&gt;&gt;()</c> enumeration fallback
    /// is skipped, which eliminates its allocation and gives fully compile-time
    /// fused pipelines. Runtime-added behaviors will be ignored.
    /// </summary>
    public bool FuseOnly { get; set; }

    /// <summary>
    /// Publisher strategy used by <c>IPublisher.Publish</c>. Defaults to
    /// <see cref="ForEachAwaitPublisher"/> which matches MediatR's default.
    /// </summary>
    public INotificationPublisher NotificationPublisher { get; set; } = new ForEachAwaitPublisher();

    /// <summary>
    /// Additional closed pipeline behaviors to register at runtime. The generator
    /// already picks up closed behaviors implementing
    /// <see cref="IPipelineBehavior{TRequest, TResponse}"/> directly; use this only
    /// for behaviors that live in an assembly the generator didn't scan, or that you
    /// want to enable conditionally.
    /// </summary>
    public List<(Type ServiceType, Type ImplementationType, ServiceLifetime Lifetime)> AdditionalBehaviors { get; } = new();

    /// <summary>
    /// MediatR source-compatibility shim. Has no effect — MDator's source generator
    /// scans the consuming compilation directly, so handler discovery is automatic.
    /// The analyzer <c>MDATOR0001</c> flags calls to this method.
    /// </summary>
    public MDatorConfiguration RegisterServicesFromAssemblyContaining<T>() => this;

    /// <summary>
    /// MediatR source-compatibility shim. Has no effect — MDator's source generator
    /// scans the consuming compilation directly, so handler discovery is automatic.
    /// The analyzer <c>MDATOR0001</c> flags calls to this method.
    /// </summary>
    public MDatorConfiguration RegisterServicesFromAssembly(System.Reflection.Assembly assembly) => this;

    /// <summary>
    /// MediatR source-compatibility shim. Has no effect — MDator's source generator
    /// scans the consuming compilation directly, so handler discovery is automatic.
    /// The analyzer <c>MDATOR0001</c> flags calls to this method.
    /// </summary>
    public MDatorConfiguration RegisterServicesFromAssemblies(params System.Reflection.Assembly[] assemblies) => this;

    /// <summary>
    /// Source-compat marker for MediatR v12 migration.
    /// Open generic handlers are discovered automatically by the source generator.
    /// </summary>
    public bool RegisterGenericHandlers { get; set; }

    /// <summary>
    /// Open behavior types registered by the source generator's <c>[ModuleInitializer]</c>.
    /// Used by <see cref="RuntimeDispatch"/> to chain open behaviors in the DI fallback
    /// path (cross-assembly handler dispatch).
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public List<(Type Type, int Order)> OpenBehaviorTypes { get; } = new();

    /// <summary>
    /// Called from generated registration code to record an open behavior type
    /// so the runtime fallback path can resolve it.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void RegisterOpenBehavior(Type type, int order)
    {
        if (OpenBehaviorTypes.All(x => x.Type != type))
            OpenBehaviorTypes.Add((type, order));
    }

    /// <summary>
    /// Registers a closed behavior at runtime.
    /// </summary>
    public MDatorConfiguration AddBehavior<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TImplementation : class
    {
        AdditionalBehaviors.Add((typeof(TImplementation), typeof(TImplementation), lifetime));
        return this;
    }

    /// <summary>
    /// Registers a closed behavior at runtime against a specific service type.
    /// </summary>
    public MDatorConfiguration AddBehavior(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        AdditionalBehaviors.Add((serviceType, implementationType, lifetime));
        return this;
    }
}
