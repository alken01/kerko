# Kërko

A search platform to access various Albanian public record databases.
Built with **.NET 10** and **Next.js 15**.

**Database Size:** 1.1GB+ of indexed and searchable records

**Data Sources:** Includes leaked datasets such as those from the [2021 data breach](https://www.tiranatimes.com/massive-data-breach-exposes-wage-and-personal-info-of-more-than-637000-residents/)
> **Note:** No data is included in this repository.

## Local Development

### Prerequisites
- .NET 10 SDK
- Node.js 20+

### Backend
```bash
cd backend/Kerko
dotnet restore
dotnet run
```

### Frontend
```bash
cd frontend
npm install
npm run dev
```
