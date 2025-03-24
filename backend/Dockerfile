# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Kerko/Kerko.csproj", "Kerko/"]
COPY ["Kerko.sln", "./"]
RUN dotnet restore "Kerko/Kerko.csproj"

# Copy the rest of the code
COPY . .

# Build and publish
RUN dotnet build "Kerko/Kerko.csproj" -c Release -o /app/build
RUN dotnet publish "Kerko/Kerko.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

# Add healthcheck
HEALTHCHECK --interval=30s --timeout=3s \
    CMD curl -f http://localhost:80/health || exit 1

# Copy the published artifacts
COPY --from=build /app/publish .

# Create data directory for SQLite
RUN mkdir -p /app/data

# Copy the SQLite database file
COPY --from=build /src/Kerko/kerko.db /app/data/

# Create a non-root user
RUN adduser --disabled-password --gecos "" appuser && \
    chown -R appuser /app /app/data
USER appuser

# Set environment variables for production
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV TZ=UTC
# Point connection string to data directory
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/kerko.db;Cache=Shared"

# Configure logging
ENV ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS=true
ENV ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT=Information
ENV ASPNETCORE_LOGGING__LOGLEVEL__MICROSOFT=Warning
ENV ASPNETCORE_LOGGING__LOGLEVEL__MICROSOFT.HOSTING.LIFETIME=Information

# Expose the port the app will run on
EXPOSE 80

# Volume for persistent data
VOLUME ["/app/data"]

# Set the entry point
ENTRYPOINT ["dotnet", "Kerko.dll"]