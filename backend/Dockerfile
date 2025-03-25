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

# Install curl for healthcheck and sqlite3 for database operations
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    sqlite3 \
    && rm -rf /var/lib/apt/lists/*

# Add healthcheck
HEALTHCHECK --interval=30s --timeout=3s \
    CMD curl -f http://localhost:8080/health || exit 1

# Copy the published artifacts
COPY --from=build /app/publish .

# Create necessary directories
RUN mkdir -p /app/data /app/Kerko/certificates

# Create a non-root user
RUN adduser --disabled-password --gecos "" appuser && \
    chown -R appuser /app /app/data /app/Kerko/certificates && \
    chmod 700 /app/Kerko/certificates && \
    chmod 755 /app/data
USER appuser

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV TZ=UTC

# Configure logging
ENV ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS=true
ENV ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT=Information
ENV ASPNETCORE_LOGGING__LOGLEVEL__MICROSOFT=Warning
ENV ASPNETCORE_LOGGING__LOGLEVEL__MICROSOFT.HOSTING.LIFETIME=Information

# Expose the ports the app will run on
EXPOSE 8080
EXPOSE 8443

# Volumes for persistent data and certificates
VOLUME ["/app/data", "/app/Kerko/certificates"]

# Set the entry point
ENTRYPOINT ["dotnet", "Kerko.dll"]