using System.Net;
using IRM.Components;
using IRM.Hosting;
using IRM.Infrastructure;
using IRM.Infrastructure.Maintenance;
using IRM.Services;

var exportIdx = Array.FindIndex(args, a => a.Equals("--export-delivery-workbooks", StringComparison.OrdinalIgnoreCase));
if (exportIdx >= 0 && exportIdx + 1 < args.Length)
{
    IRM.Infrastructure.Excel.DeliveryWorkbooks.WriteAll(args[exportIdx + 1]);
    return;
}

var bootstrapOnly = args.Any(x => x.Equals("--bootstrap", StringComparison.OrdinalIgnoreCase));
var provisionDatabase = args.Any(x => x.Equals("--provision-database", StringComparison.OrdinalIgnoreCase));
var backupOnly = args.Any(x => x.Equals("--backup", StringComparison.OrdinalIgnoreCase));
var restoreOnly = args.Any(x => x.Equals("--restore", StringComparison.OrdinalIgnoreCase));
var selfTestOnly = args.Any(x => x.Equals("--self-test", StringComparison.OrdinalIgnoreCase));
var builder = WebApplication.CreateBuilder(args);

WebConfiguration.Configure(builder);
var command = await MaintenanceCommands.ConfigureAsync(builder.Configuration, bootstrapOnly, provisionDatabase, backupOnly, restoreOnly);
if (command.Handled) return;
builder.Services.AddIrmInfrastructure(
    command.ConnectionString,
    builder.Configuration.GetValue("LegacySync:Enabled", false),
    builder.Configuration.GetValue("Security:RequireDefenderScanner", false));
var app = builder.Build();
await DatabaseStartup.InitializeAsync(app.Services, builder.Configuration, bootstrapOnly, provisionDatabase,
    builder.Configuration["Provisioning:AdminConnectionString"]);
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

app.MapIrmEndpoints();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();

public partial class Program;
