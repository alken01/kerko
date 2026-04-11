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
- DB is SQLite, opened ReadOnly in production. Migrations require `--connection` with a writable path.
- Apply migration locally: `cd backend && dotnet ef database update --project Kerko --connection "Data Source=/Users/alken/Code/kerko/backend/data/kerko.db" -- --environment Development`
- Then scp the DB to the server

## Frontend

- `cd frontend && npm ci && npm run dev`
- Deployed automatically via Vercel on push to main

## Deployment

- Backend runs on EC2 t4g.micro (Amazon Linux 2023, ARM64) in eu-central-1
- Docker container exposes port 8080
- API Gateway (HTTP API) provides HTTPS at `https://kqullbamb0.execute-api.eu-central-1.amazonaws.com`
- CI/CD: GitHub Actions builds, tests, and deploys via SSH on push to main
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
