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

# Persistent demo data and private legal-document storage.
RUN mkdir -p /app/data/private-files /app/data/dataprotection-keys && chown -R app:app /app/data

COPY --from=build /app/publish .

# VPS demo profile. Production SQL Server must be configured explicitly and
# must pass the backup/restore + schema migration gate before deployment.
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5050
ENV Database__Provider=Sqlite
ENV ConnectionStrings__Sqlite="Data Source=/app/data/IRM-v0.1.0-demo.db"
ENV FileStorage__Root=/app/data/private-files
ENV DataProtection__KeysPath=/app/data/dataprotection-keys
# Disable file watchers to avoid inotify limit on Render
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

VOLUME ["/app/data"]
EXPOSE 5050

USER app

ENTRYPOINT ["dotnet", "IRM.dll"]
