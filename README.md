# ZeissAssessment – Product Management API

A production-oriented Product Management REST API built with **.NET 10**, **ASP.NET Core**, **Entity Framework Core (SQL Server, code-first)**, **FluentValidation**, and **xUnit**. The solution follows **Clean Architecture** with clear separation between Domain, Application, Infrastructure, and API layers.

## Solution Structure

```
src/
  ZeissAssessment.Domain          -> Entities, domain exceptions, domain invariants
  ZeissAssessment.Application     -> DTOs, services, validators, abstractions (interfaces)
  ZeissAssessment.Infrastructure  -> EF Core DbContext, repository, migrations, id generator, seeder
  ZeissAssessment.Api             -> Controllers, DI wiring, exception handling, Program.cs
tests/
  ZeissAssessment.Tests.Unit          -> Domain + application service unit tests (SQLite in-memory)
  ZeissAssessment.Tests.Integration   -> WebApplicationFactory end-to-end HTTP tests
```

Dependency direction: **Api → Infrastructure → Application → Domain**. Domain has zero external dependencies.

## Endpoints

| Method | Route | Description |
| ------ | ----- | ----------- |
| GET    | `/api/products` | List all products (includes stock) |
| GET    | `/api/products/{id}` | Get a single product |
| POST   | `/api/products` | Create a product |
| PUT    | `/api/products/{id}` | Replace a product |
| DELETE | `/api/products/{id}` | Delete a product |
| POST   | `/api/products/{id}/decrement-stock/{quantity}` | Decrement stock (409 if insufficient) |
| POST   | `/api/products/{id}/add-to-stock/{quantity}` | Increment stock |
| GET    | `/api/products/search?name=...` | Case-insensitive partial name search |
| GET    | `/api/products/stock-level?min=&max=` | Products with stock within range |

All list responses include the current `stock` field for every product.

## Architectural Decisions & Trade-offs

### 1. Unique 6-digit product IDs (concurrent-safe)
The `Product` primary key is a 6-digit integer generated from a **SQL Server sequence** (`ProductIdSequence`, range `100_000 – 999_999`). Sequences are atomic at the database level, so multiple API instances running concurrently cannot produce duplicates. `IProductIdGenerator` is the abstraction; `SqlServerProductIdGenerator` calls `NEXT VALUE FOR` on the sequence, and `InMemoryProductIdGenerator` is used in tests. A CHECK constraint (`CK_Products_Id_SixDigits`) enforces the range at the schema level as a defence-in-depth measure.

Trade-off: If the process fetches an id and then fails before insert, the id is "lost" (gaps are acceptable). A GUID would sidestep this but breaks the 6-digit requirement.

### 2. Clean Architecture
- **Domain**: `Product` owns invariants (`DecrementStock`, `IncrementStock`) so business rules are enforced in one place.
- **Application**: DTOs, `IProductService`, `ProductService`, FluentValidation validators, and repository abstractions.
- **Infrastructure**: EF Core `ProductDbContext`, entity configurations, migrations, `ProductRepository`, id generator, seeder.
- **Api**: Thin controllers that delegate to `IProductService`. No business logic in controllers.

### 3. EF Core best practices
- **`AsNoTracking`** for all read paths (`GetAll`, `GetById`, `Search`, `StockLevel`).
- **Tracking queries** only when updating (`GetByIdForUpdateAsync`).
- **`RowVersion` concurrency token** on `Product` to detect concurrent stock modifications; conflicts surface as HTTP 409.
- **Async** everywhere with `CancellationToken` support.
- **CHECK constraints** for `Stock >= 0` and `Price >= 0` at the database level.
- **Retry on failure** enabled on the SQL Server connection.
- **Sequences** used instead of `IDENTITY` to satisfy the 6-digit-id requirement without race conditions.

### 4. Validation
FluentValidation validators live in the Application layer (`CreateProductRequestValidator`, `UpdateProductRequestValidator`) and are enforced by a global `FluentValidationFilter` on the MVC pipeline. Failures throw `ValidationException`, which the `GlobalExceptionHandler` converts to RFC 7807 `ProblemDetails` with per-field errors.

### 5. Error handling
`GlobalExceptionHandler` (implements `IExceptionHandler`) maps exceptions to consistent problem responses:

| Exception | HTTP status |
| --------- | ----------- |
| `ProductNotFoundException` | 404 |
| `ValidationException` | 400 |
| `InvalidStockOperationException` | 409 |
| `ArgumentException` / `ArgumentOutOfRangeException` | 400 |
| `DbUpdateConcurrencyException` | 409 |
| Any other exception | 500 |

### 6. Seeding
`ProductDbContextSeeder` runs at startup after `MigrateAsync()`, populating five sample products only when the table is empty. Seed data uses the same id generator so ids remain unique across environments.

### 7. Testing strategy
- **Unit tests** (`ZeissAssessment.Tests.Unit`): domain invariants, `ProductService` behaviour (against SQLite in-memory), validators.
- **Integration tests** (`ZeissAssessment.Tests.Integration`): full HTTP stack via `WebApplicationFactory<Program>` with SQLite substituted for SQL Server. Covers the CRUD lifecycle, validation failures, stock conflict handling, search, and stock-level queries.

Focus is on behavioural correctness (edge cases: not found, insufficient stock, invalid inputs) rather than line coverage.

## Running

### Prerequisites
- .NET 10 SDK
- SQL Server LocalDB (or update `ConnectionStrings:ProductDb` in `appsettings.json`)
- `dotnet ef` tool: `dotnet tool install --global dotnet-ef --version 10.0.0`

### Migrations
Migrations are pre-generated in `src/ZeissAssessment.Infrastructure/Persistence/Migrations` and applied automatically at startup. To regenerate:

```powershell
dotnet ef migrations add <Name> --project src\ZeissAssessment.Infrastructure --startup-project src\ZeissAssessment.Api --output-dir Persistence\Migrations
```

### Run the API
```powershell
dotnet run --project src\ZeissAssessment.Api
```

### Run tests
```powershell
dotnet test
```

## Assumptions
- The 6-digit id space (900,000 values) is sufficient for the domain scope. If exhaustion becomes a risk, switch to a wider generator (e.g. bigint or GUID).
- Stock is an `int` (non-negative). Overflow is not a realistic concern for the target volume.
- "Partial match" search means case-insensitive `LIKE %name%`. Full-text search was considered out of scope.
- The API is single-tenant; there is no multi-tenancy or authorization layer (would be added via ASP.NET Identity / JWT in production).
- SQL Server is the production database. Tests use SQLite for isolation/speed; production behaviours specific to SQL Server (sequences, `rowversion`) are exercised via migrations locally.
