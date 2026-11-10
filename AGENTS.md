# IRM repository guide

## Active source

- `IRM.sln`: current web solution; includes Web, Core, Application, Infrastructure and tests only.
- `IRM/`: ASP.NET Core 8 Blazor Server host. Keep its path, assembly name and `IRM.exe` stable for installers.
- `IRM.Core/`: shared entities, enums and pure compatibility rules; no EF, ASP.NET or Excel dependencies.
- `IRM.Application/`: feature service interfaces, detached DTOs, command validation and materialized persistence/file/security ports.
- `IRM.Infrastructure/`: EF Core context/configurations/migrations, SQL stores, Excel, private-file storage, antivirus, legacy sync and maintenance.
- `IRM.Tests/`: xUnit tests and pre-refactor schema/migration/JSON compatibility baselines.
- `deploy/`: deployment scripts, SQL scripts, installer definitions and vendor-media placeholders.
- `docs/ARCHITECTURE.md`: project boundaries, module map, compatibility contracts and verification commands.

Treat `immigration-reportmanager-master/` (the nested WPF solution), `TestExcelGen/`, and `mockup-demo/` as legacy/reference code. Do not inspect or change them unless the task explicitly concerns the WPF client, Excel experiment, or mockup.

Ignore generated content: `bin/`, `obj/`, `build-output/`, `deploy-package/`, `.local-run/`, versioned installer folders/archives, runtime databases, logs, and data-protection keys. Do not search or summarize these directories.

## Boundaries

- Application references Core only. Infrastructure references Application and Core. Web composes Application and Infrastructure in `Hosting/` and `Program.cs`.
- UI lives in `IRM/Features/` and injects Application interfaces; bind forms to Application DTOs, never tracked entities or DbContext.
- Shared UI lives in `IRM/Components/Shared/`; Import/Admin state coordination lives in their feature code-behind files.
- Entity namespaces and SQL table/column names intentionally retain legacy naming for WPF, EF migration and stored-JSON compatibility.
- SQL context and historical migrations are in the same Infrastructure assembly. Do not generate a migration merely to move code.
- Store ports return materialized results, never IQueryable, DbSet or DbContext. Keep import/rollback transactions atomic.
- Preserve authorization, password compatibility, enum values, soft-delete conventions and legacy sync behavior.

## Commands

```powershell
dotnet restore IRM.sln
dotnet build IRM.sln -c Release
dotnet test IRM.Tests/IRM.Tests.csproj -c Release
dotnet run --project IRM/IRM.csproj
dotnet tool restore
dotnet ef migrations list --project IRM.Infrastructure --startup-project IRM --no-connect
```

Production uses SQL Server. SQLite is test-only. SQL integration tests use `IRM_TEST_SQLSERVER` and create/drop only generated `IRM_RefactorTest_*` databases; never point tests or maintenance at production. Never commit credentials or generated installer media. Keep dependency versions and database schema unchanged during architectural refactoring.
