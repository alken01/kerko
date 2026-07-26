# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Kerko

Search platform for Albanian public record databases (~1.1GB SQLite, 4 tables: `Person`, `Rrogat`, `Targat`, `Patronazhist`). Albanian diacritics (ç→c, ë→e) are normalized at both index and query time — searching `kuci` matches `Kuçi`.

## Project Structure

- `backend/` — .NET 10 ASP.NET Core API with SQLite (EF Core)
- `frontend/` — Next.js 15 + React 19 (App Router), deployed on Vercel at `kerko.vercel.app`

## Backend

- Entry point: `backend/Kerko/Program.cs`
- Solution file: `backend/Kerko.sln`
- Build: `cd backend && dotnet build`
- Test: `cd backend && dotnet test` (single test: `dotnet test --filter "FullyQualifiedName~SearchServiceTests.MethodName"`)
- Docker: `cd backend && docker build -t kerko-backend .`
- Two DbContexts: `ApplicationDbContext` (main, `DefaultConnection` → `kerko.db`, ReadOnly in prod), `AnalyticsDbContext` (`AnalyticsConnection` → `analytics.db`, read-write).
- Main DB uses EF migrations. Apply locally from `backend/`: `dotnet ef database update --project Kerko --connection "Data Source=../data/kerko.db" -- --environment Development`, then scp to server.
- Analytics DB does **not** use EF migrations — `EnsureCreated` at startup + manual `ALTER TABLE RequestLogs ADD COLUMN Location` in `Program.cs` (schema drift handled inline).
- Admin endpoints (`/api/admin/*`) require `X-Admin-Token` header matching `KERKO_ADMIN_TOKEN` env var. Enforced by `AdminAuthMiddleware`, branched via `UseWhen` on path prefix.
- Request logging: `RequestLoggingMiddleware` writes to a bounded `Channel<RequestLog>` (cap 10k, `DropWrite` on overflow). `RequestLogWriter` hosted service drains the channel into analytics DB. IP geolocation lookup via `ip-api.com` (`IpGeolocationService`).
- Rate limit: per-IP fixed window, configured via `RateLimiting:PermitLimit` / `WindowMinutes` in appsettings.
- CORS: origins come from `CORS_ORIGINS` env var (comma-separated) OR `CORS:AllowedOrigins` appsetting. Production throws if neither is set.
- `Testing` environment (used by integration tests) **skips** admin token check and rate limiter setup — see the `IsEnvironment("Testing")` guards in `Program.cs`.

## Endpoints

| Route | Purpose |
| ------ | ------- |
| `/api/kerko` | Search by first and/or last name (at least one required) across all 4 tables |
| `/api/targat` | Search by license plate |
| `/api/telefon` | Search by phone number |
| `/api/admin/*` | Admin (requires `X-Admin-Token`) |
| `/api/health` | Health check + DB connectivity |

## Frontend

- `cd frontend && npm ci && npm run dev` (runs on `:3000`)
- Build: `npm run build` / Lint: `npm run lint`
- App Router under `src/app/` (public search at `/`, admin at `/admin`).
- API client: `src/services/api.ts` (reads `NEXT_PUBLIC_API_URL`). Local storage helpers: `src/services/storage.ts`.
- i18n under `src/i18n/`. Saved items persist in IndexedDB (`src/services/storage.ts`). No PWA/service worker.
- Deployed automatically via Vercel on push to main.

## Deployment

- Backend runs locally in Docker and is exposed through ngrok.
- Vercel's `NEXT_PUBLIC_API_URL` points to the ngrok tunnel. Production search
  and admin availability therefore depend on the local Docker container,
  network connection, and ngrok agent remaining online.
- CI: `.github/workflows/ci.yml` runs path-filtered backend/frontend builds and
  tests. There is no automated backend deployment.
- Docker run: use `backend/docker-compose.yml`; local ports and admin token come
  from the gitignored `backend/.env`.
- Debug logs: `docker logs kerko-backend --tail 50`
- SQLite data lives under `backend/data/` and is not committed.

## Sensitive Files (gitignored)

- `*.pem` — SSH keys and certificates
- `backend/data/kerko.db` — production database (1.1GB)
- `.env.local` — frontend environment variables
- `appsettings.*.json` — except Development

## Conventions

- Commit messages: conventional commits (`feat:`, `fix:`, `perf:`, `ci:`, `chore:`), one line max.
- Never add Co-Authored-By lines to commits.
- PRs: squash merge with branch deletion.
- `DbContext` is not thread-safe — never use `Task.WhenAll` with parallel queries on the same context.
- `ClientInfo.GetClientIpAddress()` currently accepts `X-Client-IP` and
  `X-Forwarded-For` before `RemoteIpAddress`; any change must account for the
  ngrok proxy boundary and include spoofing tests.
- All `DateTime` values serialized with `Z` suffix via `UtcDateTimeJsonConverter` so frontend parses UTC correctly.
- Use `string.Empty` instead of `""` for empty string literals (including inside EF LINQ expressions — it translates the same).
- Test data uses Albanian public figures (e.g. Ismail Kadare) — never real personal names of contributors.
