# Roadmap to 1.0

Condensed from the maturity assessment of 2026-08-19. The code itself is in
good shape; the gaps are about the *guarantees* a 1.0 promises: pinned
generator output, a stable public API, MediatR source compatibility, and
package hygiene. Status reflects `origin/main` as of 2026-09-15.

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

## 2. Close the MediatR v12 compat gaps — In review

Draft [#77](https://github.com/ArtnimIO/MDator/pull/77), branch
`compat/mediatr-v12-parity`, not yet merged.

The README promises migration is "a namespace find-replace". The PR makes
that true for:

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

## 3. Lock the public API — Open

1.0 means no breaking changes until 2.0. Nothing enforces that today.

- Add `Microsoft.CodeAnalysis.PublicApiAnalyzers` with `PublicAPI.Shipped.txt`
  / `PublicAPI.Unshipped.txt` to `MDator` and `MDator.Abstractions`.
- Move `MDATOR0001` from `AnalyzerReleases.Unshipped.md` to `Shipped` (it
  shipped in 0.5.0).

## 4. NuGet package hygiene — Open

All absent today:

- SourceLink (`Microsoft.SourceLink.GitHub`), symbol packages (`snupkg`),
  `ContinuousIntegrationBuild`, `EmbedUntrackedSources`.
- `global.json` pinning the CI SDK.
- Trimming / AOT decision: `RuntimeDispatch` uses `Expression.Compile()` and
  `MakeGenericMethod` with no `RequiresUnreferencedCode` /
  `RequiresDynamicCode` annotations, so NativeAOT consumers get silent
  breakage instead of warnings. Annotate it or document it.

## 5. Widen and harden CI — Open

- CI runs on Ubuntu only. Add a Windows leg.
- Tests run on net10.0 only; the shipped net9.0 assembly is never executed.
  Run tests per TFM.
- The `SampleCompile` build target is never invoked in CI, so samples can rot
  silently. Add it to the PR workflow.

## 6. Decide the TFM floor — Open (decision, not code)

`MDator` targets `net9.0;net10.0`. MediatR supports netstandard2.0 / net6.0+,
so anyone on net8.0 LTS cannot adopt MDator. Adding a TFM later is easy;
removing one is a breaking change. Decide before tagging 1.0.

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
| 0.7 | Items 1–2: snapshot tests, packaging assertion, compat fixes |
| 0.8 | Items 3–5: PublicAPI analyzer, SourceLink / deterministic builds, CI matrix |
| 0.9 | Items 6–8: TFM decision, benchmarks, docs |
| 1.0 | Remove obsolete aliases (`ForEachAwaitPublisher`), resolve the namespace-split decision, tag |

Items 1–3 are the 1.0 blockers. Item 6 must be *decided* before 1.0 even if
no code changes. 1.0 should be a stamp on something already proven.
