# 🧪 Copilot Minimal API Evaluation Tasks (.NET, C#)

Test plan for evaluating **GitHub Copilot (agent mode)** in an **Empty ASP.NET Core Minimal API** project (`dotnet new web`).  
Each task includes: the **agent instruction** and **acceptance criteria**.

---

## ✅ Test 1: Bootstrap & Essentials (Routing, Swagger, Config, Logging)

**Agent Instruction**

> Create a .NET minimal API starting from an Empty ASP.NET Core template.  
> Add endpoints:
> - `GET /health` → returns `{ "status": "OK" }`
> - `GET /time` → returns the current UTC time as ISO 8601  
> Enable Swagger/OpenAPI in Development only.  
> Read an `AppName` setting from configuration (`appsettings.json`) and log it at startup using `ILogger`.  
> Use structured logging (log level Information for requests).  
> Keep everything idiomatic minimal API style.

**Acceptance Criteria**

- `GET /health` returns JSON `{ "status": "OK" }`
- `GET /time` returns ISO 8601 UTC time
- Swagger available under `/swagger` **only in Development**
- `AppName` value logged on startup
- No redundant boilerplate or classes

---

## ✅ Test 2: CRUD Todo In-Memory + Validation

**Agent Instruction**

> Add an in-memory CRUD for a `Todo` resource with fields:  
> - `Id` (int)  
> - `Title` (required, 1–100 chars)  
> - `IsDone` (bool)  
> - `DueDate` (nullable, must be future if provided)  
>
> Expose routes:
> - `GET /todos` (paginated: `page`, `pageSize`)
> - `GET /todos/{id}`
> - `POST /todos` (returns 201 + Location)
> - `PUT /todos/{id}`
> - `DELETE /todos/{id}`  
>
> Return **ProblemDetails** for validation errors (400) and not found (404).  
> Use request/response DTOs to prevent over-posting.  
> Keep implementation minimal and idiomatic.

**Acceptance Criteria**

- Validation for title and future date works (400 with ProblemDetails)
- `POST` returns 201 + `Location` header
- 404 returned for non-existing `id`
- Pagination works via query parameters
- Data stored in memory (no database yet)

---

## ✅ Test 3: Persistence with EF Core (SQLite) + Migrations

**Agent Instruction**

> Replace the in-memory `Todo` store with EF Core using SQLite (`Data Source=app.db`).  
> Create an `AppDbContext` with a `Todos` DbSet.  
> Add the initial migration and update the database automatically at startup if pending migrations exist.  
> Use minimal mapping between DTOs and entity.  
> Keep the routes from the previous task intact.

**Acceptance Criteria**

- `AppDbContext` configured with connection string in `appsettings.json`
- Migrations created and applied automatically at startup
- `app.db` file created and populated
- CRUD endpoints work exactly as before
- Uses EF Core idiomatically

---

## ✅ Test 4: AuthN/AuthZ with JWT + Protected Routes

**Agent Instruction**

> Add JWT Bearer authentication.  
> Provide a minimal `POST /auth/login` endpoint that accepts username/password  
> (hardcode a single user for demo) and returns a signed JWT (HS256)  
> with `sub` and `role` claim (`"user"`).  
>
> Protect all `POST/PUT/DELETE /todos` endpoints so they require a valid Bearer token;  
> allow `GET` endpoints anonymously.  
> Add an `[Authorize]` policy requiring the `role=user` claim.  
> Document the security scheme in Swagger and enable the Authorize button.

**Acceptance Criteria**

- `POST /auth/login` returns JWT token
- `POST/PUT/DELETE` without token → 401 or 403
- Authorized requests with valid token succeed
- Swagger shows Authorize button and security scheme
- JWT includes correct claims (`sub`, `role`)

---

## ✅ Test 5: Production Hardening  
(Error Handling, Rate Limiting, Logging, Integration Tests)

**Agent Instruction**

> Add:
> 1. A global exception handler returning RFC 7807 ProblemDetails (map known exceptions to 400/404, others to 500).  
> 2. Built-in rate limiting middleware: limit `/todos` write operations to **10 requests/minute per IP**, with proper `Retry-After` header.  
> 3. Request logging using `UseHttpLogging` and a correlation ID middleware that adds `X-Correlation-Id` if missing and logs it in the scope.  
> 4. A separate test project (xUnit + WebApplicationFactory) verifying:
>    - `GET /health` returns 200 and `{ "status": "OK" }`
>    - `POST /todos` without token returns 401
>    - `POST /todos` with valid token returns 201 + Location  
>
> Keep code minimal and idiomatic for .NET 8.

**Acceptance Criteria**

- ProblemDetails for handled/unhandled exceptions with correct `type`, `title`, `status`
- Rate limit returns 429 Too Many Requests after 10 writes/minute
- Every response includes `X-Correlation-Id` header
- Integration tests compile and pass
- Implementation follows .NET 8 minimal API idioms

---

## 🧭 Evaluation Rubric

Use these criteria to assess the Copilot model:

| Criterion | Description |
|------------|--------------|
| **Correctness** | Endpoints, HTTP codes, validation, and auth work as expected |
| **Code Quality** | Clean, idiomatic minimal API, no over-engineering |
| **Developer Experience** | Clear guidance, minimal setup friction |
| **Operational Readiness** | Logging, error handling, config, rate limiting implemented |
| **Independence** | Agent autonomously adds missing packages, migrations, and setup |
