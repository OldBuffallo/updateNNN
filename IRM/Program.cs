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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddMudServices();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    Directory.CreateDirectory(dataProtectionKeysPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
        .SetApplicationName("IRM-v0.1.0");
}

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "irm.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
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

var provider = builder.Configuration["Database:Provider"] ?? "SqlServer";
var isSqlite = provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (isSqlite)
{
    connectionString = builder.Configuration.GetConnectionString("Sqlite")
        ?? $"Data Source={Path.Combine(AppContext.BaseDirectory, "IRM-v0.1.0-demo.db")}";
    builder.Services.AddDbContextFactory<IrmDbContext>(options => options.UseSqlite(connectionString));
    builder.Services.AddDbContext<IrmDbContext>(options => options.UseSqlite(connectionString));
}
else
{
    if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("Thiếu ConnectionStrings:DefaultConnection.");
    builder.Services.AddDbContextFactory<IrmDbContext>(options => options.UseSqlServer(connectionString, sql => sql.CommandTimeout(60)));
    builder.Services.AddDbContext<IrmDbContext>(options => options.UseSqlServer(connectionString, sql => sql.CommandTimeout(60)));
}

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
builder.Services.AddSingleton<IMalwareScanner, WindowsDefenderMalwareScanner>();
builder.Services.AddScoped<ILegacySyncService, LegacySyncService>();
builder.Services.AddScoped<ISchemaVersionService, SchemaVersionService>();
builder.Services.AddScoped<IServiceAuthorizationGuard, ServiceAuthorizationGuard>();
builder.Services.AddHostedService<LegacySyncWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IrmDbContext>();
    if (isSqlite)
    {
        await db.Database.EnsureCreatedAsync();
        await DatabaseSeeder.SeedAsync(db);
        await DatabaseSeeder.SeedV010Async(db);
        await scope.ServiceProvider.GetRequiredService<IForeignerRegistryService>().BackfillLegacyAsync();
    }
    else
    {
        await scope.ServiceProvider.GetRequiredService<ISchemaVersionService>().ValidateAsync();
    }
}

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error", createScopeForErrors: true);
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

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
