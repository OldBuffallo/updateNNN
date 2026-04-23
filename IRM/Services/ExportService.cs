using ClosedXML.Excel;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

public class ExportService
{
    private readonly IDbContextFactory<IrmDbContext> _dbFactory;

    public ExportService(IDbContextFactory<IrmDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    /// <summary>Export danh sách công ty ra Excel</summary>
    public async Task<byte[]> ExportCompaniesAsync(string? search = null, int? fieldId = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        IQueryable<Company> query = db.Companies
            .Include(c => c.Field)
            .Include(c => c.Tracker)
            .OrderBy(c => c.CompanyName);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.CompanyName.Contains(search));
        if (fieldId.HasValue)
            query = query.Where(c => c.IDField == fieldId.Value);

        var companies = await query.ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Danh sách Công ty");

        // Header
        var headers = new[] { "STT", "Tên công ty", "Loại hình", "Lĩnh vực", "Tổng LĐ", "Có GPLĐ", "Chưa có", "Miễn", "Cán bộ TD" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#2563eb");
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        // Data
        for (int r = 0; r < companies.Count; r++)
        {
            var c = companies[r];
            ws.Cell(r + 2, 1).Value = r + 1;
            ws.Cell(r + 2, 2).Value = c.CompanyName;
            ws.Cell(r + 2, 3).Value = c.TypeOfBusiniess;
            ws.Cell(r + 2, 4).Value = c.Field?.FieldName ?? "";
            ws.Cell(r + 2, 5).Value = c.TotalAmount;
            ws.Cell(r + 2, 6).Value = c.QuantityAvailable;
            ws.Cell(r + 2, 7).Value = c.QuantityNotYet;
            ws.Cell(r + 2, 8).Value = c.AmountOfExemption;
            ws.Cell(r + 2, 9).Value = c.Tracker?.Name ?? "";
        }

        ws.Columns().AdjustToContents();
        return WorkbookToBytes(wb);
    }

    /// <summary>Export danh sách nhân viên ra Excel</summary>
    public async Task<byte[]> ExportEmployeesAsync(int? companyId = null, string? nationality = null, int? workPermit = null, bool expiringOnly = false)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        IQueryable<Employee> query = db.Employees
            .Include(e => e.Company)
            .Include(e => e.NationalityNav)
            .Include(e => e.Career)
            .Where(e => e.WorkingStatus == 0)
            .OrderBy(e => e.StaffName);

        if (companyId.HasValue)
            query = query.Where(e => e.IDCompany == companyId.Value);
        if (!string.IsNullOrWhiteSpace(nationality))
            query = query.Where(e => e.Nationality == nationality);
        if (workPermit.HasValue)
            query = query.Where(e => e.WorkPermit == workPermit.Value);
        if (expiringOnly)
        {
            var cutoff = DateTime.Now.AddDays(90);
            query = query.Where(e => e.TemporaryStay != null && e.TemporaryStay <= cutoff);
        }

        var employees = await query.ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Danh sách Nhân viên");

        var headers = new[] { "STT", "Họ tên", "Quốc tịch", "Hộ chiếu", "Công ty", "Nghề nghiệp", "GPLĐ", "Hạn tạm trú", "Còn lại (ngày)" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#059669");
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        for (int r = 0; r < employees.Count; r++)
        {
            var e = employees[r];
            var days = e.DaysUntilExpiry;
            ws.Cell(r + 2, 1).Value = r + 1;
            ws.Cell(r + 2, 2).Value = e.StaffName;
            ws.Cell(r + 2, 3).Value = e.NationalityNav?.NationalityName ?? e.Nationality;
            ws.Cell(r + 2, 4).Value = e.Passport;
            ws.Cell(r + 2, 5).Value = e.Company?.CompanyName ?? "";
            ws.Cell(r + 2, 6).Value = e.Career?.CareerName ?? "";
            ws.Cell(r + 2, 7).Value = e.WorkPermitString;
            ws.Cell(r + 2, 8).Value = e.TemporaryStay?.ToString("dd/MM/yyyy") ?? "";
            ws.Cell(r + 2, 9).Value = days.HasValue ? days.Value.ToString() : "";

            // Color code expiring rows
            if (days.HasValue && days.Value <= 7)
                ws.Row(r + 2).Style.Font.FontColor = XLColor.Red;
            else if (days.HasValue && days.Value <= 30)
                ws.Row(r + 2).Style.Font.FontColor = XLColor.FromHtml("#d97706");
        }

        ws.Columns().AdjustToContents();
        return WorkbookToBytes(wb);
    }

    /// <summary>Export báo cáo tùy chỉnh</summary>
    public async Task<byte[]> ExportReportAsync(List<string> selectedColumns, string groupBy = "company")
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var employees = await db.Employees
            .Include(e => e.Company).ThenInclude(c => c!.Field)
            .Include(e => e.NationalityNav)
            .Include(e => e.Career)
            .Where(e => e.WorkingStatus == 0)
            .OrderBy(e => e.Company!.CompanyName)
            .ThenBy(e => e.StaffName)
            .ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Báo cáo");

        // Build headers from selected columns
        var colMap = new Dictionary<string, Func<Employee, string>>
        {
            ["Công ty"] = e => e.Company?.CompanyName ?? "",
            ["Họ tên"] = e => e.StaffName ?? "",
            ["Quốc tịch"] = e => e.NationalityNav?.NationalityName ?? e.Nationality ?? "",
            ["Hộ chiếu"] = e => e.Passport ?? "",
            ["Nghề nghiệp"] = e => e.Career?.CareerName ?? "",
            ["Loại GPLĐ"] = e => e.WorkPermitString ?? "",
            ["Hạn tạm trú"] = e => e.TemporaryStay?.ToString("dd/MM/yyyy") ?? "",
            ["Giới tính"] = e => e.GenderString,
            ["Ngày sinh"] = e => e.Birthday?.ToString("dd/MM/yyyy") ?? "",
            ["Còn lại (ngày)"] = e => e.DaysUntilExpiry?.ToString() ?? ""
        };

        var cols = selectedColumns.Where(c => colMap.ContainsKey(c)).ToList();
        if (!cols.Any()) cols = colMap.Keys.Take(7).ToList();

        // Write header
        ws.Cell(1, 1).Value = "STT";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#2563eb");
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.White;
        for (int i = 0; i < cols.Count; i++)
        {
            ws.Cell(1, i + 2).Value = cols[i];
            ws.Cell(1, i + 2).Style.Font.Bold = true;
            ws.Cell(1, i + 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#2563eb");
            ws.Cell(1, i + 2).Style.Font.FontColor = XLColor.White;
        }

        // Group and write data
        int row = 2;
        int stt = 1;
        string? currentGroup = null;

        foreach (var emp in employees)
        {
            var groupValue = groupBy switch
            {
                "nationality" => emp.NationalityNav?.NationalityName ?? emp.Nationality ?? "Khác",
                "field" => emp.Company?.Field?.FieldName ?? "Khác",
                _ => emp.Company?.CompanyName ?? "Khác"
            };

            if (groupValue != currentGroup)
            {
                currentGroup = groupValue;
                ws.Cell(row, 1).Value = $"🏢 {currentGroup}";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#f0f4f8");
                ws.Range(row, 1, row, cols.Count + 1).Merge();
                row++;
            }

            ws.Cell(row, 1).Value = stt++;
            for (int i = 0; i < cols.Count; i++)
            {
                ws.Cell(row, i + 2).Value = colMap[cols[i]](emp);
            }
            row++;
        }

        ws.Columns().AdjustToContents();
        return WorkbookToBytes(wb);
    }

    private static byte[] WorkbookToBytes(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
