using IRM.Data;
using IRM.Data.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    public IrmDbContext Context { get; private set; } = null!;

    public async Task InitializeAsync(bool seedBaseline = true)
    {
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<IrmDbContext>().UseSqlite(_connection).Options;
        Context = new IrmDbContext(options);
        await Context.Database.EnsureCreatedAsync();

        if (seedBaseline)
        {
            await SeedMinimalLookupsAsync();
        }
    }

    public async Task SeedMinimalLookupsAsync()
    {
        if (!await Context.Accounts.AnyAsync())
        {
            Context.Accounts.Add(new Account { IDUser = 1, Username = "admin_default", Name = "Admin", Password = "password", Permission = AccountPermission.Admin });
            await Context.SaveChangesAsync();
        }

        if (!await Context.Fields.AnyAsync())
        {
            Context.Fields.Add(new Field { IDField = 1, FieldName = "Công nghệ thông tin", Description = "CNTT" });
            await Context.SaveChangesAsync();
        }

        if (!await Context.CareerGroups.AnyAsync())
        {
            Context.CareerGroups.Add(new CareerGroup { IDCG = 1, CareerGroupName = "Kỹ thuật" });
            await Context.SaveChangesAsync();
        }

        if (!await Context.Careers.AnyAsync())
        {
            Context.Careers.Add(new Career { IDCareer = 1, CareerName = "Kỹ sư", IDCG = 1 });
            await Context.SaveChangesAsync();
        }

        if (!await Context.Nationality.AnyAsync())
        {
            Context.Nationality.AddRange(
                new NationalityEntity { NationalityCode = "USA", NationalityName = "Hoa Kỳ" },
                new NationalityEntity { NationalityCode = "GBR", NationalityName = "Vương quốc Anh" },
                new NationalityEntity { NationalityCode = "CHN", NationalityName = "Trung Quốc" },
                new NationalityEntity { NationalityCode = "CN", NationalityName = "Trung Quốc" },
                new NationalityEntity { NationalityCode = "KR", NationalityName = "Hàn Quốc" },
                new NationalityEntity { NationalityCode = "KOR", NationalityName = "Hàn Quốc" },
                new NationalityEntity { NationalityCode = "JP", NationalityName = "Nhật Bản" },
                new NationalityEntity { NationalityCode = "JPN", NationalityName = "Nhật Bản" },
                new NationalityEntity { NationalityCode = "VN", NationalityName = "Việt Nam" },
                new NationalityEntity { NationalityCode = "LA", NationalityName = "Lào" },
                new NationalityEntity { NationalityCode = "KH", NationalityName = "Campuchia" },
                new NationalityEntity { NationalityCode = "TH", NationalityName = "Thái Lan" },
                new NationalityEntity { NationalityCode = "ID", NationalityName = "Indonesia" },
                new NationalityEntity { NationalityCode = "IN", NationalityName = "Ấn Độ" },
                new NationalityEntity { NationalityCode = "MY", NationalityName = "Malaysia" },
                new NationalityEntity { NationalityCode = "PH", NationalityName = "Philippines" },
                new NationalityEntity { NationalityCode = "TW", NationalityName = "Đài Loan" }
            );
            await Context.SaveChangesAsync();
        }

        if (!await Context.Companies.AnyAsync())
        {
            Context.Companies.Add(new Company { IDCompany = 1, CompanyName = "Default Test Company", IDField = 1, TrackerID = 1 });
            await Context.SaveChangesAsync();
        }

        if (!await Context.AdministrativeUnits.AnyAsync())
        {
            Context.AdministrativeUnits.Add(new AdministrativeUnit
            {
                Id = 1,
                Code = "QN-W-DEFAULT",
                Name = "Phường Hồng Gai",
                TypeCode = AdministrativeUnitTypeCodes.Ward,
                ValidFrom = new DateTime(2020, 1, 1)
            });
            await Context.SaveChangesAsync();
        }

        if (!await Context.EconomicZones.AnyAsync())
        {
            Context.EconomicZones.Add(new EconomicZone
            {
                Id = 1,
                Code = "EZ-DEFAULT",
                Name = "KCN Đông Mai",
                TypeCode = "IZ",
                ValidFrom = new DateTime(2020, 1, 1)
            });
            await Context.SaveChangesAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Context is not null) await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
