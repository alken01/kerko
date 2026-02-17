# Kerko Project Review — Improvements & Feature Ideas

## Overview

This document contains findings from a comprehensive review of the Kerko codebase
(backend: .NET 10 / ASP.NET Core, frontend: Next.js 15 / React 19). Each section
is categorized by priority and effort level.

---

## 1. Code Quality & Bug Fixes

### 1.1 Diacritic variant explosion (Backend — SearchService.cs:404-428)

`GenerateAlbanianVariants` produces a **combinatorial explosion**. A name like
"ece" (3 chars each mapping to 3 variants) generates 3^3 = 27 variants. A 10-char
name with 4 diacritic-eligible characters creates 3^4 = 81 variants, each turned
into a `LIKE` clause. This degrades query performance and could be exploited with
crafted inputs.

**Recommendation:** Cap the variant count (e.g., max 50) or switch to a
collation-based approach where SQLite handles case/diacritic folding via `COLLATE NOCASE`
with a custom collation, or pre-normalize column data with a computed/shadow column.

### 1.2 Parallel queries sharing a single DbContext (Backend — SearchService.cs:140)

`KerkoAsync` fires 4 parallel `Task.WhenAll` queries against the same scoped
`ApplicationDbContext`. EF Core's `DbContext` is **not thread-safe**. This works
incidentally with SQLite's serialized mode but will break under PostgreSQL/SQL Server
and can produce subtle data-corruption bugs.

**Recommendation:** Either run queries sequentially, or inject `IDbContextFactory<ApplicationDbContext>`
and create a separate context per task.

### 1.3 String interpolation in structured logging (Backend — SearchController.cs:25, 46, 68)

```csharp
_logger.LogInformation($"Search request | IP: {GetClientIpAddress()} ...");
```

Using `$""` defeats structured logging — the values won't appear as separate fields
in Application Insights or any structured sink. Use message templates instead:

```csharp
_logger.LogInformation("Search request | IP: {ClientIp} | {UserAgent}", GetClientIpAddress(), ...);
```

### 1.4 Double count+fetch pattern (Backend — SearchService.cs:182-197, 242-269, 365-371)

Every search runs `CountAsync()` then the actual paginated query, hitting the
database twice with the same expensive WHERE clause.

**Recommendation:** Use a single query with a window function (`COUNT(*) OVER()`)
or load one extra row to detect "has next page" without a separate count.

### 1.5 `useEffect` missing dependency warning (Frontend — page.tsx:53)

```tsx
useEffect(() => { ... handleSearch ... }, [searchParams]);
```

`handleSearch`, `handleSearchTarga`, `handleSearchTelefon` are in the dependency
closure but not listed. This works today but is fragile. Wrap handlers in
`useCallback` or move search logic into the effect.

### 1.6 User-scalable disabled (Frontend — layout.tsx:14)

```ts
userScalable: false
```

This prevents pinch-to-zoom, which is an **accessibility violation** (WCAG 1.4.4).
Many users with low vision depend on zoom. Remove this unless there's a strong
product reason.

---

## 2. Architecture & Performance Improvements

### 2.1 Add response caching / ETag support (Backend)

Search queries are read-only against a static dataset. Adding `ResponseCaching`
middleware or ETag-based conditional responses would dramatically reduce load for
repeated queries. Since the database doesn't change frequently, even a 60-second
cache would help.

### 2.2 Add a `/api/db-status` endpoint to the frontend (Frontend)

The backend has `DbStatusAsync()` but the frontend doesn't use it. Showing record
counts (e.g., "Searching 1.1M+ records across 4 databases") builds user confidence
and serves as a health indicator.

### 2.3 Search debouncing / request cancellation (Frontend)

There's no `AbortController` usage or debouncing. If a user rapidly changes search
terms or pages, all requests fire and race. The last response displayed may not
match the last query submitted.

**Recommendation:** Add `AbortController` to `fetchApi`, cancel in-flight requests
on new searches, and debounce rapid input changes.

### 2.4 Replace SQLite with PostgreSQL for production (Backend)

SQLite is fine for development and small deployments, but the 1.1GB database size
is pushing its practical limits for concurrent read workloads. The project already
includes `Npgsql.EntityFrameworkCore.PostgreSQL` as a dependency — it should be
used in production for better concurrency, full-text search, and proper collation
support for Albanian characters.

### 2.5 Add full-text search (Backend)

Currently using `LIKE '%term%'` which cannot use indexes (index only helps with
prefix matches). For a 1.1GB database this is slow. PostgreSQL's `tsvector` / GIN
indexes, or SQLite FTS5 would dramatically improve query performance.

### 2.6 Connection pooling and health checks (Backend)

The health endpoint (`/api/health`) just returns "OK" without checking database
connectivity. Use ASP.NET Core's built-in health check framework:

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();
```

---

## 3. Security Improvements

### 3.1 Input sanitization for LIKE patterns (Backend)

User input is passed directly into `LIKE` patterns. Characters like `%`, `_`, and
`[` have special meaning in SQL LIKE. A user searching for `%` would match
everything. Escape these characters before constructing patterns.

### 3.2 Rate limiter uses RemoteIpAddress which is unreliable behind proxies (Backend — Program.cs:29)

```csharp
partitionKey: context.Connection.RemoteIpAddress?.ToString()
```

Behind a reverse proxy (common with Docker), this will be the proxy IP, not the
client. All users would share one rate-limit bucket. Use `X-Forwarded-For` as the
controller already does for logging, or configure `ForwardedHeadersMiddleware`.

### 3.3 CORS hardcoded origins should come from environment (Backend — appsettings.json)

The allowed origins (`kerko.vercel.app`, `localhost:3000`) are in the config file.
This is fine, but consider allowing override via environment variables for
containerized deployments without rebuilding.

### 3.4 API key not used for public endpoints (Backend)

The `RequireApiKeyAttribute` exists but isn't applied to any endpoint. Either
remove the dead code or apply it to the `db-status` endpoint (if you add one) to
prevent abuse. The public search endpoints probably should remain open.

### 3.5 Add request size limits and input length caps (Backend)

Query parameters have minimum length checks but no maximum. A very long search
string (e.g., 10,000 characters) combined with diacritic variant generation could
cause memory/CPU exhaustion. Add `[StringLength]` or manual max-length validation.

---

## 4. Testing & CI/CD

### 4.1 Add automated tests (High Priority)

Tests were removed ("Update to dotnet 10, remove the tests"). This should be
restored as a priority:

- **Backend unit tests:** SearchService logic (diacritic normalization, variant
  generation, pagination validation). These are pure logic and easy to test.
- **Backend integration tests:** API endpoint tests using `WebApplicationFactory`.
- **Frontend tests:** Component rendering tests with React Testing Library.

### 4.2 Add GitHub Actions CI pipeline

No CI/CD exists. A minimal pipeline should:
1. Build the backend (`dotnet build`)
2. Run backend tests (`dotnet test`)
3. Build the frontend (`npm run build`)
4. Run frontend lint (`npm run lint`)
5. (Optional) Run E2E tests

### 4.3 Add a pre-commit hook or lint step

Use `husky` + `lint-staged` for the frontend to enforce formatting before commits.
For the backend, `dotnet format` can be integrated.

---

## 5. New Feature Ideas

### 5.1 Personal Number (Numri Personal) search

The `Rrogat`, `Targat`, and `Patronazhist` tables all have a `NumriPersonal`
column. A dedicated search by personal number would let users find all records
across tables for the same individual — a powerful cross-reference feature.

**Effort:** Low — the backend generic search pattern makes this straightforward.

### 5.2 Search history (Frontend)

Store recent searches in IndexedDB (like saved items) so users can quickly
re-execute previous queries. Show as a dropdown under the search form.

**Effort:** Low — reuse the existing `idb` storage service pattern.

### 5.3 Export saved items (Frontend)

Allow users to export their saved items as CSV or JSON. The data is already
structured in IndexedDB — just add a download button.

**Effort:** Low.

### 5.4 Advanced/combined search (Backend + Frontend)

Allow searching with partial information — e.g., first name + city, or last name +
birth year. Currently, both first and last name are required. A more flexible query
builder would increase usability.

**Effort:** Medium.

### 5.5 Result cross-linking

When viewing a person's record, show a "See also" section with matching records
from other tables (same Numri Personal or same name). This is partially done via
the multi-table search but could be more explicit in the UI.

**Effort:** Medium.

### 5.6 Statistics / analytics dashboard

Show aggregate statistics like number of records per city, most common surnames,
age distribution, etc. This could use pre-computed aggregates so it doesn't
require live queries.

**Effort:** Medium.

### 5.7 Share search results via URL (Frontend)

Search params are already in the URL, but adding a "Copy link" or native share
button (using the Web Share API) would make it more discoverable. This is mostly a
UI addition.

**Effort:** Low.

### 5.8 Keyboard shortcuts (Frontend)

Add shortcuts for common actions: `/` to focus search, `Escape` to clear, arrow
keys for pagination, `Tab` between result tabs. Improves power-user experience.

**Effort:** Low.

### 5.9 Swagger/OpenAPI documentation (Backend)

Swashbuckle is already a dependency but there's no visible Swagger UI
configuration. Enabling it (at least in development) would help with API
exploration and third-party integration.

**Effort:** Very Low — just configure the middleware in `Program.cs`.

### 5.10 i18n / multi-language support (Frontend)

The app is currently Albanian-only. Adding English (or other language) support
using `next-intl` or similar would broaden the audience, especially for diaspora
users.

**Effort:** Medium.

---

## 6. Frontend UX Improvements

### 6.1 Loading skeletons instead of blank state

Currently, the search just shows nothing while loading. Skeleton cards matching
the result card layout would provide better perceived performance.

### 6.2 Scroll to results after search

After a search completes, the results area may be below the fold on mobile.
Auto-scrolling to the first result improves the experience.

### 6.3 Empty state illustrations

When no results are found, show a more engaging empty state than just an error
message — e.g., a simple illustration with suggestions.

### 6.4 Per-tab pagination

In a name search, all 4 tables share one pagination. If the user switches from
"Persona" to "Rrogat" tab, pagination resets. Each tab should maintain its own
page state so switching tabs doesn't lose your place.

### 6.5 Infinite scroll option

For mobile users especially, infinite scroll (or "Load more" button) may be more
natural than pagination.

### 6.6 Dark mode: hardcoded gray values

Some components use hardcoded Tailwind grays (e.g., `text-gray-400`,
`hover:text-gray-600` in PersonCard.tsx:70-72) instead of the semantic CSS
variables. These don't adapt to dark mode properly.

---

## 7. DevOps & Infrastructure

### 7.1 Add `.env.example` files

Neither backend nor frontend has an `.env.example`. New developers have to guess
what environment variables are needed (`NEXT_PUBLIC_API_URL`, `AdminApiKey`,
`ConnectionStrings__DefaultConnection`, etc.).

### 7.2 Docker Compose for full-stack local development

The current `docker-compose.yml` only defines the backend. A full-stack compose
file with both backend and frontend services would streamline local development.

### 7.3 Add version tagging

No git tags or version numbers exist. Use semantic versioning with tags so you can
track releases and rollback if needed.

### 7.4 Database backup strategy

With a 1.1GB SQLite database in a Docker volume, there's no documented backup
strategy. Add a cron-based backup script or document how to backup the volume.

---

## Priority Matrix

| Item | Priority | Effort | Impact |
|------|----------|--------|--------|
| 1.2 DbContext thread safety | Critical | Low | Prevents data corruption |
| 1.1 Variant explosion cap | High | Low | Prevents DoS |
| 3.1 LIKE pattern sanitization | High | Low | Prevents data leak |
| 3.2 Rate limiter IP detection | High | Low | Makes rate limiting work |
| 3.5 Input length caps | High | Low | Prevents resource exhaustion |
| 4.1 Add automated tests | High | Medium | Prevents regressions |
| 1.3 Structured logging | Medium | Low | Better observability |
| 2.3 Request cancellation | Medium | Low | Better UX |
| 1.6 Remove userScalable:false | Medium | Trivial | Accessibility |
| 5.1 Personal number search | Medium | Low | High-value feature |
| 2.1 Response caching | Medium | Low | Performance |
| 5.2 Search history | Low | Low | UX improvement |
| 5.9 Swagger docs | Low | Trivial | Developer experience |
| 6.1 Loading skeletons | Low | Low | UX polish |
