# IRM repository guide

## Active source

- `IRM/`: ASP.NET Core 8 Blazor Server application. This is the production code.
- `IRM.Tests/`: xUnit tests for `IRM/`.
- `deploy/`: deployment scripts, SQL migrations, installer definitions, and vendor-media placeholders.
- `docs/`: product, release, and feature documentation.

Treat `immigration-reportmanager-master/` (the nested WPF solution), `TestExcelGen/`, and `mockup-demo/` as legacy/reference code. Do not inspect or change them unless the task explicitly concerns the WPF client, Excel experiment, or mockup.

Ignore generated content: `bin/`, `obj/`, `build-output/`, `deploy-package/`, `.local-run/`, versioned installer folders/archives, runtime databases, logs, and data-protection keys. Do not search or summarize these directories.

## Where to look

- Startup, dependency injection, middleware: `IRM/Program.cs`
- UI and routes: `IRM/Components/`
- Business logic: `IRM/Services/`
- EF Core context, models, migrations: `IRM/Data/`
- Static assets: `IRM/wwwroot/`
- Feature tests: `IRM.Tests/*FeatureTddTests.cs`
- Authorization/security tests: `IRM.Tests/AuthorizationTests.cs`, `IRM.Tests/SecurityAndMigrationTests.cs`
- Release/build automation: `deploy/`, `Dockerfile`, `docker-compose.openship.yml`

## Commands

```powershell
dotnet build IRM/IRM.csproj -c Release
dotnet test IRM.Tests/IRM.Tests.csproj -c Release
dotnet run --project IRM/IRM.csproj
```

The application uses SQL Server in production; SQLite is test-only. Never commit credentials or generated installer media. Keep changes scoped, preserve the additive legacy-schema compatibility, and add or update focused tests for behavior changes.
