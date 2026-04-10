# MDator Web API Sample

ASP.NET Core Minimal API demonstrating MDator in a realistic inventory management system with vertical slice architecture.

## Run

```bash
dotnet run
```

The API starts at `http://localhost:5000` with seeded data (5 products, 2 categories).

## Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/products` | List products (supports `?skip=0&take=50`) |
| GET | `/products/{id}` | Get product by ID |
| GET | `/products/search?term=...` | Search products by name |
| POST | `/products` | Create a product |
| PUT | `/products/{id}` | Update a product |
| DELETE | `/products/{id}` | Delete a product |
| GET | `/products/stream` | Stream all products (SSE) |
| POST | `/stock/{productId}/adjust` | Adjust stock level (`{ "delta": -5 }`) |
| GET | `/stock/alerts` | Get recent stock alerts |
| GET | `/stock/alerts/stream` | Stream stock alerts (SSE) |
| POST | `/categories` | Create a category |
| GET | `/categories` | List categories |
| GET | `/categories/{id}` | Get category by ID |

## Testing

Open `requests.http` in VS Code, Rider, or Visual Studio for pre-built request examples.

## Architecture

- **Vertical slices** — each feature is a single file with request, handler, validator, and endpoint
- **Open behaviors** — LoggingBehavior (Order=0), ValidationBehavior (Order=1), TransactionBehavior (Order=2)
- **Closed behavior** — StockAuditBehavior (only wraps stock adjustments)
- **Notification cascade** — stock adjustment → StockLevelChanged → LowStockDetected
- **Exception pipeline** — ProductNotFoundException → handled result (404) + logged via exception action
