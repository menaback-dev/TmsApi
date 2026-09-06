# TMS API (ASP.NET Core)

Training Management System backend built with **.NET 10**, **EF Core**, **PostgreSQL**, **ASP.NET Core Identity**, and **JWT**.

Consumed by the Angular **tms-client** application.

---

## Solution layout

```text
TmsApi.sln
TmsApi.Api/              Controllers, Program.cs, middleware, auth endpoints
TmsApi.Application/      DTOs, interfaces, MediatR handlers, grading, helpers
TmsApi.Domain/           Entities (Student, Course, Enrollment, …)
TmsApi.Infrastructure/   EF Core, Identity, TokenService, migrations
TmsApi.Tests/            xUnit, NSubstitute, WebApplicationFactory
```

Dependency rule: **Api → Application / Infrastructure → Domain** (clean architecture).

---

## Stack

| Area | Technology |
|------|------------|
| Runtime | .NET 10 |
| HTTP | ASP.NET Core (controllers + host pipeline) |
| Data | EF Core + Npgsql (PostgreSQL) |
| Auth | Identity + JWT Bearer + refresh token rotation |
| Patterns | CQRS (MediatR), Result types, authorization policies |
| API docs | OpenAPI / Scalar |
| Cross-cutting | CORS, rate limiting, security headers, ProblemDetails |
| Realtime | SignalR (as implemented for notifications / enrollment updates) |
| Tests | xUnit, NSubstitute, `WebApplicationFactory` |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL
- Optional: `dotnet tool install -g dotnet-ef`

---

## Configuration

### Connection string

In `TmsApi.Api/appsettings.Development.json` (example):

```json
{
  "ConnectionStrings": {
    "TmsDatabase": "Host=localhost;Port=5432;Database=TmsDb;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "Issuer": "http://localhost:5013",
    "Audience": "tms-client",
    "ExpiryMinutes": 15
  }
}
```

### JWT signing key (user secrets — never commit the real key)

```bash
cd TmsApi.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "A-Very-Long-Secret-Key-For-TMS-Auth-Stored-Safely-2026"
```

### CORS

Configure a named policy (e.g. `TmsClient`) that allows the Angular origin `http://localhost:4200` and the methods/headers your SPA needs.

---

## Database

```bash
dotnet ef migrations add <MigrationName> \
  --project TmsApi.Infrastructure \
  --startup-project TmsApi.Api

dotnet ef database update \
  --project TmsApi.Infrastructure \
  --startup-project TmsApi.Api
```

Identity creates standard `AspNet*` tables. Domain tables include `Students`, `Courses`, `Enrollments`, `RefreshTokens`, and related entities from the lab modules.

### Student registration numbers

When a user registers with role **Student**, the API also inserts a `Student` row with:

```text
TMS-{year}-{####}
```

Example sequence: existing `TMS-2026-0001` … `TMS-2026-0005` → next **`TMS-2026-0006`**.

The register response includes:

```json
{
  "message": "Registration successful.",
  "studentId": "TMS-2026-0006"
}
```

(`studentId` is `null` for Instructor / Admin registrations.)

Enrollment create expects this registration number (or your API’s agreed `studentId` format) together with `courseCode`.

---

## Run

```bash
dotnet run --project TmsApi.Api
```

- Default HTTP URL is often `http://localhost:5013` (see `Properties/launchSettings.json`)
- Scalar / OpenAPI: e.g. `http://localhost:5013/scalar/v1` when mapped in `Program.cs`

---

## Auth API (summary)

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/api/auth/register` | Create Identity user; create `Student` + `TMS-…` id when role is Student |
| `POST` | `/api/auth/login` | Returns `accessToken` and `refreshToken` |
| `POST` | `/api/auth/refresh` | Issues new pair; detects refresh-token reuse (theft) |

Password policy (as configured in Identity options): minimum length, uppercase, digit, non-alphanumeric, etc.  
Failed logins can trigger **lockout** (e.g. HTTP 423).

Protected endpoints expect:

```http
Authorization: Bearer <accessToken>
```

---

## Domain API (summary)

| Area | Typical endpoints |
|------|-------------------|
| Courses | `GET` / `POST /api/courses`, `PUT /api/courses/{id}`, query `page` & `pageSize` |
| Enrollments | `GET /api/enrollments`, `POST /api/enrollments` with `{ studentId, courseCode }`, approve, delete |
| Grades | Instructor grade submission endpoints (as implemented in your solution) |

**Enrollment flow**

1. Student: `POST /api/enrollments` → row **Pending**
2. Admin: approve → **Approved**
3. Instructor client: shows approved roster only

---

## Authorization

- Roles: `Student`, `Instructor`, `Admin` (normalize casing in the client if tokens emit lowercase)
- Example resource policy: course edit only if the instructor owns the course (or caller is Admin)

---

## Tests

```bash
dotnet test
```

Coverage includes:

- **Unit:** `GradingService` (`[Fact]` / `[Theory]` boundaries)
- **Mocking:** MediatR handlers with NSubstitute (e.g. enroll command paths)
- **Integration:** `WebApplicationFactory` + EF InMemory (skip relational `Migrate()` when provider is not relational)

---

## Security (as implemented in labs)

- JWT validation (issuer, audience, lifetime, signing key)
- Refresh token rotation and reuse detection
- Rate limiting on sensitive routes (e.g. login)
- Security response headers
- RFC 7807-style ProblemDetails where configured

---

## Local ports

| Service | Typical port |
|---------|----------------|
| TMS API | `5013` |
| Angular client | `4200` |

---

## Related repository

Frontend SPA: **tms-client** (Angular 22 + Tailwind/daisyUI + Material).

---

## License

Private / coursework — update as appropriate for your institution.
