# MDator Roadmap

## Completed

### Cross-assembly compile-time dispatch

**Status**: Implemented

Each assembly's generator now emits `[assembly: KnownRequest(typeof(...))]`
for every handler it discovers. Consuming assemblies read these attributes from
their references and include those types in the compile-time `switch` —
eliminating the `RuntimeDispatch` reflection fallback for known cross-assembly
request types.

**Before**: Cross-assembly requests hit `MakeGenericMethod` + `Invoke` on every
call (uncached).

**After**: Cross-assembly requests get the same strongly-typed `SendCore_*`
pipeline as same-assembly ones — open behaviors fused at compile time,
pre/post processors resolved from DI with known type arguments, zero
reflection.

The `RuntimeDispatch` fallback remains for truly dynamic/plugin-loaded request
types that no assembly advertises at compile time.

### Runtime-dispatch delegate caching

**Status**: Implemented

The remaining `RuntimeDispatch` fallback path now caches a compiled,
strongly-typed delegate per `(TRequest, TResponse)` pair (or per `TRequest`
for void / object overloads). The first call builds an Expression-tree wrapper
that closes the open generic and converts the request to its runtime type;
subsequent calls hit a `ConcurrentDictionary` lookup and invoke the delegate
directly — no `MakeGenericMethod`, no `MethodInfo.Invoke`, no `object[]` arg
array allocation.

Applies to `SendFallback`, `SendVoidFallback`, `SendObjectFallback`,
`StreamFallback`, and `StreamObjectFallback`.

### Notification fallback for unknown types

**Status**: Implemented

`Publish<T>` and `Publish(object)` previously returned `Task.CompletedTask`
when the notification type was not in the compile-time switch — silently
dropping the message. The generator default arm now calls
`RuntimeDispatch.PublishFallback`, which resolves
`INotificationHandler<T>` instances from DI for the runtime notification type
and dispatches them through the active `INotificationPublisher`. The fallback
shares the same compiled-delegate cache pattern used by the request/stream
fallbacks.

### Analyzer for MediatR-compat shim methods

**Status**: Implemented

`MDatorConfiguration.RegisterServicesFromAssembly`,
`RegisterServicesFromAssemblies`, and
`RegisterServicesFromAssemblyContaining<T>` exist as no-op shims so MediatR
config code keeps compiling during migration. The new analyzer `MDATOR0001`
flags these calls at `Info` severity, telling migrators that the call has no
effect and can be removed — handler discovery is driven by the source
generator scanning the consuming compilation directly.

## Known Limitations

### Exception handlers not fused for cross-assembly requests

Same-assembly requests get compile-time exception handler/action blocks because
the generator sees the `IRequestExceptionHandler<TReq, TResp, TEx>` class
declarations. Cross-assembly pipelines cannot know which exception types are
handled, so they omit exception blocks entirely. Exception handling for
cross-assembly requests must be done inside the handler itself or via a
pipeline behavior.

**Possible fix**: Extend `KnownRequestAttribute` or add a companion
`KnownExceptionHandler` attribute so the originating assembly can advertise
its exception handler registrations.

### Open generic handlers not included in compile-time switch

Handlers like `GenericHandler<T> : IRequestHandler<GenericRequest<T>, T>` are
discovered by the generator but excluded from the switch because `typeof()`
cannot represent open generic types. These always use the DI runtime path.

## Potential Future Work

- **Analyzer diagnostics**: Warn when a request type has no handler registered
  in the same assembly or any referenced assembly.
- **`FuseOnly` enforcement**: Emit a compiler error (not just silent fallback)
  when `FuseOnly = true` and a cross-assembly request would hit
  `RuntimeDispatch`.
- **Pre/post processor propagation**: Extend the attribute mechanism to
  advertise pre/post processors so cross-assembly pipelines can conditionally
  skip the `GetServices` call when none exist.
