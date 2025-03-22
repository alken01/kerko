# Kerko - Multi-Database Search Platform

## Project Overview

Kerko is a personal web application that aggregates and searches data across multiple databases, providing a unified interface with authentication and subscription-based access.

## Tech Stack

### Frontend

- Next.js 14+ (App Router)
- Shadcn UI Components
- TypeScript
- Tailwind CSS
- NextAuth.js (for authentication)

### Backend

- .NET 9
- Entity Framework Core
- RESTful API
- Multiple database connections

### Database Recommendations

For your .accdb, .mdb, and .xlsx files, I recommend:

- **Primary Database**: PostgreSQL (AWS RDS Free tier)
  - Best for structured data with complex queries
  - Strong performance for search operations
  - Good compatibility with Entity Framework
  
- **Data Migration Strategy**:
  - Use a one-time ETL process to migrate from Access/Excel to PostgreSQL
  - Consider Npgsql.EntityFrameworkCore.PostgreSQL for the .NET ORM
  - For Excel files: Use EPPlus or ExcelDataReader libraries to extract data

- **Alternative**: SQL Server Express (if you prefer staying in Microsoft ecosystem)

### Infrastructure

- Frontend Deployment: Vercel
- Backend Deployment: AWS (EC2 or ECS)
- Database: PostgreSQL (AWS RDS Free tier)
- Caching: Redis (for frequently accessed data)

## Features

### Phase 1 (MVP)

- [x] Basic Next.js frontend with Shadcn UI
- [x] .NET 9 backend API
- [x] Multi-database integration
- [x] Basic data display
- [x] Responsive design
- [ ] Basic search functionality
- [ ] Data migration tools

### Phase 2 (Authentication)

- [ ] Google Sign-in
- [ ] Apple Sign-in
- [ ] User profile management
- [ ] Role-based access control

### Phase 3 (Payment Integration)

- [ ] Stripe integration
- [ ] Subscription plans:
  - 10 calls for €0.99
  - 200 calls for €9.99
- [ ] Usage tracking
- [ ] Payment history
- [ ] Admin dashboard

### Phase 4 (Advanced Features)

- [ ] Advanced search with filters and facets
- [ ] Saved searches
- [ ] Export functionality
- [ ] API rate limiting
- [ ] Notification system

## Project Structure

```
kerko/
├── frontend/                  # Next.js frontend application
│   ├── app/                  # App router pages
│   ├── components/           # Reusable components
│   │   ├── ui/              # Shadcn components
│   │   ├── search/          # Search-related components
│   │   └── auth/            # Authentication components
│   ├── lib/                 # Utility functions
│   │   ├── api/            # API client
│   │   └── auth/           # Auth helpers
│   ├── styles/              # Global styles
│   └── types/               # TypeScript type definitions
│
├── backend/                  # .NET 9 backend application
│   ├── src/
│   │   ├── Kerko.API/       # Main API project
│   │   ├── Kerko.Core/      # Core domain models
│   │   ├── Kerko.Infrastructure/  # Data access
│   │   ├── Kerko.Services/  # Business logic
│   │   └── Kerko.Shared/    # Shared DTOs and utilities
│   │
│   ├── tests/               # Unit and integration tests
│   │
│   └── tools/               # Migration and data tools
│
└── docs/                    # Documentation
    ├── api/                 # API documentation
    ├── database/            # Database schema
    └── deployment/          # Deployment guides
```

## Development Setup

1. Clone the repository
2. Install dependencies:

   ```bash
   # Frontend
   cd frontend
   npm install

   # Backend
   cd backend
   dotnet restore
   ```

3. Set up database:

   ```bash
   # Create PostgreSQL database
   # Run migrations
   cd backend/src/Kerko.API
   dotnet ef database update
   ```

4. Set up environment variables
5. Run development servers:

   ```bash
   # Frontend
   npm run dev

   # Backend
   dotnet run --project src/Kerko.API
   ```

## Data Migration Tools

For converting your existing data:

```bash
# Install data migration tools
dotnet tool install -g Kerko.Tools.DataMigration

# Run migration from Access to PostgreSQL
dotnet kerko-migrate --source access --file path/to/your.accdb --target postgres
```

## Deployment Strategy

### Frontend (Vercel)

1. Connect GitHub repository to Vercel
2. Configure environment variables
3. Deploy with automatic CI/CD

### Backend (AWS)

1. Set up EC2 instance or ECS cluster
2. Configure security groups and networking
3. Set up CI/CD pipeline with GitHub Actions
4. Deploy using Docker containers

### Database

1. Set up PostgreSQL RDS instance (free tier)
2. Configure VPC and security groups
3. Set up automated backups

## Environment Variables

### Frontend

```env
NEXT_PUBLIC_API_URL=
NEXTAUTH_URL=
NEXTAUTH_SECRET=
NEXT_PUBLIC_GOOGLE_CLIENT_ID=
NEXT_PUBLIC_APPLE_CLIENT_ID=
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=
```

### Backend

```env
ConnectionStrings__PostgreSQL=
JWT__Secret=
JWT__Issuer=
JWT__Audience=
Stripe__SecretKey=
CORS__AllowedOrigins=
```

## Performance Monitoring

- Application Insights for backend monitoring
- Vercel Analytics for frontend performance

## Personal Notes

- [ ] Research best migration approach for Access databases
- [ ] Decide on search implementation (EF vs Elasticsearch)
- [ ] Consider caching strategy for frequently accessed data
- [ ] Plan database schema design
