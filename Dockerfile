# === Build stage ===
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY IRM/IRM.csproj IRM/
RUN dotnet restore IRM/IRM.csproj

# Copy all and build
COPY IRM/ IRM/
RUN dotnet publish IRM/IRM.csproj -c Release -o /app/publish

# === Runtime stage ===
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create data directory for SQLite
RUN mkdir -p /app/data

COPY --from=build /app/publish .

# Use SQLite for cloud deployment (no SQL Server needed)
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:10000
ENV ConnectionStrings__DefaultConnection=""

EXPOSE 10000

ENTRYPOINT ["dotnet", "IRM.dll"]
