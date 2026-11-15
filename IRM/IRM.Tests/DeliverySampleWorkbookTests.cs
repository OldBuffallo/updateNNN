using ClosedXML.Excel;
using IRM.Application.Features.ImportExport;
using IRM.Data;
using IRM.Data.Models;
using IRM.Infrastructure.Excel;
using IRM.Infrastructure.Persistence.Stores.ImportExport;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public class DeliverySampleWorkbookTests
{
    [Fact]
    public async Task SampleTestWorkbook_ImportsTenRowsCleanlyOnFreshR6SeededDatabase()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: false);
        await DatabaseSeeder.SeedCatalogsIfEmptyAsync(db.Context, "admin_r6", "AdminR6@123456!");
        await DatabaseSeeder.SeedOnboardingGuideIfEmptyAsync(db.Context);

        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "IRM-MAU-IMPORT-TEST-DA-SUA.xlsx");
        Assert.True(File.Exists(fixturePath), $"Missing fixture workbook at {fixturePath}");

        var generatedBytes = DeliveryWorkbooks.CreateSampleImportWorkbook();
        Assert.NotEmpty(generatedBytes);

        using (var wb = new XLWorkbook(fixturePath))
        {
            Assert.Equal(2, wb.Worksheets.Count);
            var ws = wb.Worksheet("Mẫu Import");
            var textCols = new[] { 5, 9, 10, 16 }; // Số hộ chiếu, Số GPLĐ, Số Visa, CMND người thân
            foreach (var col in textCols)
            {
                for (var row = 2; row <= 11; row++)
                {
                    Assert.Equal("@", ws.Cell(row, col).Style.NumberFormat.Format);
                }
            }
            Assert.Equal("000000000001", ws.Cell(2, 16).GetString());
            Assert.Equal("000000000004", ws.Cell(5, 16).GetString());
            Assert.Equal("000000000008", ws.Cell(9, 16).GetString());
            Assert.Equal("000000000010", ws.Cell(11, 16).GetString());
        }

        await using var stream = File.OpenRead(fixturePath);
        var sheets = ExcelReaderHelper.ReadSheetNames(stream);
        Assert.Equal("Mẫu Import", sheets[0]);

        var headers = ExcelReaderHelper.ReadHeaders(stream);
        Assert.Equal(19, headers.Count);

        var mappings = ImportMapping.AutoMapColumns(headers);
        Assert.DoesNotContain(mappings, m => m.SystemField == "skip");
        Assert.Empty(ImportMapping.GetMissingRequiredFields(mappings));

        var company = await db.Context.Companies.SingleAsync(c => c.CompanyName == DatabaseSeeder.InitialCompanyName);
        var techGroup = await db.Context.CareerGroups.SingleAsync(g => g.CareerGroupName == "Kỹ thuật");
        var relativeCareer = await db.Context.Careers.SingleAsync(c => c.CareerName == "Thân nhân");

        var store = new SqlImportStore(db.Context, new SqlAuditStore(db.Context));
        var options = new ImportReadOptions
        {
            CareerGroupId = techGroup.IDCG
        };

        var preview = await store.GeneratePreviewAsync(stream, mappings, company.IDCompany, options);
        Assert.Equal(10, preview.Count());
        Assert.All(preview, row =>
        {
            Assert.Equal("new", row.Status);
            Assert.Null(row.ErrorMessage);
            Assert.NotNull(row.ParsedEmployee);
            Assert.True(LegacyImportRules.IsValidNationalityCode(row.ParsedEmployee!.Nationality));
        });

        var row1 = preview.Single(r => r.ParsedEmployee!.Passport == "IRMTEST26001");
        Assert.Equal("000000000001", row1.ParsedEmployee!.FamilyVisitRelativeIdCard);

        var row10 = preview.Single(r => r.ParsedEmployee!.Passport == "IRMTEST26010");
        Assert.Equal(6, (int)row10.ParsedEmployee!.WorkPermit);
        Assert.Equal(relativeCareer.IDCareer, row10.ParsedEmployee!.IDCareer);

        var importResult = await store.ExecuteImportAsync(preview, [], company.IDCompany, "IRM-MAU-IMPORT-TEST-DA-SUA.xlsx");
        Assert.True(importResult.Success);
        Assert.Equal(10, importResult.AddedCount);
        Assert.Equal(0, importResult.ErrorCount);

        stream.Position = 0;
        var secondPreview = await store.GeneratePreviewAsync(stream, mappings, company.IDCompany, options);
        Assert.Equal(10, secondPreview.Count());
        Assert.All(secondPreview, row => Assert.Equal("unchanged", row.Status));
    }

    [Fact]
    public void FeedbackSurveyWorkbook_HasThreeSheetsValidationAndCountIfFormulas()
    {
        var bytes = DeliveryWorkbooks.CreateFeedbackSurveyWorkbook();
        Assert.NotEmpty(bytes);

        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        Assert.Equal(3, wb.Worksheets.Count);
        Assert.Equal("Tổng hợp & Thông tin", wb.Worksheet(1).Name);
        Assert.Equal("Chi tiết khảo sát", wb.Worksheet(2).Name);
        Assert.Equal("Đề xuất cột Excel thực tế", wb.Worksheet(3).Name);

        var summary = wb.Worksheet("Tổng hợp & Thông tin");
        var formulas = summary.CellsUsed(c => c.HasFormula).Select(c => c.FormulaA1).ToList();
        Assert.Contains(formulas, f => f.Contains("COUNTIF", StringComparison.OrdinalIgnoreCase) && f.Contains("'Chi tiết khảo sát'!$E$2:$E$23", StringComparison.Ordinal));

        var detail = wb.Worksheet("Chi tiết khảo sát");
        Assert.Equal(23, detail.LastRowUsed()!.RowNumber());
        Assert.NotEmpty(detail.DataValidations);
        var dvValues = string.Join(";", detail.DataValidations.Select(d => d.Value));
        Assert.Contains("Đạt yêu cầu", dvValues);
        Assert.Contains("Cần chỉnh sửa nhỏ", dvValues);
        Assert.Contains("Không đạt / Lỗi", dvValues);
    }

    [Fact]
    public void StaticFeedbackAssets_AndMainLayoutTrigger_ArePresentOffline()
    {
        var repoIrmDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var htmlPath = Path.Combine(repoIrmDir, "IRM", "wwwroot", "PHIEU-KHAO-SAT-GOP-Y.html");
        var xlsxPath = Path.Combine(repoIrmDir, "IRM", "wwwroot", "PHIEU-KHAO-SAT-GOP-Y.xlsx");
        var mainLayoutPath = Path.Combine(repoIrmDir, "IRM", "Components", "Shared", "MainLayout.razor");

        Assert.True(File.Exists(htmlPath), $"Missing {htmlPath}");
        Assert.True(File.Exists(xlsxPath), $"Missing {xlsxPath}");
        Assert.True(File.Exists(mainLayoutPath), $"Missing {mainLayoutPath}");

        var html = File.ReadAllText(htmlPath);
        Assert.DoesNotContain("https://", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http://", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("PHIẾU KHẢO SÁT", html);
        Assert.Contains("irm_r6_feedback_v1", html);

        var mainLayout = File.ReadAllText(mainLayoutPath);
        Assert.Contains("id=\"btnCustomerFeedback\"", mainLayout);
        Assert.Contains("irmInterop.initFeedbackSpotlight", mainLayout);
        Assert.Contains("/PHIEU-KHAO-SAT-GOP-Y.html", mainLayout);
        Assert.Contains("/PHIEU-KHAO-SAT-GOP-Y.xlsx", mainLayout);
    }
}
