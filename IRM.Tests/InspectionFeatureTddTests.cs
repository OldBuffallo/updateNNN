using IRM.Data.Models;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class InspectionFeatureTddTests
{
    [Fact]
    public async Task SaveAsync_EnforcesInspectorRole_AndMandatoryFields()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var foreignerService = new FamilyVisitorService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        var viewerGuard = TestSecurityContext.CreateGuard(IrmRoles.Viewer);
        var serviceViewer = new InspectionService(db.Context, foreignerService, audit, viewerGuard);

        var insp = new Inspection { InspectedAt = DateTime.Today, LocationText = "Công ty A", InspectorNames = "Cán bộ 1" };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => serviceViewer.SaveAsync(insp));

        var inspectorGuard = TestSecurityContext.CreateGuard(IrmRoles.Inspector);
        var serviceInspector = new InspectionService(db.Context, foreignerService, audit, inspectorGuard);

        var emptyLocation = new Inspection { InspectedAt = DateTime.Today, LocationText = "" };
        await Assert.ThrowsAsync<ArgumentException>(() => serviceInspector.SaveAsync(emptyLocation));

        var id = await serviceInspector.SaveAsync(insp);
        Assert.True(id > 0);
    }

    [Fact]
    public async Task AddSubjectAsync_EnforcesUniqueSubjectPerInspection_AndPreservesSnapshot()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var foreignerService = new FamilyVisitorService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());
        var inspectionService = new InspectionService(db.Context, foreignerService, audit, TestSecurityContext.CreateAllowAllGuard());

        var insp = new Inspection { InspectedAt = DateTime.Today, LocationText = "Khách sạn X", InspectorNames = "Đội 1" };
        var inspId = await inspectionService.SaveAsync(insp);

        var personInput = new FamilyVisitInput { FullName = "Subject A", PassportNumber = "SUB-01", NationalityCode = "CHN" };
        var subject1 = new InspectionSubject
        {
            PurposeCodeSnapshot = StayPurposeCodes.Tourism,
            DocumentsSnapshot = "VISA-T01",
            ResultCode = "VALID"
        };

        var subjId = await inspectionService.AddSubjectAsync(inspId, personInput, subject1);
        Assert.True(subjId > 0);

        var addedSubject = await db.Context.InspectionSubjects.FindAsync(subjId);
        Assert.NotNull(addedSubject);
        Assert.Equal(StayPurposeCodes.Tourism, addedSubject!.PurposeCodeSnapshot);
        Assert.Equal("VISA-T01", addedSubject.DocumentsSnapshot);
        Assert.Equal("VALID", addedSubject.ResultCode);

        // Adding same person again to same inspection -> throws
        var duplicatePersonInput = new FamilyVisitInput { ForeignPersonId = addedSubject.ForeignPersonId, FullName = "Subject A" };
        var subject2 = new InspectionSubject { ResultCode = "WARNING" };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            inspectionService.AddSubjectAsync(inspId, duplicatePersonInput, subject2));
        Assert.Contains("đã có trong đợt kiểm tra", ex.Message);
    }

    [Fact]
    public async Task GetCheckedPersonIdsAsync_ValidatesLookbackWindowBoundaries()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var foreignerService = new FamilyVisitorService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());
        var inspectionService = new InspectionService(db.Context, foreignerService, audit, TestSecurityContext.CreateAllowAllGuard());

        var p1 = new ForeignPerson { FullName = "P1" };
        var p2 = new ForeignPerson { FullName = "P2" };
        var p3 = new ForeignPerson { FullName = "P3" };
        db.Context.ForeignPersons.AddRange(p1, p2, p3);
        await db.Context.SaveChangesAsync();

        var inspOld = new Inspection { InspectedAt = new DateTime(2026, 1, 1), LocationText = "L1", InspectorNames = "I" };
        var inspInside = new Inspection { InspectedAt = new DateTime(2026, 6, 15), LocationText = "L2", InspectorNames = "I" };
        var inspFuture = new Inspection { InspectedAt = new DateTime(2026, 12, 1), LocationText = "L3", InspectorNames = "I" };
        db.Context.Inspections.AddRange(inspOld, inspInside, inspFuture);
        await db.Context.SaveChangesAsync();

        db.Context.InspectionSubjects.AddRange(
            new InspectionSubject { InspectionId = inspOld.Id, ForeignPersonId = p1.Id },
            new InspectionSubject { InspectionId = inspInside.Id, ForeignPersonId = p2.Id },
            new InspectionSubject { InspectionId = inspFuture.Id, ForeignPersonId = p3.Id }
        );
        await db.Context.SaveChangesAsync();

        var lookbackSet = await inspectionService.GetCheckedPersonIdsAsync(
            new DateTime(2026, 6, 1), new DateTime(2026, 6, 30), null);

        Assert.Single(lookbackSet);
        Assert.Contains(p2.Id, lookbackSet);
        Assert.DoesNotContain(p1.Id, lookbackSet);
        Assert.DoesNotContain(p3.Id, lookbackSet);
    }
}
