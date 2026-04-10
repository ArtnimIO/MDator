# Nuke Build System Design

## Overview

Replace the raw `dotnet` CLI calls in the GitHub Actions workflow with a Nuke build project that owns the full pipeline: clean, restore, compile, test, pack, and publish. As part of this change, `net8.0` is dropped from `MDator.csproj` targets since .NET 10 is LTS — the library now targets `net9.0;net10.0`.

## Build Project Structure

A `build/` directory at the repo root containing:

- `_build.csproj` — console project targeting `net10.0`, referencing `Nuke.Common`
- `Build.cs` — build class inheriting `NukeBuild`, defines all targets
- Bootstrapper scripts (`build.sh`, `build.cmd`) placed at the repo root by `nuke :setup`
- `.nuke/` directory at repo root with solution/params config

The `_build.csproj` is NOT added to `MDator.slnx` — it is a standalone build orchestrator, not part of the product.

## Targets

Linear dependency chain:

```
Clean → Restore → Compile → Test → Pack → Publish
```

| Target    | Action                                                                       |
|-----------|------------------------------------------------------------------------------|
| Clean     | Deletes `output/` directory                                                  |
| Restore   | `dotnet restore MDator.slnx`                                                |
| Compile   | `dotnet build MDator.slnx -c <Configuration> --no-restore`                  |
| Test      | `dotnet test MDator.slnx -c <Configuration> --no-build`                     |
| Pack      | Packs `MDator` and `MDator.Abstractions` to `output/`                       |
| Publish   | `dotnet nuget push output/*.nupkg` to NuGet.org using `NuGetApiKey` param   |

## Parameters

| Parameter     | Source                         | Default                                        | Required by |
|---------------|--------------------------------|------------------------------------------------|-------------|
| Configuration | `CONFIGURATION` env / CLI arg  | `Release` on CI (`IsServerBuild`), `Debug` local | Compile, Test, Pack |
| NuGetApiKey   | `NUGET_APIKEY` env / CLI arg   | —                                              | Publish     |

## CI Workflow Changes

The GitHub Actions workflow (`publish.yml`) simplifies to:

### `build-test` job
1. Checkout with `fetch-depth: 0` (full history for Nerdbank.GitVersioning)
2. Setup .NET SDKs: `9.0.x`, `10.0.x`
3. Run `./build.sh Compile Test Pack`
4. Upload `output/*.nupkg` as artifact

### `publish` job
1. Download artifact
2. Setup .NET SDK `10.0.x`
3. Run `./build.sh Publish` with `NUGET_APIKEY` env var

## Output Directory

Nuke's conventional `output/` directory replaces the previous `artifacts/` directory.

## .gitignore

Add `.nuke/temp/` to `.gitignore`. The `.nuke/` config files (e.g., `parameters.json`) are committed.

## Packable Projects

Only these two projects are packed:

- `src/MDator/MDator.csproj`
- `src/MDator.Abstractions/MDator.Abstractions.csproj`

`MDator.SourceGenerator` is bundled inside the `MDator` package and is not packed separately.

## Solution File

`MDator.slnx` remains the solution used for restore/build/test. The build project is not added to it.
