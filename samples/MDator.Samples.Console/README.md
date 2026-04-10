# MDator Console Sample

Interactive menu-driven console app demonstrating each MDator feature in isolation.

## Run

```bash
dotnet run
```

## Menu Items

| # | Feature | What It Shows |
|---|---------|---------------|
| 1 | Basic Requests | Commands, queries, and void requests via `Send<T>()` and `Send()` |
| 2 | Notifications | Multi-handler fanout with `ForEachAwaitPublisher` vs `TaskWhenAllPublisher` |
| 3 | Pipeline Behaviors | Open behaviors with ordering, closed behaviors for specific requests |
| 4 | Streaming | `IAsyncEnumerable<T>` via `CreateStream()` |
| 5 | Validation | FluentValidation pipeline behavior rejecting invalid input |
| 6 | Exception Handling | `IRequestExceptionHandler` (converts to response) and `IRequestExceptionAction` (observes) |
| 7 | Pre/Post Processors | `IRequestPreProcessor` and `IRequestPostProcessor` running inside the behavior pipeline |
