using IRM.Data.Models;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class LegacySyncFeatureTddTests
{
    [Fact]
    public async Task ProcessPendingAsync_ParsesXml_AndCreatesForeignPersonWithSourceLink()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var registry = new FamilyVisitorService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());
        var syncService = new LegacySyncService(db.Context, registry);

        var xml = "<row><StaffName>Tanaka Taro</StaffName><Passport>JP-001</Passport><Nationality>JPN</Nationality></row>";
        var evt = new LegacyChangeEvent
        {
            EntityType = "Employees",
            EntityId = "100",
            AfterXml = xml,
            StatusCode = "PENDING"
        };
        db.Context.LegacyChangeEvents.Add(evt);
        await db.Context.SaveChangesAsync();

        var processed = await syncService.ProcessPendingAsync(10);
        Assert.Equal(1, processed);

        var updatedEvt = await db.Context.LegacyChangeEvents.SingleAsync();
        Assert.Equal("PROCESSED", updatedEvt.StatusCode);
        Assert.NotNull(updatedEvt.ProcessedAt);

        var person = await db.Context.ForeignPersons.SingleAsync();
        Assert.Equal("Tanaka Taro", person.FullName);
        Assert.Equal("JP001", person.PassportNumber);

        var link = await db.Context.ForeignPersonSourceLinks.SingleAsync();
        Assert.Equal("EMPLOYEE", link.SourceType);
        Assert.Equal("100", link.SourceId);
        Assert.Equal(person.Id, link.ForeignPersonId);
    }

    [Fact]
    public async Task ProcessPendingAsync_DeadLetterAfter5Retries()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var registry = new FamilyVisitorService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());
        var syncService = new LegacySyncService(db.Context, registry);

        var corruptXml = "<unclosed-tag><StaffName>";
        var evt = new LegacyChangeEvent
        {
            EntityType = "Employees",
            EntityId = "999",
            AfterXml = corruptXml,
            StatusCode = "PENDING",
            RetryCount = 4
        };
        db.Context.LegacyChangeEvents.Add(evt);
        await db.Context.SaveChangesAsync();

        await syncService.ProcessPendingAsync(10);

        var updatedEvt = await db.Context.LegacyChangeEvents.SingleAsync();
        Assert.Equal("DEAD_LETTER", updatedEvt.StatusCode);
        Assert.Equal(5, updatedEvt.RetryCount);
        Assert.NotNull(updatedEvt.LastError);
    }

    [Fact]
    public async Task BackfillLegacyAsync_HandlesDuplicateAndMissingPassports()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var registry = new FamilyVisitorService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        // Employee 1: Valid
        var e1 = new Employee { StaffName = "Valid Person", Passport = "P-VALID", IDCompany = 1, IDCareer = 1, Nationality = "USA", Hidden_flag = 0 };
        // Employee 2: Missing passport
        var e2 = new Employee { StaffName = "Missing Passport Person", Passport = null, IDCompany = 1, IDCareer = 1, Nationality = "USA", Hidden_flag = 0 };
        db.Context.Employees.AddRange(e1, e2);
        await db.Context.SaveChangesAsync();

        var result = await registry.BackfillLegacyAsync();
        Assert.Equal(1, result.Linked);
        Assert.Equal(1, result.Created);
        Assert.Equal(1, result.Issues);

        var issues = await db.Context.MigrationIssues.ToListAsync();
        Assert.Single(issues);
        Assert.Equal("MISSING_PASSPORT", issues.Single().IssueCode);
    }
}
