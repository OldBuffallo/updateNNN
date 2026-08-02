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
builder.Services.AddScoped<StudentService>();

var app = builder.Build();

// === Auto-create database + seed data ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IrmDbContext>();

    if (useSqlite)
    {
        // SQLite (demo/fallback): tạo toàn bộ schema + seed data mẫu
        await db.Database.EnsureCreatedAsync();
        await DatabaseSeeder.SeedAsync(db);
        Console.WriteLine("✅ Database SQLite sẵn sàng");
    }
    else
    {
        // SQL Server production: DB đã có sẵn dữ liệu cũ
        // Chỉ tạo các bảng MỚI nếu chưa có (AuditLogs, ImportHistories, Students...)
        // KHÔNG gọi EnsureCreatedAsync() vì sẽ không tạo bảng thiếu trên DB đã tồn tại
        try
        {
            // Kiểm tra DB có dữ liệu cũ không
            var hasData = await db.Accounts.AnyAsync();
            if (hasData)
            {
                Console.WriteLine("📦 SQL Server: Phát hiện dữ liệu cũ — bỏ qua seed");
                // Tạo các bảng mới nếu thiếu (an toàn, dùng raw SQL)
                await EnsureNewTablesAsync(db);
            }
            else
            {
                // DB trống (mới restore hoặc mới tạo)
                await db.Database.EnsureCreatedAsync();
                await DatabaseSeeder.SeedAsync(db);
                Console.WriteLine("✅ Database SQL Server mới — đã seed data");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Lỗi kiểm tra DB: {ex.Message}");
            // Fallback: thử tạo DB
            await db.Database.EnsureCreatedAsync();
            await DatabaseSeeder.SeedAsync(db);
        }
        Console.WriteLine("✅ Database SQL Server sẵn sàng");
    }
}

// Tạo các bảng mới trên database SQL Server cũ
static async Task EnsureNewTablesAsync(IrmDbContext db)
{
    var tablesToCheck = new Dictionary<string, string>
    {
        ["AuditLogs"] = @"
            CREATE TABLE [dbo].[AuditLogs] (
                [Id]          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [Action]      NVARCHAR(50)   NOT NULL,
                [EntityType]  NVARCHAR(100)  NOT NULL,
                [EntityId]    INT            NULL,
                [Description] NVARCHAR(MAX)  NULL,
                [Username]    NVARCHAR(100)  NULL,
                [Timestamp]   DATETIME       NOT NULL DEFAULT GETDATE(),
                [IpAddress]   NVARCHAR(50)   NULL
            )",
        ["ImportHistories"] = @"
            CREATE TABLE [dbo].[ImportHistories] (
                [Id]           BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [SessionId]    NVARCHAR(50)   NOT NULL,
                [FileName]     NVARCHAR(500)  NOT NULL,
                [CompanyId]    INT            NOT NULL DEFAULT 0,
                [CompanyName]  NVARCHAR(500)  NULL,
                [TotalRows]    INT            NOT NULL DEFAULT 0,
                [AddedRows]    INT            NOT NULL DEFAULT 0,
                [UpdatedRows]  INT            NOT NULL DEFAULT 0,
                [ErrorRows]    INT            NOT NULL DEFAULT 0,
                [Status]       NVARCHAR(20)   NOT NULL DEFAULT 'committed',
                [Username]     NVARCHAR(100)  NULL,
                [ImportDate]   DATETIME       NOT NULL DEFAULT GETDATE(),
                [ErrorDetails] NVARCHAR(MAX)  NULL
            )",
        ["ImportBackups"] = @"
            CREATE TABLE [dbo].[ImportBackups] (
                [Id]              BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [ImportSessionId] NVARCHAR(50)   NOT NULL,
                [ActionType]      NVARCHAR(20)   NOT NULL,
                [EmployeeId]      INT            NOT NULL,
                [OldData]         NVARCHAR(MAX)  NULL,
                [CreatedAt]       DATETIME       NOT NULL DEFAULT GETDATE()
            )",
        ["ColumnMappingTemplates"] = @"
            CREATE TABLE [dbo].[ColumnMappingTemplates] (
                [Id]           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [TemplateName] NVARCHAR(200)  NOT NULL,
                [CompanyId]    INT            NULL,
                [MappingJson]  NVARCHAR(MAX)  NOT NULL,
                [CreatedBy]    NVARCHAR(100)  NULL,
                [CreatedAt]    DATETIME       NOT NULL DEFAULT GETDATE(),
                [UpdatedAt]    DATETIME       NULL
            )",
        ["Students"] = @"
            CREATE TABLE [dbo].[Students] (
                [IDStudent]           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [FullName]            NVARCHAR(200)  NOT NULL,
                [Gender]              INT            NOT NULL DEFAULT 1,
                [Birthday]            DATETIME       NULL,
                [Nationality]         NVARCHAR(10)   NULL,
                [Passport]            NVARCHAR(50)   NULL,
                [Address]             NVARCHAR(500)  NULL,
                [SchoolName]          NVARCHAR(500)  NULL,
                [Major]               NVARCHAR(200)  NULL,
                [StudentCode]         NVARCHAR(50)   NULL,
                [EducationLevel]      INT            NOT NULL DEFAULT 0,
                [EnrollmentDate]      DATETIME       NULL,
                [ExpectedGraduation]  DATETIME       NULL,
                [VisaNumber]          NVARCHAR(100)  NULL,
                [VisaExpiry]          DATETIME       NULL,
                [TemporaryStay]       DATETIME       NULL,
                [ScholarshipType]     INT            NOT NULL DEFAULT 0,
                [Status]              INT            NOT NULL DEFAULT 0,
                [Note]                NVARCHAR(MAX)  NULL,
                [IDUser]              INT            NOT NULL DEFAULT 1,
                [DateCreated]         DATETIME       NULL DEFAULT GETDATE(),
                [Hidden_flag]         INT            NOT NULL DEFAULT 0
            )",
        ["ArchivedEmployees"] = @"
            CREATE TABLE [dbo].[ArchivedEmployees] (
                [Id]                          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [OriginalId]                  INT            NOT NULL,
                [StaffName]                   NVARCHAR(MAX)  NOT NULL,
                [Gender]                      INT            NOT NULL DEFAULT 1,
                [Birthday]                    DATETIME       NULL,
                [Nationality]                 NVARCHAR(MAX)  NULL,
                [Passport]                    NVARCHAR(MAX)  NULL,
                [Address]                     NVARCHAR(MAX)  NULL,
                [IDCareer]                    INT            NULL,
                [WorkPermit]                  INT            NOT NULL DEFAULT 0,
                [WorkPermitNumber]            NVARCHAR(MAX)  NULL,
                [VisaNumber]                  NVARCHAR(MAX)  NULL,
                [TemporaryStay]               DATETIME       NULL,
                [Note]                        NVARCHAR(MAX)  NULL,
                [SettlementResults]           INT            NOT NULL DEFAULT 0,
                [SettlementResultsString]     NVARCHAR(MAX)  NULL,
                [IDUser]                      INT            NOT NULL DEFAULT 1,
                [IDCompany]                   INT            NOT NULL DEFAULT 0,
                [DateCreated]                 DATETIME       NULL,
                [CardCreationDate]            DATETIME       NULL,
                [WorkingStatus]               INT            NOT NULL DEFAULT 0,
                [DateOfJoin]                  DATETIME       NULL,
                [DateOfLeave]                 DATETIME       NULL,
                [FamilyVisit]                 INT            NOT NULL DEFAULT 0,
                [FamilyVisitRelativeName]     NVARCHAR(200)  NULL,
                [FamilyVisitRelationship]     NVARCHAR(100)  NULL,
                [FamilyVisitRelativeIdCard]   NVARCHAR(50)   NULL,
                [FamilyVisitStartDate]        DATETIME       NULL,
                [FamilyVisitEndDate]          DATETIME       NULL,
                [FamilyVisitNote]             NVARCHAR(500)  NULL,
                [CompanyName]                 NVARCHAR(MAX)  NULL,
                [CareerName]                  NVARCHAR(MAX)  NULL,
                [ArchiveReason]               NVARCHAR(50)   NOT NULL DEFAULT '',
                [ArchivedBy]                  NVARCHAR(100)  NULL,
                [ArchivedAt]                  DATETIME       NOT NULL DEFAULT GETDATE()
            )"
    };

    foreach (var (table, createSql) in tablesToCheck)
    {
        try
        {
            var exists = await db.Database.ExecuteSqlRawAsync(
                $"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{table}') BEGIN {createSql} END");
            Console.WriteLine($"  ✅ Bảng {table}: OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️ Bảng {table}: {ex.Message}");
        }
    }

    try
    {
        await db.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Employees') AND name = 'FamilyVisit')
            BEGIN
                ALTER TABLE [dbo].[Employees] ADD
                    [FamilyVisit]               INT           NOT NULL DEFAULT 0,
                    [FamilyVisitRelativeName]   NVARCHAR(200) NULL,
                    [FamilyVisitRelationship]   NVARCHAR(100) NULL,
                    [FamilyVisitRelativeIdCard] NVARCHAR(50)  NULL,
                    [FamilyVisitStartDate]      DATETIME      NULL,
                    [FamilyVisitEndDate]        DATETIME      NULL,
                    [FamilyVisitNote]           NVARCHAR(500) NULL
            END");
        Console.WriteLine("  ✅ Cột FamilyVisit: OK");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ⚠️ Cột FamilyVisit: {ex.Message}");
    }

    // Thêm cột RegistrationProfileIndex vào Companies nếu chưa có
    try
    {
        await db.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Companies') AND name = 'RegistrationProfileIndex')
            BEGIN
                ALTER TABLE [dbo].[Companies] ADD [RegistrationProfileIndex] INT NOT NULL DEFAULT 0
            END");
        Console.WriteLine("  ✅ Cột RegistrationProfileIndex: OK");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ⚠️ Cột RegistrationProfileIndex: {ex.Message}");
    }
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
