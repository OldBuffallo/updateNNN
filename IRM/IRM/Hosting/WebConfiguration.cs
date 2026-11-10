using IRM.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using MudBlazor.Services;

namespace IRM.Hosting;

internal static class WebConfiguration
{
    public static void Configure(WebApplicationBuilder builder)
    {
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


        builder.Services.AddScoped<ICompanyService, CompanyApplicationService>();
        builder.Services.AddScoped<ICompanyProfileService, CompanyProfileApplicationService>();
        builder.Services.AddScoped<IEmployeeService, EmployeeApplicationService>();
        builder.Services.AddScoped<IStudentService, StudentApplicationService>();
        builder.Services.AddScoped<IForeignerRegistryService, FamilyVisitorApplicationService>();
        builder.Services.AddScoped<IResidenceService, ResidenceApplicationService>();
        builder.Services.AddScoped<IAccommodationService, AccommodationApplicationService>();
        builder.Services.AddScoped<IInspectionService, InspectionApplicationService>();
        builder.Services.AddScoped<ILegalDocumentService, LegalDocumentApplicationService>();
        builder.Services.AddScoped<IDashboardService, DashboardApplicationService>();
        builder.Services.AddScoped<ISearchService, SearchApplicationService>();
        builder.Services.AddScoped<IStatisticsService, StatisticsApplicationService>();
        builder.Services.AddScoped<IAuthService, AuthApplicationService>();
        builder.Services.AddScoped<IAuditService, AuditApplicationService>();
        builder.Services.AddScoped<ICatalogService, CatalogApplicationService>();
        builder.Services.AddScoped<IImportService, ImportApplicationService>();
        builder.Services.AddScoped<IExportService, ExportApplicationService>();
        builder.Services.AddScoped<IStatisticsExportService, StatisticsExportApplicationService>();
        builder.Services.AddScoped<IServiceAuthorizationGuard, ServiceAuthorizationGuard>();
    }
}
