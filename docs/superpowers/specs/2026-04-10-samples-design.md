# MDator Samples Design Spec

**Date:** 2026-04-10
**Status:** Approved

## Overview

Add a `samples/` folder with comprehensive sample projects demonstrating MDator's full feature set. Two host applications — a menu-driven console explorer for learning features in isolation, and an ASP.NET Core Minimal API showing realistic usage — share a common inventory/product catalog domain.

Inspired by MediatR's samples but differentiated: MediatR's samples focus on DI container integration; MDator's samples focus on feature depth, vertical slice architecture, and a realistic domain.

## Audience

Layered progression targeting all skill levels:

- **Library evaluators** — see that it works and is easy to adopt
- **New adopters** — learn each feature through the interactive console app
- **Advanced users** — explore streaming, exception pipelines, open behavior ordering, notification cascading in the web API

## Project Structure

```
samples/
├── Samples.slnx                              # Standalone solution (NuGet refs)
├── MDator.Samples.Domain/                     # Shared inventory domain models
│   ├── Models/
│   │   ├── Product.cs
│   │   ├── Category.cs
│   │   └── StockAlert.cs
│   └── Interfaces/
│       ├── IProductRepository.cs
│       ├── ICategoryRepository.cs
│       └── IStockAlertRepository.cs
├── MDator.Samples.Console/                    # Menu-driven feature explorer
│   ├── Program.cs                             # Interactive menu
│   ├── Features/
│   │   ├── 01-BasicRequests/
│   │   ├── 02-Notifications/
│   │   ├── 03-PipelineBehaviors/
│   │   ├── 04-Streaming/
│   │   ├── 05-Validation/
│   │   ├── 06-ExceptionHandling/
│   │   └── 07-PrePostProcessors/
│   └── Infrastructure/
│       └── InMemoryRepositories.cs
└── MDator.Samples.WebApi/                     # Realistic inventory API
    ├── Program.cs
    ├── Infrastructure/
    │   ├── AppDbContext.cs
    │   └── Repositories/
    ├── Features/
    │   ├── Products/
    │   ├── Categories/
    │   └── Stock/
    ├── Behaviors/
    ├── Notifications/
    ├── ExceptionHandling/
    └── Processors/
```

## Shared Domain Library (MDator.Samples.Domain)

A thin, dependency-free class library containing the inventory domain.

### Models

- **Product** — Id, Name, Sku, Description, Price, StockQuantity, CategoryId, CreatedAt, UpdatedAt
- **Category** — Id, Name, Description
- **StockAlert** — Id, ProductId, AlertType (LowStock/OutOfStock/Restocked), Threshold, CurrentQuantity, CreatedAt

### Interfaces

- **IProductRepository** — GetById, GetAll, Search, Add, Update, Delete
- **ICategoryRepository** — GetById, GetAll, Add
- **IStockAlertRepository** — Add, GetByProduct, GetRecent

No MDator dependency. Each sample provides its own repository implementations.

## Console Sample (MDator.Samples.Console)

Menu-driven console app teaching each MDator feature in isolation. Uses simple in-memory repository implementations (`Dictionary<Guid, T>` behind the domain interfaces).

### Menu Structure

```
=== MDator Samples ===
1. Basic Requests
2. Notifications
3. Pipeline Behaviors
4. Streaming
5. Validation
6. Exception Handling
7. Pre/Post Processors
0. Exit
```

### Feature Breakdown

**1. Basic Requests**
- `CreateProduct` command returning `Product`
- `GetProductById` query
- `DeleteProduct` void request
- Demonstrates `Send<T>()` and `Send()`

**2. Notifications**
- `ProductCreatedNotification` with three handlers (inventory indexer, audit logger, email sender)
- Runs once with `ForEachAwaitPublisher`, once with `TaskWhenAllPublisher` to show the difference

**3. Pipeline Behaviors**
- `LoggingBehavior<,>` (open, Order=0)
- `TimingBehavior<,>` (open, Order=1)
- `ProductCommandAuditBehavior` (closed, only wraps product commands)
- Shows ordering and selective application

**4. Streaming**
- `StreamAllProducts` request returning `IAsyncEnumerable<Product>`
- Populates 20 products, streams them with a small delay to show async enumeration

**5. Validation**
- `CreateProductValidator` using FluentValidation (Name required, Price > 0, etc.)
- `ValidationBehavior<,>` rejects invalid input before the handler runs

**6. Exception Handling**
- `ReduceStock` handler throws `InsufficientStockException`
- `IRequestExceptionHandler` catches and converts to an error result
- `IRequestExceptionAction` logs the error regardless

**7. Pre/Post Processors**
- `AuditPreProcessor` logs request details before handler
- `AuditPostProcessor` logs response after handler
- Shows they run inside the innermost behavior scope

### Presentation

- Each menu item prints a brief explanation of the feature being demonstrated
- Sets up a fresh `ServiceCollection` with just what's needed for that feature
- Uses `Console.ForegroundColor` to visually distinguish pipeline stages

## Web API Sample (MDator.Samples.WebApi)

ASP.NET Core Minimal API modelling an inventory management system. Uses EF Core (in-memory provider) and FluentValidation. Vertical slice architecture.

### Dependencies

- MDator
- Microsoft.EntityFrameworkCore.InMemory
- FluentValidation
- FluentValidation.DependencyInjectionExtensions

### Feature Files

Each feature is a single file containing request, handler, validator, and endpoint mapping method.

**Products:**
- `CreateProduct.cs` — Command + Handler + Validator + Endpoint
- `UpdateProduct.cs` — Command + Handler + Validator + Endpoint
- `DeleteProduct.cs` — Void request + Handler + Endpoint
- `GetProduct.cs` — Query + Handler + Endpoint
- `ListProducts.cs` — Query with pagination + Handler + Endpoint
- `SearchProducts.cs` — Query with filtering + Handler + Endpoint
- `StreamProducts.cs` — IStreamRequest → SSE endpoint

**Categories:**
- `CreateCategory.cs` — Command + Handler + Endpoint
- `GetCategory.cs` — Query + Handler + Endpoint
- `ListCategories.cs` — Query + Handler + Endpoint

**Stock:**
- `AdjustStock.cs` — Command that triggers notifications + Handler + Endpoint
- `GetStockAlerts.cs` — Query + Handler + Endpoint
- `StreamStockAlerts.cs` — Real-time alert streaming (SSE)

### Behaviors

| Behavior | Type | Order | Purpose |
|----------|------|-------|---------|
| `LoggingBehavior<,>` | Open | 0 | Logs request/response |
| `ValidationBehavior<,>` | Open | 1 | FluentValidation integration |
| `TransactionBehavior<,>` | Open | 2 | Wraps commands in EF Core transactions |
| `StockAuditBehavior` | Closed | — | Only wraps stock adjustment commands |

### Notifications

- **ProductCreated** — published after product creation, handlers for indexing and logging
- **StockLevelChanged** — published after stock adjustment, evaluates thresholds
- **LowStockDetected** — cascaded from StockLevelChanged when threshold breached

### Exception Handling

- **ProductNotFoundHandler** — `IRequestExceptionHandler` converting `ProductNotFoundException` to a 404-friendly result
- **ExceptionLoggingAction** — `IRequestExceptionAction` logging all unhandled exceptions

### Processors

- **RequestTimingPreProcessor** — stamps request start time
- **ResponseEnrichmentPostProcessor** — adds metadata to responses

### Endpoints

| Method | Route | MDator Feature |
|--------|-------|---------------|
| POST | `/products` | Command + Validation + Transaction |
| GET | `/products/{id}` | Query + Exception handling |
| GET | `/products` | Query + Pagination |
| GET | `/products/search` | Query + Filtering |
| PUT | `/products/{id}` | Command + Validation |
| DELETE | `/products/{id}` | Void request |
| GET | `/products/stream` | Streaming (SSE) |
| POST | `/stock/{productId}/adjust` | Command + Notifications + Closed behavior |
| GET | `/stock/alerts` | Query |
| GET | `/stock/alerts/stream` | Streaming (SSE) |
| POST | `/categories` | Command |
| GET | `/categories` | Query |
| GET | `/categories/{id}` | Query |

### Seed Data

The web API seeds the in-memory database in `Program.cs` with a handful of products, categories, and stock levels so it's immediately explorable.

## Solution Structure & Project References

### Dual-Mode Referencing

Each sample `.csproj` uses a conditional ItemGroup:

```xml
<ItemGroup Condition="'$(UseMDatorSource)' != 'true'">
  <PackageReference Include="MDator" Version="*" />
</ItemGroup>
<ItemGroup Condition="'$(UseMDatorSource)' == 'true'">
  <ProjectReference Include="../../src/MDator/MDator.csproj" />
</ItemGroup>
```

- `Samples.slnx` — builds without the property; uses NuGet (consumer experience)
- `MDator.slnx` — passes `/p:UseMDatorSource=true` (or sets it in the root `Directory.Build.props`), so project references activate when building from the root solution

### Target Framework

Samples target `net10.0` only.

### Nuke Integration

Add a `SampleCompile` target that:
1. Runs after `Pack`
2. Restores `Samples.slnx` with a local NuGet source pointing at `output/`
3. Builds the samples against the just-packed packages
4. Validates that the consumer experience works in CI

## Documentation

### samples/README.md

- Overview of what's included
- Prerequisites (dotnet SDK 10)
- How to run each sample
- Feature map table showing which MDator feature each console menu item and web API endpoint demonstrates
- "Coming from MediatR?" section (3-5 bullets):
  - `using MediatR;` → `using MDator;`
  - `services.AddMediatR(...)` → `services.AddMDator(...)`
  - Open behaviors: `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(...))` → `[assembly: OpenBehavior(typeof(...), Order = N)]`
  - Notification publisher strategies via `MDatorConfiguration.NotificationPublisher`
  - Link to main README migration section

### Per-Sample READMEs

- `MDator.Samples.Console/README.md` — what each menu item demonstrates, how to run
- `MDator.Samples.WebApi/README.md` — endpoint table, how to run, how to test with curl

### HTTP File

`MDator.Samples.WebApi/requests.http` — example requests for every endpoint with sample payloads, usable in VS Code / Rider / Visual Studio.
