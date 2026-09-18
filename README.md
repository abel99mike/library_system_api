# Library System API

A small ASP.NET Core Web API for managing a library's books, users, and loans.

## Tech stack

- ASP.NET Core 9 Web API (controllers)
- EF Core 9 with the **SQLite** provider — a single file (`library.db`), no database server required, and data persists across restarts
- Swashbuckle / Swagger UI for interactive API docs
- xUnit for unit/integration-style tests (against EF Core's InMemory provider, for fast/isolated test runs)

## Project structure

```
library_system_api/
├── LibrarySystem.sln
├── src/
│   └── LibrarySystem.Api/
│       ├── Controllers/       # Thin HTTP layer: routing, status codes
│       ├── Services/          # Business rules (uniqueness, loan state, etc.)
│       ├── Repositories/      # EF Core data access, one per aggregate
│       ├── Models/            # EF Core entities (Book, User, Loan)
│       ├── DTOs/              # Request/response contracts, decoupled from entities
│       ├── Data/               # LibraryDbContext + DbSeeder (fake-data seeding, `dotnet run -- seed`)
│       ├── Migrations/         # EF Core migrations for the SQLite schema
│       ├── Middleware/        # Global exception handling
│       ├── Exceptions/        # NotFoundException, ConflictException, ValidationException
│       └── Program.cs         # Composition root / DI wiring
└── tests/
    └── LibrarySystem.Tests/   # Service-layer tests against a real EF Core InMemory context
```

## Running the API

```bash
dotnet restore
dotnet run --project src/LibrarySystem.Api
```

The API starts on the URL printed in the console (e.g. `http://localhost:5299`). Swagger UI is available at `/swagger` in the Development environment and doubles as a way to try every endpoint by hand.

The database is a single SQLite file, `library.db`, created next to the running executable the first time the app starts (see `ConnectionStrings:LibraryDb` in `appsettings.json`). `Program.cs` calls `dbContext.Database.Migrate()` on startup, so the schema is created/updated automatically from the `Migrations/` folder — there's nothing to run by hand, and no database server to install. Data persists across restarts; delete `library.db` (and its `-shm`/`-wal` sidecar files, if present) to reset to an empty database.

To add a schema change later: update the models, then run `dotnet ef migrations add <Name> --project src/LibrarySystem.Api` (requires the `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`). The next app startup applies it automatically.

### Running with VS Code

Open the repository root folder in VS Code with the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) (or the standalone [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)) extension installed. The checked-in `.vscode/launch.json` and `.vscode/tasks.json` provide:

- **Run and Debug (F5)** → "Run API (http)": builds the project and starts it with breakpoint support, on `http://localhost:5044`. The browser opens automatically once the server is ready.
- **Terminal ▸ Run Task ▸ build**: a plain `dotnet build` of the API project, wired up so compiler errors show in the Problems panel.
- **Terminal ▸ Run Task ▸ watch**: runs `dotnet watch run`, restarting the API on file changes — useful when you don't need the debugger attached.

Swagger UI (`/swagger`) is still the easiest way to exercise the endpoints once the server is running, whether started via F5 or from the terminal.

### Seeding sample data

```bash
dotnet run --project src/LibrarySystem.Api -- seed
```

This applies migrations (if needed) and populates the database with 1,000 books, 200 users, and 500 loans (450 already returned, 50 still active), all generated with [Bogus](https://github.com/bchavez/Bogus) using a fixed random seed for reproducibility. It's a no-op if the database already has any books, users, or loans, so it's safe to run more than once. Delete `library.db` first if you want to regenerate from scratch.

Swapping to a different real database (SQL Server/PostgreSQL) is a one-line change in `Program.cs` (replace `UseSqlite` with `UseSqlServer`/`UseNpgsql`) plus a new connection string and a fresh migration.

## Running the tests

```bash
dotnet test
```

Tests exercise the service layer against a real (InMemory) `LibraryDbContext` and repositories rather than mocked repositories, so the tests validate the actual EF Core queries and business rules together (uniqueness checks, loan/return state transitions, filtering) without needing a mocking framework.

## API overview

### Books — `/api/books`
| Method | Route | Description |
|---|---|---|
| GET | `/api/books?isAvailable={bool}&author={text}` | List all books, optionally filtered |
| GET | `/api/books/{id}` | Get a single book |
| POST | `/api/books` | Create a book |
| PUT | `/api/books/{id}` | Update a book |
| DELETE | `/api/books/{id}` | Delete a book (409 if currently on loan) |

### Users — `/api/users`
| Method | Route | Description |
|---|---|---|
| GET | `/api/users` | List all registered users |
| POST | `/api/users` | Register a new user |

### Loans — `/api/loans`
| Method | Route | Description |
|---|---|---|
| GET | `/api/loans/active` | List loans that haven't been returned |
| POST | `/api/loans` | Borrow a book (`{ userId, bookId }`) |
| PUT | `/api/loans/{id}/return` | Return a borrowed book |

## Key design decisions

- **SQLite for the API, EF Core InMemory for tests.** SQLite gives the deliverable real persistence (a single portable file, no server to install) while keeping the "clone and run" experience trivial. Tests use EF Core's InMemory provider instead purely for speed and per-test isolation (a fresh, disposable database per test with no file I/O); since the tests exercise the service layer's business rules rather than SQL-specific behavior, the difference in provider doesn't affect what's being verified.
- **Layered architecture (Controller → Service → Repository → DbContext).** Controllers only translate HTTP in/out and map results to status codes; all business rules (ISBN/email uniqueness, "can't delete a book on loan", "can't borrow an unavailable book", "can't return a loan twice") live in the service layer, and all EF Core querying lives behind repository interfaces. This keeps each layer independently testable and swappable — for example the InMemory provider could be replaced with SQL Server without touching services or controllers.
- **DTOs instead of exposing entities.** `BookCreateDto`/`BookUpdateDto`/`BookResponseDto` (and their User/Loan equivalents) decouple the wire contract from the EF Core model, so entity navigation properties (e.g. `Loan.User`, `Loan.Book`) never leak into API responses and validation attributes can live on the input DTOs without polluting the domain model.
- **Domain-specific exceptions + global middleware.** Services throw `NotFoundException`, `ConflictException`, or `ValidationException` for business-rule violations; `ExceptionHandlingMiddleware` maps those to the correct HTTP status codes (404/409/400) and a consistent `ProblemDetails` body, and treats anything else as a logged 500. This avoids repeating try/catch or status-code logic in every controller action.
- **`IsAvailable` is a stored, service-managed flag rather than a computed property.** It's flipped to `false` when a loan is created and back to `true` when it's returned. This keeps the common "list available books" query a simple indexed filter rather than a join against loans, at the cost of the service layer being responsible for keeping it in sync (acceptable for a single-process API with no direct DB writes outside the service layer).
- **Uniqueness enforced in the service layer, not only in the database.** `ExistsByIsbnAsync`/`ExistsByEmailAsync` checks run before insert so the API can return a clean `409 Conflict` with a helpful message instead of surfacing a raw database constraint exception. A unique index is still declared in `LibraryDbContext` as a defense-in-depth safety net.
- **Repository + Service pattern over a generic repository.** Each aggregate (Book, User, Loan) gets its own repository interface with the specific queries it actually needs (e.g. `HasActiveLoanAsync`, `GetActiveLoansAsync`) rather than a generic `IRepository<T>`, since the queries aren't uniform across entities and a generic abstraction would just be method call indirection with no real reuse.
- **Async all the way down.** Every controller action and repository/service method is `async`/`await` over EF Core's async APIs, since that's the correct default for I/O-bound ASP.NET Core code even against the InMemory provider.

## Possible next steps

- Swap SQLite for a networked database (SQL Server/PostgreSQL) via a connection string and a fresh migration, if the API needs to run against a shared/remote database.
- Add pagination to the list endpoints.
- Add authentication/authorization (e.g. so only a user can return their own loan).
