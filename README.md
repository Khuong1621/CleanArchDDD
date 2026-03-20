# 🏗️ CleanArch DDD — .NET 8 Boilerplate

> **Senior-level** .NET 8 solution với Clean Architecture + DDD + CQRS + Swagger

---

## 📐 Architecture Overview

```
CleanArchDDD/
├── src/
│   ├── Domain/                     ← Core business logic (NO dependencies)
│   │   ├── Aggregates/             ← Product, Order (Aggregate Roots)
│   │   ├── Entities/               ← BaseEntity (Id, Audit, DomainEvents)
│   │   ├── ValueObjects/           ← Money, Address (immutable)
│   │   ├── Events/                 ← IDomainEvent, Domain Events
│   │   ├── Enums/                  ← ProductStatus, OrderStatus
│   │   ├── Exceptions/             ← DomainException, NotFoundException
│   │   └── Interfaces/             ← IRepository, IUnitOfWork
│   │
│   ├── Application/                ← Use Cases (depends on Domain only)
│   │   ├── Common/
│   │   │   ├── Behaviors/          ← Logging, Validation, Performance (Pipeline)
│   │   │   ├── Exceptions/         ← ValidationException, NotFoundException
│   │   │   └── Result.cs           ← Result<T>, PagedResult<T>
│   │   ├── Features/
│   │   │   ├── Products/
│   │   │   │   ├── Commands/       ← CreateProduct, UpdatePrice, Delete
│   │   │   │   ├── Queries/        ← GetProducts, GetProductById
│   │   │   │   └── EventHandlers/  ← ProductCreatedHandler
│   │   │   └── Orders/
│   │   │       └── Commands/       ← CreateOrder, ConfirmOrder
│   │   ├── DTOs/                   ← ProductDto, OrderDto
│   │   └── DependencyInjection.cs
│   │
│   ├── Infrastructure/             ← External concerns (EF Core, Repos)
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs     ← EF + Domain Event dispatch
│   │   │   ├── UnitOfWork.cs
│   │   │   ├── Configurations/     ← EF Fluent API configs
│   │   │   └── Repositories/       ← Generic + specific repos
│   │   └── DependencyInjection.cs
│   │
    ├── API/                        ← Presentation Layer (Web API)
    │   ├── Controllers/            ← ProductsController, OrdersController
    │   ├── Middleware/             ← GlobalExceptionMiddleware
    │   └── Program.cs              ← DI, Swagger, Versioning, Serilog
    │
    └── WinFormsUI/                 ← Presentation Layer (Desktop UI - MVP Pattern)
        ├── Views/                  ← WinForms & View Interfaces (IMainView)
        ├── Presenters/             ← UI Logic and Event Handling (MainPresenter)
        ├── Models/                 ← UI-specific data models
        └── Program.cs              ← DI Container and Application Entry Point
│
└── tests/
    ├── Domain.Tests/
    └── Application.Tests/
```

---

## 🔑 Key Patterns

| Pattern | Implementation |
|---------|---------------|
| **DDD** | Aggregate Roots, Value Objects, Domain Events |
| **CQRS** | MediatR Commands & Queries, separate handlers |
| **Clean Architecture** | Dependency Rule: Domain ← Application ← Infrastructure ← API |
| **Pipeline Behaviors** | Logging → Validation → Performance (MediatR pipeline) |
| **Unit of Work** | Transaction management, repo coordination |
| **Result Pattern** | `Result<T>` instead of exceptions for expected failures |
| **Domain Events** | Dispatched post-SaveChanges via MediatR |
| **API Versioning** | `/api/v1/...` URL-based versioning |

---

## 🚀 Getting Started

```bash
# 1. Clone & restore
dotnet restore

# 2. Run API (uses InMemory DB by default)
cd src/API
dotnet run

# 3. Run WinForms (Requires Windows)
cd src/WinFormsUI
dotnet run

# 4. Open Swagger UI
# http://localhost:5000 (Swagger loads at root)
```

### With SQL Server
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CleanArchDB;Trusted_Connection=True;"
  }
}
```
Then run migrations:
```bash
dotnet ef migrations add InitialCreate -p src/Infrastructure -s src/API
dotnet ef database update -p src/Infrastructure -s src/API
```

---

## 📡 API Endpoints

### Products
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/products` | Get paged products |
| `GET` | `/api/v1/products/{id}` | Get product by ID |
| `POST` | `/api/v1/products` | Create product |
| `PATCH` | `/api/v1/products/{id}/price` | Update price |
| `DELETE` | `/api/v1/products/{id}` | Soft delete |

### Orders
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/v1/orders` | Create order |
| `POST` | `/api/v1/orders/{id}/confirm` | Confirm order |

### System
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/health` | Health check |

---

## 🏛️ Domain Rules (DDD Business Logic)

- `Product` cannot have negative price or stock
- `Order` can only be confirmed when in `Pending` status
- `Order` cannot add items after confirmation
- `Product` stock is automatically deducted when order is created (with transaction rollback on failure)
- All entities have audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- Soft delete via `IsDeleted` flag + EF Query Filter

---

## 🔧 Dependencies

| Package | Purpose |
|---------|---------|
| `MediatR` | CQRS Commands/Queries/Events |
| `FluentValidation` | Input validation in pipeline |
| `Entity Framework Core 8` | ORM + Migrations |
| `Swashbuckle.AspNetCore` | Swagger/OpenAPI UI |
| `Serilog` | Structured logging |
