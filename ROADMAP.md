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

## Known Limitations

### Notification fallback silently drops unknown types

When `Publish<T>` is called with a notification type not in the compile-time
switch (and not advertised via `[assembly: KnownRequest]`), the generated code
returns `Task.CompletedTask` — silently dropping the notification. There is no
`RuntimeDispatch.PublishFallback` equivalent.

**Possible fix**: Add a `PublishFallback` that resolves
`INotificationHandler<T>` from DI at runtime, mirroring the request fallback
pattern.

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

### `RegisterServicesFromAssembly` is a no-op

`MDatorConfiguration.RegisterServicesFromAssembly()` and
`RegisterServicesFromAssemblyContaining<T>()` exist for MediatR source
compatibility but do nothing. Registration is driven entirely by the
source generator's `[ModuleInitializer]`. This can confuse migrators who
expect assembly-scanning behavior.

## Potential Future Work

- **Analyzer diagnostics**: Warn when a request type has no handler registered
  in the same assembly or any referenced assembly.
- **`FuseOnly` enforcement**: Emit a compiler error (not just silent fallback)
  when `FuseOnly = true` and a cross-assembly request would hit
  `RuntimeDispatch`.
- **Pre/post processor propagation**: Extend the attribute mechanism to
  advertise pre/post processors so cross-assembly pipelines can conditionally
  skip the `GetServices` call when none exist.
