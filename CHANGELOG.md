# Changelog

All notable changes to MDator will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Releases are cut by tagging `vX.Y.Z` on `main`; the publish workflow then packs
and pushes to NuGet. GitHub auto-generated release notes cover the full commit
list — this file curates the user-visible changes.

## [Unreleased]

### Fixed

- `AddMDator` no longer throws `InvalidOperationException: Collection was
  modified` when a registration callback indirectly triggers loading of another
  handler-bearing assembly mid-iteration. The iteration over
  `MDatorGeneratedHook.Registrations` now uses an index loop so module
  initializers that append during the call are picked up too.

### Added

- `RuntimeDispatch.PublishFallback` dispatches unknown notification types when
  no compile-time switch arm matches.
- `MDATOR0001` analyzer flags no-op MediatR-compat shim methods.

## [0.4.0]

- Cache compiled delegates in `RuntimeDispatch` fallback paths.
- Pin vulnerable transitive dependencies to fixed versions.
- Add missing XML comments and remove the `CS1591` suppression.

For prior releases see the [GitHub Releases](https://github.com/ArtnimIO/MDator/releases).

[Unreleased]: https://github.com/ArtnimIO/MDator/compare/v0.4.0...HEAD
[0.4.0]: https://github.com/ArtnimIO/MDator/compare/v0.3.0...v0.4.0
