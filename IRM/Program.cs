using System.Net;
using IRM.Components;
using IRM.Data;
using IRM.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var bootstrapOnly = args.Any(x => x.Equals("--bootstrap", StringComparison.OrdinalIgnoreCase));
var provisionDatabase = args.Any(x => x.Equals("--provision-database", StringComparison.OrdinalIgnoreCase));
var backupOnly = args.Any(x => x.Equals("--backup", StringComparison.OrdinalIgnoreCase));
var restoreOnly = args.Any(x => x.Equals("--restore", StringComparison.OrdinalIgnoreCase));
var selfTestOnly = args.Any(x => x.Equals("--self-test", StringComparison.OrdinalIgnoreCase));
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService(options => options.ServiceName = "IRM");
builder.Host.UseSystemd();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        // Fix: mặc định 32KB quá nhỏ cho import/export Excel qua SignalR
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB
        // Tránh ngắt kết nối khi xử lý import/export file lớn trên VPS chậm
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
        options.KeepAliveInterval = TimeSpan.FromSeconds(30);
    });
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddMudServices();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];
if (string.IsNullOrWhiteSpace(dataProtectionKeysPath))
    dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "data-protection-keys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("IRM");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "irm.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        // HTTP is accepted only on loopback. Remote requests are rejected below unless HTTPS is used.
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManageData", p => p.RequireRole(IrmRoles.Admin, IrmRoles.DataEditor));
    options.AddPolicy("Inspect", p => p.RequireRole(IrmRoles.Admin, IrmRoles.Inspector));
    options.AddPolicy("Report", p => p.RequireRole(IrmRoles.Admin, IrmRoles.Reporter, IrmRoles.Viewer));
    options.AddPolicy("AdminOnly", p => p.RequireRole(IrmRoles.Admin));
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Thiếu ConnectionStrings:DefaultConnection cho SQL Server.");

if (backupOnly)
{
    await DatabaseBackup.CreateAndRotateAsync(
        connectionString,
        builder.Configuration["Backup:Directory"] ?? throw new InvalidOperationException("Thiếu Backup:Directory."));
    return;
}

if (restoreOnly)
{
    await DatabaseBackup.RestoreAsync(
        builder.Configuration["Restore:AdminConnectionString"]
            ?? throw new InvalidOperationException("Thiếu Restore:AdminConnectionString."),
        builder.Configuration["Restore:ArchivePath"]
            ?? throw new InvalidOperationException("Thiếu Restore:ArchivePath."),
        builder.Configuration["Backup:Directory"]
            ?? throw new InvalidOperationException("Thiếu Backup:Directory."));
    return;
}

var adminConnectionString = builder.Configuration["Provisioning:AdminConnectionString"];
if (provisionDatabase)
{
    if (string.IsNullOrWhiteSpace(adminConnectionString))
        throw new InvalidOperationException("Thiếu Provisioning:AdminConnectionString.");
    await DatabaseProvisioner.PrepareAsync(
        adminConnectionString,
        builder.Configuration["Provisioning:AppPassword"],
        builder.Configuration["Provisioning:DbaPassword"]);
}

var efConnectionString = bootstrapOnly
    ? DatabaseProvisioner.ForIrmDatabase(adminConnectionString
        ?? throw new InvalidOperationException("Bootstrap yêu cầu Provisioning:AdminConnectionString."))
    : connectionString;

void ConfigureSqlServer(DbContextOptionsBuilder options) => options.UseSqlServer(efConnectionString, sql =>
{
    sql.CommandTimeout(60);
    sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
});
builder.Services.AddDbContextFactory<IrmDbContext>(ConfigureSqlServer);
builder.Services.AddDbContext<IrmDbContext>(ConfigureSqlServer);

builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<ImportService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<IForeignerRegistryService, FamilyVisitorService>();
builder.Services.AddScoped<FamilyVisitorService>(sp => (FamilyVisitorService)sp.GetRequiredService<IForeignerRegistryService>());
builder.Services.AddScoped<IResidenceService, ResidenceService>();
builder.Services.AddScoped<ICompanyProfileService, CompanyProfileService>();
builder.Services.AddScoped<IAccommodationService, AccommodationService>();
builder.Services.AddScoped<AccommodationService>(sp => (AccommodationService)sp.GetRequiredService<IAccommodationService>());
builder.Services.AddScoped<IInspectionService, InspectionService>();
builder.Services.AddScoped<InspectionService>(sp => (InspectionService)sp.GetRequiredService<IInspectionService>());
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<StatisticsService>(sp => (StatisticsService)sp.GetRequiredService<IStatisticsService>());
builder.Services.AddScoped<IStatisticsExportService, StatisticsExportService>();
builder.Services.AddScoped<ILegalDocumentService, LegalDocumentService>();
builder.Services.AddSingleton<IStorageCapacityGuard, StorageCapacityGuard>();
builder.Services.AddSingleton<IMalwareScanner>(_ => OperatingSystem.IsWindows()
    ? new WindowsDefenderMalwareScanner()
    : OperatingSystem.IsLinux()
        ? new ClamAvMalwareScanner()
        : throw new PlatformNotSupportedException("IRM chỉ hỗ trợ Windows x64 và Ubuntu 22.04 amd64."));
builder.Services.AddScoped<ILegacySyncService, LegacySyncService>();
builder.Services.AddScoped<ISchemaVersionService, SchemaVersionService>();
builder.Services.AddScoped<IServiceAuthorizationGuard, ServiceAuthorizationGuard>();
builder.Services.AddHostedService<LegacySyncWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IrmDbContext>();
    if (bootstrapOnly)
    {
        await db.Database.MigrateAsync();
        await DatabaseSeeder.SeedCatalogsIfEmptyAsync(
            db,
            builder.Configuration["Bootstrap:AdminUsername"],
            builder.Configuration["Bootstrap:AdminPassword"]);
        if (provisionDatabase)
            await DatabaseProvisioner.FinalizeAsync(adminConnectionString!);
    }
    await scope.ServiceProvider.GetRequiredService<ISchemaVersionService>().ValidateAsync();
}

if (selfTestOnly)
{
    await ReleaseSelfTest.RunAsync(app.Services, builder.Configuration);
    return;
}

if (bootstrapOnly)
    return;

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

var requireHttpsForRemoteClients = builder.Configuration.GetValue("Security:RequireHttpsForRemoteClients", true);
if (requireHttpsForRemoteClients)
{
    app.Use(async (context, next) =>
    {
        var remoteAddress = context.Connection.RemoteIpAddress;
        if (!context.Request.IsHttps && remoteAddress is not null && !IPAddress.IsLoopback(remoteAddress))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Kết nối từ máy khác phải sử dụng HTTPS.");
            return;
        }
        await next();
    });
}

app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health/live", () => Results.Ok(new { status = "ok", version = "1.0.1" })).AllowAnonymous();
app.MapGet("/health/ready", async (IDbContextFactory<IrmDbContext> factory, CancellationToken cancellationToken) =>
{
    await using var db = await factory.CreateDbContextAsync(cancellationToken);
    return await db.Database.CanConnectAsync(cancellationToken)
        ? Results.Ok(new
        {
            status = "ready",
            database = "sqlserver",
            schema = SchemaVersionService.RequiredVersion,
            schemaChecksum = SchemaVersionService.RequiredChecksum
        })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
}).AllowAnonymous();

app.MapPost("/auth/login", async (HttpContext context, AuthService authService) =>
{
    var form = await context.Request.ReadFormAsync(context.RequestAborted);
    var principal = await authService.AuthenticateWebAsync(form["username"].ToString(), form["password"].ToString(),
        context.Connection.RemoteIpAddress?.ToString(), context.RequestAborted);
    if (principal is null) return Results.LocalRedirect("/login?loginError=1");
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
        new AuthenticationProperties { IsPersistent = false });
    return Results.LocalRedirect("/");
}).AllowAnonymous();

app.MapPost("/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.LocalRedirect("/");
}).RequireAuthorization();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();

public partial class Program;
