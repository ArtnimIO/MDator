# MDator Samples

Comprehensive samples demonstrating MDator's features through two applications sharing a common inventory/product catalog domain.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Projects

| Project | Description |
|---------|-------------|
| **MDator.Samples.Domain** | Shared domain models and repository interfaces |
| **MDator.Samples.Console** | Interactive menu-driven console app — learn each feature in isolation |
| **MDator.Samples.WebApi** | ASP.NET Core Minimal API — see features working together in a realistic app |

## Quick Start

### Console Explorer

```bash
dotnet run --project MDator.Samples.Console
```

Pick a menu item to explore a feature. Each demo sets up its own DI container and runs independently.

### Web API

```bash
dotnet run --project MDator.Samples.WebApi
```

Open `requests.http` in your IDE (VS Code, Rider, Visual Studio) to test endpoints interactively, or use curl:

```bash
curl http://localhost:5000/products
```

## Feature Map

| MDator Feature | Console Menu | Web API |
|----------------|-------------|---------|
| Request/Response (`Send<T>`) | 1. Basic Requests | All endpoints |
| Void Requests (`Send`) | 1. Basic Requests | `DELETE /products/{id}` |
| Notifications (`Publish`) | 2. Notifications | `POST /products`, `POST /stock/{id}/adjust` |
| Open Behaviors | 3. Pipeline Behaviors | Logging, Validation, Transaction |
| Closed Behaviors | 3. Pipeline Behaviors | StockAuditBehavior |
| Streaming (`CreateStream`) | 4. Streaming | `GET /products/stream`, `GET /stock/alerts/stream` |
| Validation | 5. Validation | `POST /products`, `PUT /products/{id}` |
| Exception Handlers | 6. Exception Handling | `GET /products/{id}` (not found) |
| Exception Actions | 6. Exception Handling | ExceptionLoggingAction |
| Pre-Processors | 7. Pre/Post Processors | CreateProductTimingPreProcessor |
| Post-Processors | 7. Pre/Post Processors | CreateProductPostProcessor |

## Coming from MediatR?

- Replace `using MediatR;` with `using MDator;`
- Replace `services.AddMediatR(...)` with `services.AddMDator(...)`
- Declare open behaviors with `[assembly: MDator.OpenBehavior(typeof(MyBehavior<,>), Order = N)]` instead of `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(...))`
- Configure notification publisher strategies via `MDatorConfiguration.NotificationPublisher` (e.g., `new TaskWhenAllPublisher()`)
- See the [main README migration guide](../README.md#migrating-from-mediatr) for full details
