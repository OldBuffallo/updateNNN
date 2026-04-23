using IRM.Components;
using IRM.Data;
using IRM.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// === Database ===
// Thử SQL Server trước, nếu không kết nối được thì dùng SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useSqlite = string.IsNullOrEmpty(connectionString);

// Kiểm tra SQL Server có khả dụng không
if (!string.IsNullOrEmpty(connectionString) && !connectionString.Contains("Sqlite"))
{
    try
    {
        using var testConn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
        testConn.Open();
        testConn.Close();
        useSqlite = false;
    }
    catch
    {
        Console.WriteLine("⚠️ SQL Server không khả dụng, chuyển sang SQLite");
        useSqlite = true;
    }
}

if (useSqlite)
{
    var sqlitePath = Path.Combine(AppContext.BaseDirectory, "ReportManagerDB.db");
    Console.WriteLine($"📦 Sử dụng SQLite: {sqlitePath}");

    builder.Services.AddDbContextFactory<IrmDbContext>(options =>
        options.UseSqlite($"Data Source={sqlitePath}"));
    builder.Services.AddDbContext<IrmDbContext>(options =>
        options.UseSqlite($"Data Source={sqlitePath}"));
}
else
{
    Console.WriteLine("📦 Sử dụng SQL Server");

    builder.Services.AddDbContextFactory<IrmDbContext>(options =>
        options.UseSqlServer(connectionString,
            sqlOptions => sqlOptions.CommandTimeout(60)));
    builder.Services.AddDbContext<IrmDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// === Services ===
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<ImportService>();
builder.Services.AddScoped<ExportService>();

var app = builder.Build();

// === Auto-create database + seed data ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IrmDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DatabaseSeeder.SeedAsync(db);
    Console.WriteLine("✅ Database sẵn sàng");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
