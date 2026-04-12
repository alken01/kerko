# Kerko

## Project Structure

- `backend/` — .NET 10 ASP.NET Core API with SQLite (EF Core)
- `frontend/` — Next.js 15 + React 19, deployed on Vercel at `kerko.vercel.app`

## Backend

- Entry point: `backend/Kerko/Program.cs`
- Solution file: `backend/Kerko.sln`
- Build: `cd backend && dotnet build`
- Test: `cd backend && dotnet test`
- Docker: `cd backend && docker build -t kerko-backend .`
- Main DB is SQLite (`kerko.db`), opened ReadOnly in production. Migrations require `--connection` with a writable path.
- Analytics DB is separate SQLite (`analytics.db`), read-write, auto-created on startup
- Apply migration locally: `cd backend && dotnet ef database update --project Kerko --connection "Data Source=/Users/alken/Code/kerko/backend/data/kerko.db" -- --environment Development`
- Then scp the DB to the server
- Admin endpoints (`/api/admin/*`) require `X-Admin-Token` header matching `KERKO_ADMIN_TOKEN` env var

## Frontend

- `cd frontend && npm ci && npm run dev`
- Deployed automatically via Vercel on push to main

## Deployment

- Backend runs on EC2 t4g.micro (Amazon Linux 2023, ARM64) in eu-central-1
- Docker container exposes port 8080
- API Gateway (HTTP API) provides HTTPS at `https://kqullbamb0.execute-api.eu-central-1.amazonaws.com`
- API Gateway has a parameter mapping: `header.X-Client-IP` → `$context.identity.sourceIp` (required for client IP extraction — API Gateway blocks overriding `X-Forwarded-For`)
- CI/CD: GitHub Actions builds, tests, and deploys via SSH on push to main
- Docker run: `docker run -d --name kerko-backend -p 8080:8080 -v ~/code/kerko/backend/data:/app/data -e KERKO_ADMIN_TOKEN="..." kerko-backend`
- Debug logs: `docker logs kerko-backend --tail 50`
- SQLite DB lives on the server at `~/code/kerko/backend/data/kerko.db` (not in git)
- DB migrations must be applied locally then re-uploaded via scp
- SSH key: `backend/Kerko/certificates/kerko-key.pem` (gitignored)

## Sensitive Files (gitignored)

- `*.pem` — SSH keys and certificates
- `backend/data/kerko.db` — production database (1.1GB)
- `.env.local` — frontend environment variables
- `appsettings.*.json` — except Development

## Conventions

- Commit messages: conventional commits (`feat:`, `fix:`, `perf:`, `ci:`, `chore:`), one line max
- Never add Co-Authored-By lines to commits
- PRs: squash merge with branch deletion
- DbContext is not thread-safe — never use Task.WhenAll with parallel queries on the same context
- Client IP comes from `X-Client-IP` header (set by API Gateway), not `X-Forwarded-For` or `RemoteIpAddress` — see `ClientInfo.GetClientIpAddress()`
