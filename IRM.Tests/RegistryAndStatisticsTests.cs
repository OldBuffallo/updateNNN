using IRM.Data.Models;
using IRM.Services;

namespace IRM.Tests;

public sealed class RegistryAndStatisticsTests
{
    [Theory]
    [InlineData(" a-12 34 ", "A1234")]
    [InlineData(null, null)]
    public void PassportNormalization_IsStable(string? value, string? expected) =>
        Assert.Equal(expected, FamilyVisitorService.NormalizePassport(value));

    [Fact]
    public async Task FamilyVisit_SaveAndOverlapValidation_AreTransactional()
    {
        await using var database = new TestDatabase(); await database.InitializeAsync();
        var service = new FamilyVisitorService(database.Context, new AuditService(database.Context));
        var id = await service.SaveFamilyVisitAsync(new FamilyVisitInput
        {
            FullName="Alice",PassportNumber="p-001",RelativeName="Nguyễn An",Relationship="Vợ",
            ValidFrom=new DateTime(2026,1,1),ValidTo=new DateTime(2026,12,31)
        });
        Assert.True(id > 0);
        var personId = database.Context.ForeignPersons.Single().Id;
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveFamilyVisitAsync(new FamilyVisitInput
        {
            ForeignPersonId=personId,FullName="Alice",PassportNumber="P001",RelativeName="Nguyễn An",Relationship="Vợ",
            ValidFrom=new DateTime(2026,6,1),ValidTo=new DateTime(2027,1,1)
        }));
        Assert.Contains("trùng", exception.Message);
        Assert.Single(database.Context.StayCases);
    }

    [Fact]
    public async Task TerritoryStatistics_CountsDistinctPeople_AtAsOfDate()
    {
        await using var database = new TestDatabase(); await database.InitializeAsync();
        var unit = new AdministrativeUnit{Code="U1",Name="Hạ Long",TypeCode=AdministrativeUnitTypeCodes.Ward,ValidFrom=new DateTime(2025,1,1)};
        var person = new ForeignPerson{FullName="Alice",PassportSearchKey="K"};
        database.Context.AddRange(unit,person); await database.Context.SaveChangesAsync();
        database.Context.StayCases.Add(new StayCase{ForeignPersonId=person.Id,PurposeCode=StayPurposeCodes.Work,IsPrimary=true,ValidFrom=new DateTime(2025,1,1)});
        database.Context.ResidencePeriods.AddRange(
            new ResidencePeriod{ForeignPersonId=person.Id,AdministrativeUnitId=unit.Id,AddressLine="A",ValidFrom=new DateTime(2025,1,1)},
            new ResidencePeriod{ForeignPersonId=person.Id,AdministrativeUnitId=unit.Id,AddressLine="A2",ValidFrom=new DateTime(2025,2,1)});
        await database.Context.SaveChangesAsync();
        var service = new StatisticsService(database.Context,new AuditService(database.Context));
        var result = await service.GetTerritoryAsync(new TemporalReportFilter{AdministrativeUnitId=unit.Id,AsOfDate=new DateTime(2026,1,1)});
        Assert.Equal(1,result.TotalForeigners);
        Assert.Equal(1,result.PeopleByPurpose[StayPurposeCodes.Work]);
    }

    [Fact]
    public async Task InspectionLookback_OnlyReturnsChosenWindow()
    {
        await using var database = new TestDatabase(); await database.InitializeAsync();
        var person = new ForeignPerson{FullName="Alice"}; database.Context.Add(person); await database.Context.SaveChangesAsync();
        var oldInspection=new Inspection{InspectedAt=new DateTime(2025,1,1),LocationText="A",InspectorNames="I"};
        var recentInspection=new Inspection{InspectedAt=new DateTime(2026,6,1),LocationText="B",InspectorNames="I"};
        database.Context.AddRange(oldInspection,recentInspection);await database.Context.SaveChangesAsync();
        database.Context.AddRange(new InspectionSubject{InspectionId=oldInspection.Id,ForeignPersonId=person.Id},new InspectionSubject{InspectionId=recentInspection.Id,ForeignPersonId=person.Id});await database.Context.SaveChangesAsync();
        var registry=new FamilyVisitorService(database.Context,new AuditService(database.Context));
        var service=new InspectionService(database.Context,registry,new AuditService(database.Context));
        var ids=await service.GetCheckedPersonIdsAsync(new DateTime(2026,5,1),new DateTime(2026,6,30),null);
        Assert.Single(ids);Assert.Contains(person.Id,ids);
    }
}
