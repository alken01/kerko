# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project files
COPY ["Kerko/Kerko.csproj", "Kerko/"]
RUN dotnet restore "Kerko/Kerko.csproj"

# Copy the rest of the code
COPY . .

# Build and publish
WORKDIR "/src/Kerko"
RUN dotnet build "Kerko.csproj" -c Release -o /app/build
RUN dotnet publish "Kerko.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port the app runs on
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080

# Run the app
ENTRYPOINT ["dotnet", "Kerko.dll"] 