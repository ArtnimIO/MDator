# Roadmap to 1.0

Condensed from the maturity assessment of 2026-08-19. The code itself is in
good shape; the gaps are about the *guarantees* a 1.0 promises: pinned
generator output, a stable public API, MediatR source compatibility, and
package hygiene. Status reflects `origin/main` as of 2026-09-16.

Feature work (cross-assembly dispatch, analyzers, known limitations) is
tracked separately in [ROADMAP.md](ROADMAP.md).

## 1. Pin the generator and the package — Done

Merged in [#76](https://github.com/ArtnimIO/MDator/pull/76) (2026-08-19).

- Generator snapshot tests (`Verify.SourceGenerators`) covering
  request/response, void, stream, notifications, open and closed behaviors,
  pre/post processors, exception handler + action, a no-handlers baseline and
  the cross-assembly `[KnownRequest]` path. Each snapshot is also compiled and
  must produce zero errors.
- `Pack` now verifies the merged nupkg contains three distinct
  `analyzers/roslyn{4.8,4.12,5.0}` DLLs, each referencing the matching
  `Microsoft.CodeAnalysis.CSharp`. The 0.5.0–0.6.1 class of silent packaging
  breakage fails the build.

## 2. Close the MediatR v12 compat gaps — Done

Merged in [#77](https://github.com/ArtnimIO/MDator/pull/77) (2026-08-19).

The README promises migration is "a namespace find-replace". That now holds
for:

- `RequestHandlerDelegate<T>` optional `CancellationToken` parameter
  (MediatR 12.3+), threaded through generated pipelines and the runtime
  fallback.
- `INotificationPublisher.Publish` takes `IEnumerable` instead of
  `IReadOnlyList`.
- `ForeachAwaitPublisher` (MediatR spelling) as default; `ForEachAwaitPublisher`
  stays as an obsolete alias **to be removed at 1.0**.
- `where TRequest : notnull` on behavior interfaces; covariant
  `StreamHandlerDelegate`.
- `NotificationHandler<T>` base class; `AddOpenBehavior(s)`,
  `AddBehavior<TService,TImpl>`, `AddStreamBehavior`, `AddOpenStreamBehavior`,
  `NotificationPublisherType` on `MDatorConfiguration`.
- Bug fix: `TaskWhenAllContinuationPublisher` captured the loop index in its
  `Task.Run` lambdas.

Verified against MediatR 12.5.0 source: `IRequest : IRequest<Unit>` is
MediatR 11, not 12 (MDator already matches), and v12 ships no
exception-handler base classes. No change needed for either.

Deliberately deferred, documented in the "Known divergences" README section
that the PR adds:

- The `MediatR.Pipeline` / `MediatR.NotificationPublishers` namespace split.
  Moving types out of the flat `MDator` namespace needs its own decision
  before 1.0.
- `AddRequestPreProcessor` / `AddRequestPostProcessor`. Config-only
  registration would be silently dropped by fused pipelines; needs a designed
  solution, not a trap.
- MediatR knobs that are structurally N/A for a source-generated mediator.

## 3. Lock the public API — Done

Merged in [#84](https://github.com/ArtnimIO/MDator/pull/84) (2026-09-15).

1.0 means no breaking changes until 2.0. The build now enforces it:

- `Microsoft.CodeAnalysis.PublicApiAnalyzers` on `MDator` and
  `MDator.Abstractions`. `PublicAPI.Shipped.txt` is the 0.6.2 surface;
  `PublicAPI.Unshipped.txt` carries the #77 additions and `*REMOVED*` entries
  for the signatures #77 changed. An unlisted public change fails the build.
- `MDATOR0001` moved from `AnalyzerReleases.Unshipped.md` to `Shipped` under
  0.5.0.

At each release, fold Unshipped into Shipped. Removing `ForEachAwaitPublisher`
at 1.0 goes through `*REMOVED*` lines like any other break.

## 4. NuGet package hygiene — Done

Merged in [#85](https://github.com/ArtnimIO/MDator/pull/85) (2026-09-16).

- Symbol packages (`snupkg`), `PublishRepositoryUrl`,
  `ContinuousIntegrationBuild` under GitHub Actions. Source Link and
  `EmbedUntrackedSources` come from the .NET SDK, so no package reference.
  The Pack target carries the symbol package through the per-Roslyn merge;
  the release uploads it.
- `global.json` pins the .NET 10 SDK line (`latestFeature`, no prerelease).
- Trimming / AOT, decided: annotate and enforce. `MDator` is
  `IsAotCompatible`; `RuntimeDispatch` is `RequiresUnreferencedCode` +
  `RequiresDynamicCode`, so trimmed and AOT consumers get `IL2026` /
  `IL3050` at the generated fallback call sites. `MDatorConfiguration`
  `Type` parameters that reach `GetInterfaces()` carry
  `DynamicallyAccessedMembers`. README has a "Trimming and Native AOT"
  section. A generator switch that omits the fallback arms is a possible
  follow-up, not a 1.0 blocker.

## 5. Widen and harden CI — Done

Merged in [#86](https://github.com/ArtnimIO/MDator/pull/86) (2026-09-16).

- CI runs on Ubuntu and Windows.
- Tests run on every target framework of `MDator` (a single one, net10.0,
  since #89).
- `SampleCompile` runs in the PR workflow and builds the samples against the
  nupkg just packed (overridable `MDatorPackageVersion`, nuget.org kept as a
  restore source, private package folder).

## 6. Decide the TFM floor — Done

Merged in [#89](https://github.com/ArtnimIO/MDator/pull/89) (2026-09-16).

Decision: `MDator` targets `net10.0` only. .NET 8 and .NET 9 both leave
support on 2026-11-10, so net8.0 gains no adopters and net9.0 would tie 1.x
to an out-of-support runtime (a TFM cannot be removed after 1.0). .NET 10 is
LTS until 2028-11-14. `MDator.Abstractions` and the generator stay
netstandard2.0, so handler libraries are unaffected.

## 7. Fill the known test holes — Open

No coverage today for: stream pipeline behaviors, cancellation (no test passes
a cancelled token), `IRequestExceptionAction` firing, exception-handler
ordering, generator negative paths (no handler, duplicate handlers),
incrementality of the generator's `EquatableArray` / `TypeRef` model, and
behavior lifetimes.

## 8. Benchmarks and docs polish — Open

- A committed BenchmarkDotNet project (MDator vs MediatR 12: startup, warm
  `Send`, `Publish` fan-out, streams, fallback path). The README currently
  makes only qualitative performance claims.
- Fold the ROADMAP.md "Known Limitations" into the README where users will
  find them.
- Add a cross-assembly handler to `MDator.Samples.Domain`; the most novel
  feature has no sample coverage.
- Clear stale internal planning files out of `docs/`.

## Suggested sequencing

| Release | Scope |
|---------|-------|
| 0.7 | Items 1–5: snapshot tests, packaging assertion, compat fixes, PublicAPI analyzer, SourceLink / symbols, AOT annotations, CI matrix |
| 0.8 | Items 6–8: TFM decision, benchmarks, docs |
| 1.0 | Remove obsolete aliases (`ForEachAwaitPublisher`), resolve the namespace-split decision, tag |

Items 1–3 are the 1.0 blockers. Item 6 must be *decided* before 1.0 even if
no code changes. 1.0 should be a stamp on something already proven.
