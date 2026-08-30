using IRM.Data.Models;
using IRM.Services;

namespace IRM.Tests;

public sealed class CompanyFeatureTddTests
{
    [Fact]
    public async Task CompanyService_GetByIdAsync_IncludesAllNavigations()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);
        var service = new CompanyService(db.Context, TestSecurityContext.CreateAllowAllGuard());

        var field = new Field { FieldName = "Công nghiệp chế tạo" };
        db.Context.Fields.Add(field);
        await db.Context.SaveChangesAsync();

        var company = new Company
        {
            CompanyName = "Samsung Electronics",
            IDField = field.IDField,
            Address = "KCN Yên Phong",
            LegalRepresentative = "Lee Jae-yong",
            TrackerID = 1
        };
        db.Context.Companies.Add(company);
        await db.Context.SaveChangesAsync();

        db.Context.Employees.Add(new Employee { StaffName = "Emp 1", IDCompany = company.IDCompany, IDCareer = 1, Nationality = "USA", Hidden_flag = 0, WorkingStatus = 0 });
        db.Context.Employees.Add(new Employee { StaffName = "Emp 2 (Hidden)", IDCompany = company.IDCompany, IDCareer = 1, Nationality = "USA", Hidden_flag = 1, WorkingStatus = 0 });
        db.Context.PhoneNumbers.Add(new PhoneNumber { Phone = "0241234567", IDCompany = company.IDCompany, Delete_flag = 0 });
        db.Context.Emails.Add(new Email { Mail = "info@samsung.vn", IDCompany = company.IDCompany, Delete_flag = 0 });
        await db.Context.SaveChangesAsync();

        var detail = await service.GetByIdAsync(company.IDCompany);
        Assert.NotNull(detail);
        Assert.Equal("Samsung Electronics", detail!.CompanyName);
        Assert.Equal("Công nghiệp chế tạo", detail.Field?.FieldName);
        Assert.Equal(2, detail.Employees.Count);
        Assert.Single(detail.PhoneNumbers);
        Assert.Single(detail.Emails);

        var empCount = await service.GetEmployeeCountAsync(company.IDCompany);
        Assert.Equal(1, empCount);
    }

    [Fact]
    public async Task CompanyProfileService_GetAsync_AggregatesAllTabs()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);
        var audit = new AuditService(db.Context);
        var foreignerService = new FamilyVisitorService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());
        var profileService = new CompanyProfileService(db.Context, foreignerService, audit, TestSecurityContext.CreateAllowAllGuard());

        var company = new Company { CompanyName = "Foxconn Vietnam", IDField = 1, TrackerID = 1 };
        db.Context.Companies.Add(company);
        await db.Context.SaveChangesAsync();

        var profile = new CompanyProfile
        {
            CompanyId = company.IDCompany,
            TaxCode = "0101234567",
            BusinessRegistrationNumber = "BR-001",
            OwnershipTypeCode = "FDI"
        };
        await profileService.SaveProfileAsync(profile);

        var zone = new EconomicZone { Code = "KCN-01", Name = "KCN Đông Mai", TypeCode = "IZ", ValidFrom = new DateTime(2020, 1, 1) };
        db.Context.EconomicZones.Add(zone);
        await db.Context.SaveChangesAsync();

        var site = new CompanySite
        {
            CompanyId = company.IDCompany,
            AdministrativeUnitId = 1,
            Name = "Nhà máy 1",
            AddressLine = "Lô A1, KCN Đông Mai",
            ValidFrom = new DateTime(2025, 1, 1)
        };
        await profileService.SaveSiteAsync(site, [zone.Id]);

        var rep = new CompanyRepresentative
        {
            CompanyId = company.IDCompany,
            FullName = "Terry Gou",
            Title = "Chủ tịch",
            ValidFrom = new DateTime(2025, 1, 1)
        };
        await profileService.SaveRepresentativeAsync(rep);

        var aggregate = await profileService.GetAsync(company.IDCompany);
        Assert.NotNull(aggregate);
        Assert.Equal("Foxconn Vietnam", aggregate!.Company?.CompanyName);
        Assert.Equal("0101234567", aggregate.Profile?.TaxCode);
        Assert.Single(aggregate.Sites);
        Assert.Equal("Nhà máy 1", aggregate.Sites.Single().Name);
        Assert.Single(aggregate.Representatives);
        Assert.Equal("Terry Gou", aggregate.Representatives.Single().FullName);
    }

    [Fact]
    public async Task SaveSiteAsync_AtomicReplaceZoneMemberships()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);
        var audit = new AuditService(db.Context);
        var foreignerService = new FamilyVisitorService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());
        var profileService = new CompanyProfileService(db.Context, foreignerService, audit, TestSecurityContext.CreateAllowAllGuard());

        var company = new Company { CompanyName = "LG Display", IDField = 1, TrackerID = 1 };
        var z1 = new EconomicZone { Code = "Z1", Name = "Zone 1", TypeCode = "IZ", ValidFrom = new DateTime(2020, 1, 1) };
        var z2 = new EconomicZone { Code = "Z2", Name = "Zone 2", TypeCode = "IZ", ValidFrom = new DateTime(2020, 1, 1) };
        db.Context.AddRange(company, z1, z2);
        await db.Context.SaveChangesAsync();

        var site = new CompanySite { CompanyId = company.IDCompany, AdministrativeUnitId = 1, Name = "Chi nhánh 1", AddressLine = "Địa chỉ 1" };
        var siteId = await profileService.SaveSiteAsync(site, [z1.Id]);

        Assert.Single(db.Context.SiteZoneMemberships.Where(x => x.CompanySiteId == siteId));
        Assert.Equal(z1.Id, db.Context.SiteZoneMemberships.Single(x => x.CompanySiteId == siteId).EconomicZoneId);

        // Update site with z2 only -> replaces z1 completely
        await profileService.SaveSiteAsync(site, [z2.Id]);
        Assert.Single(db.Context.SiteZoneMemberships.Where(x => x.CompanySiteId == siteId));
        Assert.Equal(z2.Id, db.Context.SiteZoneMemberships.Single(x => x.CompanySiteId == siteId).EconomicZoneId);
    }
}
