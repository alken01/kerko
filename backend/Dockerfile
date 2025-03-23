FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/Kerko.API/Kerko.API.csproj", "src/Kerko.API/"]
COPY ["src/Kerko.Core/Kerko.Core.csproj", "src/Kerko.Core/"]
COPY ["src/Kerko.Infrastructure/Kerko.Infrastructure.csproj", "src/Kerko.Infrastructure/"]
RUN dotnet restore "src/Kerko.API/Kerko.API.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "src/Kerko.API/Kerko.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "src/Kerko.API/Kerko.API.csproj" -c Release -o /app/publish

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY database/data.db ./data.db
ENTRYPOINT ["dotnet", "Kerko.API.dll"] 