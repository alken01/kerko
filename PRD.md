# Kerko — Product Requirements Document

**Last updated:** 2026-04-05
**Stack:** .NET 10 (ASP.NET Core) + Next.js 15 (React 19) + SQLite
**Deployment:** Backend on Docker (VPS/Fly.io), Frontend on Vercel

---

## 1. Bugs & Critical Fixes

- [x] **1.1 DbContext thread safety** — `Task.WhenAll` on a single scoped DbContext is unsafe. Run queries sequentially or use `IDbContextFactory`.
- [x] **1.2 Diacritic variant explosion** — `GenerateAlbanianVariants` grows O(3^n). Replaced with SQL-side `REPLACE()` normalization.
- [x] **1.3 LIKE pattern injection** — User input passed raw into LIKE patterns. Fixed with `EscapeLikePattern()` + `ESCAPE '\'`.
- [x] **1.4 Rate limiter IP behind proxy** — `RemoteIpAddress` was proxy IP. Fixed with `ForwardedHeadersMiddleware`.
- [x] **1.5 String interpolation in logging** — Replaced `$""` with message templates in all log calls.
- [x] **1.6 Input max-length caps** — Added `MaxInputLength = 100` validation in all search methods.
- [x] **1.7 Remove `userScalable: false`** — Removed from layout.tsx viewport config. WCAG compliant.
- [x] **1.8 `useEffect` stale closure** — Wrapped handlers in `useCallback`, fixed dependency arrays.
- [x] **1.9 `key={index}` in result lists** — Replaced with composite keys from data fields.

---

## 2. Performance

- [ ] **2.1 Double count+fetch** — Every search runs `CountAsync()` then paginated fetch (2 DB roundtrips with same WHERE). Use `COUNT(*) OVER()` window function or fetch `pageSize + 1` to detect next page.
- [x] **2.2 Response caching** — Added `[ResponseCache(Duration = 60, VaryByQueryKeys = ["*"])]` on controller.
- [x] **2.3 Response compression** — Added brotli/gzip compression middleware in Program.cs.
- [x] **2.4 AbortController for requests** — Added AbortController that cancels previous request on new search.
- [ ] **2.5 SQLite FTS5** — All searches use `LIKE '%term%'` (full table scan). FTS5 virtual tables on name columns would be 10-100x faster. *(Prerequisite for scaling beyond current dataset size.)*
- [x] **2.6 Search debouncing** — AbortController handles rapid search cancellation; form uses submit-based (not live) search.

---

## 3. Security

- [x] **3.1 LIKE pattern sanitization** — Fixed.
- [x] **3.2 Rate limiter real IP** — Fixed.
- [x] **3.3 CORS from environment variables** — Added `CORS_ORIGINS` env var override (comma-separated).
- [x] **3.4 Remove or apply API key middleware** — Removed unused `RequireApiKeyAttribute` and middleware.
- [x] **3.5 Input max-length** — (Same as 1.6, listed here for security context.)

---

## 4. Testing & CI/CD

- [x] **4.1 Backend unit tests** — 43 NUnit integration tests: diacritics, pagination, validation, LIKE escaping, boundary clamping, DbStatus.
- [ ] **4.2 Frontend component tests** — React Testing Library for SearchForm, result cards, pagination. Vitest or Jest.
- [ ] **4.3 GitHub Actions pipeline** — `dotnet build` + `dotnet test` + `npm run build` + `npm run lint` on every PR.
- [ ] **4.4 Pre-commit hooks** — `husky` + `lint-staged` for frontend. `dotnet format` for backend.

---

## 5. New Features

- [x] **5.1 Personal Number (Numri Personal) search** — New `/api/numripersonal` endpoint + NP tab in search form.
- [x] **5.2 Search history** — IndexedDB storage, recent searches dropdown under search form.
- [x] **5.3 Cross-reference "See also" links** — Name links on RrogatCard, PatronazhistCard, TargatCard headers.
- [x] **5.4 Employer (NIPT) search** — New `/api/nipt` endpoint searching Rrogat by company tax ID.
- [x] **5.5 City/address filter** — Optional `qyteti` parameter on `/api/kerko` filters Person results by city.
- [x] **5.6 Export saved items** — JSON download from saved items panel.
- [x] **5.7 Share button** — Copy link button in results tabs header.
- [x] **5.8 Keyboard shortcuts** — `/` to focus search, `Esc` to clear.
- [x] **5.9 Result statistics summary** — "Found in X/Y databases" + total count above tabs.
- [x] **5.10 DB status on frontend** — DbStatus component showing "Searching X records" + `/api/dbstatus` endpoint.
- [x] **5.11 Swagger UI** — Enabled in development via `UseSwagger()` + `UseSwaggerUI()`.

---

## 6. UX Polish

- [x] **6.1 Scroll to results** — Auto-scroll to results on mobile after search.
- [x] **6.2 Empty state** — Friendly empty state with :/ face and search suggestion when no results found.
- [ ] **6.3 Per-tab pagination** — Each result tab should maintain its own page state. Currently switching tabs resets pagination.
- [x] **6.4 Hardcoded dark mode grays** — Replaced `hover:text-blue-600` with `hover:text-blue-700 dark:hover:text-blue-400`.
- [ ] **6.5 Infinite scroll option** — "Load more" button as alternative to pagination, especially on mobile.

---

## 7. Infrastructure & Deployment

### Deployment strategy

| Option | Cost | Cold start | SQLite compatible | Notes |
|--------|------|------------|-------------------|-------|
| **VPS (Hetzner/DO)** | $4-6/mo | None | Yes | Simplest. Best for SQLite. |
| **Fly.io** | $5-7/mo | None (min 1 machine) | Yes (volumes) | Docker-native, auto-TLS, easy scaling. |
| **Railway** | $5-10/mo | None | Yes (volumes) | Git-push deploy, simple. |
| **ECS Fargate** | $10-15/mo | None | Yes (EFS, slower) | AWS-native. Overkill for this scale. |
| **AWS Lambda** | ~$2/mo | 1-3s (.NET) | No (needs RDS) | Bad fit: cold starts + no filesystem for SQLite. |

**Recommendation:** Fly.io or a small VPS. SQLite on a persistent volume is the simplest architecture and performs well for this read-heavy workload. Lambda only makes sense if you migrate to PostgreSQL first, and even then cold starts hurt the search UX.

### TODOs

- [ ] **7.1 `.env.example` files** — Neither backend nor frontend has one. New devs have to guess required env vars.
- [ ] **7.2 Full-stack Docker Compose** — Current compose only has backend. Add frontend service for local dev.
- [ ] **7.3 Version tagging** — No git tags or semver. Add tags for release tracking and rollback.
- [ ] **7.4 Database backup strategy** — 1.1GB SQLite in a Docker volume with no backup. Add cron backup script or document volume backup.
- [x] **7.5 Health check with DB ping** — Added `AddHealthChecks().AddDbContextCheck<>()` + `MapHealthChecks("/api/health")`.

---

## Priority Matrix

| Priority | Items |
|----------|-------|
| **Do first** | ~~1.5, 1.6, 1.7, 2.4, 4.3~~ (done except 4.3) |
| **High value, low effort** | ~~5.1, 5.2, 5.3, 5.7, 5.10, 5.11, 7.1, 7.5~~ (done except 7.1) |
| **Performance wins** | ~~2.2, 2.3~~ done; 2.1, 2.5 remaining |
| **When scaling** | 6.3, 6.5, 7.2, 7.4 |
| **Nice to have** | ~~5.4, 5.5, 5.6, 5.8, 5.9, 6.1, 6.2, 6.4~~ all done |

### Remaining items (not done in this pass)
- **2.1** Double count+fetch — requires API contract change or window functions
- **2.5** SQLite FTS5 — major DB schema migration
- **4.2** Frontend component tests — requires Vitest setup
- **4.3** GitHub Actions pipeline — CI/CD infra (skipped per user request)
- **4.4** Pre-commit hooks — dev infra (skipped per user request)
- **6.3** Per-tab pagination — complex state management change
- **6.5** Infinite scroll — significant UX change
- **7.1–7.4** Infrastructure items — skipped per user request
