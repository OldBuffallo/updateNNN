using ClosedXML.Excel;
using IRM.Application.Features.Administration.Contracts;
using IRM.Data;
using IRM.Data.Models;
using IRM.Infrastructure.Excel;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class AdministrativeUnitExcelImportTests
{
    [Fact]
    public void BuildTemplateWorkbook_ProducesValidXlsxWithExpectedHeadersAndSampleRows()
    {
        var bytes = AdministrativeUnitExcelHelper.BuildTemplateWorkbook();
        Assert.NotEmpty(bytes);

        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();

        Assert.Equal("Mã địa bàn", ws.Cell(1, 1).GetString().Trim());
        Assert.Equal("Tên địa bàn", ws.Cell(1, 2).GetString().Trim());
        Assert.Equal("Loại", ws.Cell(1, 3).GetString().Trim());
        Assert.Equal("Hiệu lực từ", ws.Cell(1, 4).GetString().Trim());
        Assert.False(string.IsNullOrWhiteSpace(ws.Cell(2, 1).GetString()));
        Assert.False(string.IsNullOrWhiteSpace(ws.Cell(2, 2).GetString()));
    }

    [Fact]
    public void ParseWorkbook_ClassifiesCreateUpdateAndValidationErrors()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("DiaBan");
        ws.Cell(1, 1).Value = "Mã địa bàn";
        ws.Cell(1, 2).Value = "Tên địa bàn";
        ws.Cell(1, 3).Value = "Loại";
        ws.Cell(1, 4).Value = "Hiệu lực từ";

        // Row 2: Create new ward with Vietnamese type and dd/MM/yyyy date
        ws.Cell(2, 1).Value = "HN-W-001";
        ws.Cell(2, 2).Value = "Phường Ba Đình";
        ws.Cell(2, 3).Value = "Phường";
        ws.Cell(2, 4).Value = "01/07/2025";

        // Row 3: Update existing code (case-insensitive)
        ws.Cell(3, 1).Value = "hn-c-001";
        ws.Cell(3, 2).Value = "Xã Đông Anh (mới)";
        ws.Cell(3, 3).Value = "Xã";
        ws.Cell(3, 4).Value = "";

        // Row 4: Error — missing Code
        ws.Cell(4, 1).Value = "";
        ws.Cell(4, 2).Value = "Thiếu mã";
        ws.Cell(4, 3).Value = "Xã";

        // Row 5: Error — invalid non-dd/MM/yyyy date
        ws.Cell(5, 1).Value = "HN-W-002";
        ws.Cell(5, 2).Value = "Phường Hoàn Kiếm";
        ws.Cell(5, 3).Value = "Phường";
        ws.Cell(5, 4).Value = "2025-07-01";

        // Row 6: Error — duplicate Code inside same file
        ws.Cell(6, 1).Value = "HN-W-001";
        ws.Cell(6, 2).Value = "Trùng mã trong file";
        ws.Cell(6, 3).Value = "Phường";

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;

        var existing = new List<AdministrativeUnit>
        {
            new() { Id = 10, Code = "HN-C-001", Name = "Xã Đông Anh", TypeCode = AdministrativeUnitTypeCodes.Commune, ValidFrom = new DateTime(2025, 1, 1) }
        };

        var preview = AdministrativeUnitExcelHelper.ParseWorkbook(ms, "diaban.xlsx", existing);

        Assert.Equal("diaban.xlsx", preview.FileName);
        Assert.Equal(5, preview.TotalRows);
        Assert.Equal(2, preview.ValidCount);
        Assert.Equal(1, preview.CreateCount);
        Assert.Equal(1, preview.UpdateCount);
        Assert.Equal(3, preview.ErrorCount);

        var row2 = preview.Rows.Single(r => r.RowNumber == 2);
        Assert.Equal(AdministrativeUnitImportAction.Create, row2.Action);
        Assert.Equal(AdministrativeUnitTypeCodes.Ward, row2.TypeCode);
        Assert.Equal(new DateTime(2025, 7, 1), row2.ValidFrom);

        var row3 = preview.Rows.Single(r => r.RowNumber == 3);
        Assert.Equal(AdministrativeUnitImportAction.Update, row3.Action);
        Assert.Equal(AdministrativeUnitTypeCodes.Commune, row3.TypeCode);

        Assert.Equal(AdministrativeUnitImportAction.Error, preview.Rows.Single(r => r.RowNumber == 4).Action);
        Assert.Contains("dd/MM/yyyy", preview.Rows.Single(r => r.RowNumber == 5).ErrorMessage);
        Assert.Contains("trùng", preview.Rows.Single(r => r.RowNumber == 6).ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SqlCatalogStore_ImportAdministrativeUnitsExcel_UpsertsAndReactivatesWithoutBreakingExistingIds()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var store = new SqlCatalogStore(db.Context);

        // Seed 1 active unit and 1 soft-deleted unit
        var active = await store.CreateAdministrativeUnitAsync(new AdministrativeUnit
        {
            Code = "DB-001",
            Name = "Phường Cũ",
            TypeCode = AdministrativeUnitTypeCodes.Ward,
            ValidFrom = new DateTime(2025, 1, 1)
        });
        var deleted = await store.CreateAdministrativeUnitAsync(new AdministrativeUnit
        {
            Code = "DB-002",
            Name = "Xã Đã Ẩn",
            TypeCode = AdministrativeUnitTypeCodes.Commune,
            ValidFrom = new DateTime(2025, 1, 1)
        });
        await store.DeactivateAdministrativeUnitAsync(deleted.Id, null);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("DiaBan");
        ws.Cell(1, 1).Value = "Mã địa bàn";
        ws.Cell(1, 2).Value = "Tên địa bàn";
        ws.Cell(1, 3).Value = "Loại";
        ws.Cell(1, 4).Value = "Hiệu lực từ";

        // Update active
        ws.Cell(2, 1).Value = "DB-001";
        ws.Cell(2, 2).Value = "Phường Đã Đổi Tên";
        ws.Cell(2, 3).Value = "Phường";
        ws.Cell(2, 4).Value = "01/07/2025";

        // Reactivate deleted
        ws.Cell(3, 1).Value = "DB-002";
        ws.Cell(3, 2).Value = "Xã Khôi Phục";
        ws.Cell(3, 3).Value = "Xã";
        ws.Cell(3, 4).Value = "01/07/2025";

        // Create brand new
        ws.Cell(4, 1).Value = "DB-003";
        ws.Cell(4, 2).Value = "Đặc khu Mới";
        ws.Cell(4, 3).Value = "Đặc khu";
        ws.Cell(4, 4).Value = "01/07/2025";

        // Error row (skipped)
        ws.Cell(5, 1).Value = "";
        ws.Cell(5, 2).Value = "Không có mã";

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;

        var result = await store.ImportAdministrativeUnitsExcelAsync(ms, "import.xlsx");

        Assert.Equal(1, result.CreatedCount);
        Assert.Equal(1, result.UpdatedCount);
        Assert.Equal(1, result.ReactivatedCount);
        Assert.Equal(1, result.SkippedErrorCount);

        var reloadedActive = await db.Context.AdministrativeUnits.AsNoTracking().SingleAsync(x => x.Id == active.Id);
        Assert.Equal("Phường Đã Đổi Tên", reloadedActive.Name);
        Assert.False(reloadedActive.IsDeleted);

        var reloadedReactivated = await db.Context.AdministrativeUnits.AsNoTracking().SingleAsync(x => x.Id == deleted.Id);
        Assert.Equal("Xã Khôi Phục", reloadedReactivated.Name);
        Assert.False(reloadedReactivated.IsDeleted);
        Assert.Null(reloadedReactivated.ValidTo);

        var created = await db.Context.AdministrativeUnits.AsNoTracking().SingleAsync(x => x.Code == "DB-003");
        Assert.Equal(AdministrativeUnitTypeCodes.SpecialZone, created.TypeCode);
    }

    [Fact]
    public async Task SqlCatalogStore_CreateAdministrativeUnit_RejectsDuplicateCodeAndValidFromWithVietnameseMessage()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var store = new SqlCatalogStore(db.Context);

        await store.CreateAdministrativeUnitAsync(new AdministrativeUnit
        {
            Code = "DUP-01",
            Name = "Phường A",
            TypeCode = AdministrativeUnitTypeCodes.Ward,
            ValidFrom = new DateTime(2025, 7, 1)
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.CreateAdministrativeUnitAsync(new AdministrativeUnit
            {
                Code = " dup-01 ",
                Name = "Phường B",
                TypeCode = AdministrativeUnitTypeCodes.Ward,
                ValidFrom = new DateTime(2025, 7, 1)
            }));

        Assert.Contains("đã tồn tại", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SqlCatalogStore_ImportAdministrativeUnitsExcel_MatchesExactCodeAndValidFromWhenHistoricalDuplicatesExist()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var store = new SqlCatalogStore(db.Context);

        // Two historical rows with same Code, different ValidFrom
        var oldRow = await store.CreateAdministrativeUnitAsync(new AdministrativeUnit
        {
            Code = "HIST-01",
            Name = "Phường Cũ 2024",
            TypeCode = AdministrativeUnitTypeCodes.Ward,
            ValidFrom = new DateTime(2024, 1, 1)
        });
        await store.DeactivateAdministrativeUnitAsync(oldRow.Id, null);

        await store.CreateAdministrativeUnitAsync(new AdministrativeUnit
        {
            Code = "HIST-01",
            Name = "Phường Hiện Tại 2025",
            TypeCode = AdministrativeUnitTypeCodes.Ward,
            ValidFrom = new DateTime(2025, 7, 1)
        });

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("DiaBan");
        ws.Cell(1, 1).Value = "Mã địa bàn";
        ws.Cell(1, 2).Value = "Tên địa bàn";
        ws.Cell(1, 3).Value = "Loại";
        ws.Cell(1, 4).Value = "Hiệu lực từ";

        // Import row matching the 2024 historical ValidFrom -> must NOT collide with unique (Code, ValidFrom)
        ws.Cell(2, 1).Value = "HIST-01";
        ws.Cell(2, 2).Value = "Phường Khôi Phục 2024";
        ws.Cell(2, 3).Value = "Phường";
        ws.Cell(2, 4).Value = "01/01/2024";

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;

        var result = await store.ImportAdministrativeUnitsExcelAsync(ms, "hist.xlsx");

        Assert.Equal(1, result.ReactivatedCount);
        Assert.Equal(0, result.SkippedErrorCount);

        var reloadedOld = await db.Context.AdministrativeUnits.AsNoTracking().SingleAsync(x => x.Id == oldRow.Id);
        Assert.Equal("Phường Khôi Phục 2024", reloadedOld.Name);
        Assert.False(reloadedOld.IsDeleted);
        Assert.Null(reloadedOld.ValidTo);
    }

    [Fact]
    public async Task DatabaseSeeder_SeedCatalogsIfEmpty_DoesNotReinjectQuangNinhUnitsAfterCustomer2ReplacesUnits()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: false);

        // First boot seeds only 1 neutral starter unit (no Quang Ninh units)
        await DatabaseSeeder.SeedCatalogsIfEmptyAsync(db.Context, "admin_kh2", "AdminKh2@123456!");
        var initialUnits = await db.Context.AdministrativeUnits.AsNoTracking().ToListAsync();
        Assert.Single(initialUnits);
        Assert.DoesNotContain(initialUnits, u => u.Code.StartsWith("QN-", StringComparison.OrdinalIgnoreCase));

        // Customer 2 replaces starter unit with their own province's unit
        db.Context.AdministrativeUnits.RemoveRange(db.Context.AdministrativeUnits);
        db.Context.AdministrativeUnits.Add(new AdministrativeUnit
        {
            Code = "HN-W-001",
            Name = "Phường Ba Đình",
            TypeCode = AdministrativeUnitTypeCodes.Ward,
            ValidFrom = new DateTime(2025, 7, 1)
        });
        await db.Context.SaveChangesAsync();

        // App restart runs SeedCatalogsIfEmptyAsync again -> must NOT re-seed starter or Quang Ninh units
        await DatabaseSeeder.SeedCatalogsIfEmptyAsync(db.Context);
        var afterRestart = await db.Context.AdministrativeUnits.AsNoTracking().ToListAsync();
        Assert.Single(afterRestart);
        Assert.Equal("HN-W-001", afterRestart[0].Code);
    }
}
