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

# Persistent data and private legal-document storage.
RUN mkdir -p /app/data/private-files /app/data/dataprotection-keys && chown -R app:app /app/data

COPY --from=build /app/publish .

# VPS production profile with SQL Server
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5050
ENV Database__Provider=SqlServer
ENV FileStorage__Root=/app/data/private-files
ENV DataProtection__KeysPath=/app/data/dataprotection-keys
# Disable file watchers to avoid inotify limit on VPS
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

VOLUME ["/app/data"]
EXPOSE 5050

USER app

ENTRYPOINT ["dotnet", "IRM.dll"]
