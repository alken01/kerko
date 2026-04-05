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
- [x] **1.5 String interpolation in logging** — `SearchController.cs:25,46,68` uses `$""` which defeats structured logging. Switch to message templates: `_logger.LogInformation("Search | emri: {Emri}", emri)`.
- [x] **1.6 Input max-length caps** — Min-length validated but no max. A 10KB query string causes unnecessary work. Add `[StringLength(100)]` or manual cap.
- [ ] **1.7 Remove `userScalable: false`** — Violates WCAG 1.4.4, blocks pinch-to-zoom for low-vision users. Remove from `viewport.tsx`.
- [ ] **1.8 `useEffect` stale closure** — `page.tsx:47-64` references handlers not in the dependency array. Wrap in `useCallback` or use refs.
- [ ] **1.9 `key={index}` in result lists** — `SearchResultsTabs.tsx:232,239,252,269`. Use composite keys for proper React reconciliation.

---

## 2. Performance

- [ ] **2.1 Double count+fetch** — Every search runs `CountAsync()` then paginated fetch (2 DB roundtrips with same WHERE). Use `COUNT(*) OVER()` window function or fetch `pageSize + 1` to detect next page.
- [ ] **2.2 Response caching** — All data is static/read-only. Add `[ResponseCache(Duration = 60)]` or `Cache-Control` headers to search endpoints.
- [ ] **2.3 Response compression** — Add `app.UseResponseCompression()` with gzip/brotli. JSON results compress 5-10x.
- [ ] **2.4 AbortController for requests** — No request cancellation on frontend. Rapid searches race and stale results can flash. Pass `AbortSignal` to `fetchApi`, abort on new search.
- [ ] **2.5 SQLite FTS5** — All searches use `LIKE '%term%'` (full table scan). FTS5 virtual tables on name columns would be 10-100x faster. *(Prerequisite for scaling beyond current dataset size.)*
- [ ] **2.6 Search debouncing** — No debounce on search submissions. Add 300ms debounce to prevent duplicate requests on double-tap/fast typing.

---

## 3. Security

- [x] **3.1 LIKE pattern sanitization** — Fixed.
- [x] **3.2 Rate limiter real IP** — Fixed.
- [ ] **3.3 CORS from environment variables** — Origins hardcoded in `appsettings.json`. Allow override via env vars for container deployments without config rebuild.
- [ ] **3.4 Remove or apply API key middleware** — `RequireApiKeyAttribute` exists but is unused dead code. Either apply it to admin endpoints or remove it.
- [x] **3.5 Input max-length** — (Same as 1.6, listed here for security context.)

---

## 4. Testing & CI/CD

- [ ] **4.1 Backend unit tests** — SearchService logic: diacritic normalization, pagination validation, edge cases. Use xUnit + `WebApplicationFactory` for integration tests.
- [ ] **4.2 Frontend component tests** — React Testing Library for SearchForm, result cards, pagination. Vitest or Jest.
- [ ] **4.3 GitHub Actions pipeline** — `dotnet build` + `dotnet test` + `npm run build` + `npm run lint` on every PR.
- [ ] **4.4 Pre-commit hooks** — `husky` + `lint-staged` for frontend. `dotnet format` for backend.

---

## 5. New Features

- [ ] **5.1 Personal Number (Numri Personal) search** — Cross-reference a person across Rrogat, Targat, and Patronazhist tables using their unique ID. Add a 4th search tab. *Effort: Low. Impact: High — most powerful cross-reference query possible.*
- [ ] **5.2 Search history** — Store recent searches in IndexedDB (reuse existing `idb` infrastructure). Show dropdown under search form with last 10 searches. *Effort: Low.*
- [ ] **5.3 Cross-reference "See also" links** — From any result card, link to search that person's name in other tables. Use existing URL params (`?emri=X&mbiemri=Y`). *Effort: Low.*
- [ ] **5.4 Employer (NIPT) search** — Search Rrogat by company tax ID to find all employees of a company. *Effort: Low.*
- [ ] **5.5 City/address filter** — Optional city dropdown on name search using Person.Qyteti. Reduces false positives for common names. *Effort: Medium.*
- [ ] **5.6 Export saved items** — CSV/JSON download of bookmarked items. Data already structured in IndexedDB. *Effort: Low.*
- [ ] **5.7 Share button** — "Copy link" / Web Share API button. URL params already support this, just surface it. *Effort: Low.*
- [ ] **5.8 Keyboard shortcuts** — `/` to focus search, `Esc` to clear, number keys for tabs. *Effort: Low.*
- [ ] **5.9 Result statistics summary** — "Found in 3/4 databases" header, total count across tabs. *Effort: Low.*
- [ ] **5.10 DB status on frontend** — Show "Searching X records" using existing `DbStatusAsync()`. Builds user trust. *Effort: Low.*
- [ ] **5.11 Swagger UI** — Swashbuckle already a dependency. Enable Swagger UI in dev for API exploration. *Effort: Trivial.*

---

## 6. UX Polish

- [ ] **6.1 Scroll to results** — Auto-scroll to first result after search on mobile (results often below fold).
- [ ] **6.2 Empty state** — Replace plain error text with illustration + search suggestions when no results found.
- [ ] **6.3 Per-tab pagination** — Each result tab should maintain its own page state. Currently switching tabs resets pagination.
- [ ] **6.4 Hardcoded dark mode grays** — Some components (PersonCard.tsx:70-72) use `text-gray-400` instead of semantic CSS variables. Doesn't adapt to dark mode.
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
- [x] **7.5 Health check with DB ping** — `/api/health` returns "OK" without checking DB. Use `AddHealthChecks().AddDbContextCheck<>()`.

---

## Priority Matrix

| Priority | Items |
|----------|-------|
| **Do first** | 1.5, 1.6, 1.7, 2.4, 4.3 |
| **High value, low effort** | 5.1, 5.2, 5.3, 5.7, 5.10, 5.11, 7.1, 7.5 |
| **Performance wins** | 2.1, 2.2, 2.3, 2.5 |
| **When scaling** | 6.3, 6.5, 7.2, 7.4 |
| **Nice to have** | 5.4, 5.5, 5.6, 5.8, 5.9, 6.1, 6.2, 6.4 |
