namespace MDator.SourceGenerator;

/// <summary>
/// Everything the emitter needs about the consuming compilation, flattened into
/// value-equatable records.
/// </summary>
internal sealed record PipelineModel(
    string RootNamespace,
    EquatableArray<HandlerInfo> Handlers,
    EquatableArray<BehaviorInfo> Behaviors,
    EquatableArray<CrossAssemblyRequestInfo> CrossAssemblyRequests);
