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

    /// <summary>Export báo cáo tùy chỉnh — hỗ trợ bộ lọc và cột thăm thân</summary>
    public async Task<byte[]> ExportReportAsync(
        List<string> selectedColumns,
        string groupBy = "company",
        int? filterWorkPermit = null,
        string? filterNationality = null,
        int? filterExpiringDays = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        IQueryable<Employee> query = db.Employees
            .Include(e => e.Company).ThenInclude(c => c!.Field)
            .Include(e => e.NationalityNav)
            .Include(e => e.Career)
            .Where(e => e.WorkingStatus == 0 && e.Hidden_flag == 0);

        // Áp dụng bộ lọc
        if (filterWorkPermit.HasValue)
            query = query.Where(e => e.WorkPermit == filterWorkPermit.Value);
        if (!string.IsNullOrWhiteSpace(filterNationality))
            query = query.Where(e => e.Nationality == filterNationality);
        if (filterExpiringDays.HasValue)
        {
            var cutoff = DateTime.Today.AddDays(filterExpiringDays.Value);
            query = query.Where(e => e.TemporaryStay != null
                && e.TemporaryStay >= DateTime.Today
                && e.TemporaryStay <= cutoff);
        }

        var employees = await query
            .OrderBy(e => e.Company!.CompanyName)
            .ThenBy(e => e.StaffName)
            .ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Báo cáo");

        // Build headers from selected columns — hỗ trợ thêm cột thăm thân
        var colMap = new Dictionary<string, Func<Employee, string>>
        {
            ["Công ty"] = e => e.Company?.CompanyName ?? "",
            ["Họ tên"] = e => e.StaffName ?? "",
            ["Quốc tịch"] = e => e.NationalityNav?.NationalityName ?? e.Nationality ?? "",
            ["Hộ chiếu"] = e => e.Passport ?? "",
            ["Nghề nghiệp"] = e => e.Career?.CareerName ?? "",
            ["Loại GPLĐ"] = e => e.WorkPermitString ?? "",
            ["Số GPLĐ"] = e => e.WorkPermitNumber ?? "",
            ["Hạn tạm trú"] = e => e.TemporaryStay?.ToString("dd/MM/yyyy") ?? "",
            ["Giới tính"] = e => e.GenderString,
            ["Ngày sinh"] = e => e.Birthday?.ToString("dd/MM/yyyy") ?? "",
            ["Còn lại (ngày)"] = e => e.DaysUntilExpiry?.ToString() ?? "",
            ["Số Visa"] = e => e.VisaNumber ?? "",
            ["Địa chỉ"] = e => e.Address ?? "",
            ["Ghi chú"] = e => e.Note ?? "",
            // Cột thăm thân
            ["Thăm thân"] = e => e.FamilyVisitString,
            ["Tên người thân"] = e => e.FamilyVisitRelativeName ?? "",
            ["Quan hệ"] = e => e.FamilyVisitRelationship ?? "",
            ["CMND người thân"] = e => e.FamilyVisitRelativeIdCard ?? "",
            ["Hạn thăm thân"] = e => e.FamilyVisitEndDate?.ToString("dd/MM/yyyy") ?? ""
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

            // Color code expiring rows
            var days = emp.DaysUntilExpiry;
            if (days.HasValue && days.Value <= 7)
                ws.Row(row).Style.Font.FontColor = XLColor.Red;
            else if (days.HasValue && days.Value <= 30)
                ws.Row(row).Style.Font.FontColor = XLColor.FromHtml("#d97706");

            row++;
        }

        // Thêm hàng thống kê cuối
        row++;
        ws.Cell(row, 1).Value = "TỔNG KẾT";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 2).Value = $"Tổng: {employees.Count} nhân viên";
        ws.Cell(row, 2).Style.Font.Bold = true;
        row++;

        var groups = employees.GroupBy(e => groupBy switch
        {
            "nationality" => e.NationalityNav?.NationalityName ?? e.Nationality ?? "Khác",
            "field" => e.Company?.Field?.FieldName ?? "Khác",
            _ => e.Company?.CompanyName ?? "Khác"
        });
        foreach (var g in groups)
        {
            ws.Cell(row, 2).Value = $"{g.Key}: {g.Count()} NLĐ";
            row++;
        }

        ws.Columns().AdjustToContents();
        return WorkbookToBytes(wb);
    }

    /// <summary>Tạo file Excel mẫu để download</summary>
    public byte[] GenerateSampleExcelBytes()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Mẫu Import");

        // Header
        var headers = new[] {
            "Họ tên", "Giới tính", "Ngày sinh", "Quốc tịch", "Số hộ chiếu",
            "Địa chỉ", "Nghề nghiệp", "GPLĐ", "Số GPLĐ", "Số Visa",
            "Hạn tạm trú", "Ghi chú",
            "Thăm thân", "Tên người thân", "Quan hệ", "CMND người thân",
            "Ngày bắt đầu thăm thân", "Hạn thăm thân", "Ghi chú thăm thân"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#2563eb");
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        // Dữ liệu mẫu dòng 1
        var sample1 = new[] {
            "NGUYEN VAN A", "Nam", "15/03/1985", "Trung Quốc", "E12345678",
            "123 Đường ABC, Quận 1", "Kỹ sư phần mềm", "Đã có GPLĐ", "GP-2024-001", "V-2024-001",
            "31/12/2025", "Ghi chú mẫu",
            "Có", "Trần Thị B", "Vợ/Chồng", "012345678901",
            "01/01/2024", "31/12/2025", ""
        };
        for (int i = 0; i < sample1.Length; i++)
            ws.Cell(2, i + 1).Value = sample1[i];

        // Dữ liệu mẫu dòng 2
        var sample2 = new[] {
            "PARK MIN YOUNG", "Nữ", "22/07/1990", "Hàn Quốc", "K98765432",
            "456 Đường XYZ, Quận 7", "Giám đốc dự án", "Miễn GPLĐ", "", "V-2024-002",
            "30/06/2026", "",
            "Không", "", "", "",
            "", "", ""
        };
        for (int i = 0; i < sample2.Length; i++)
            ws.Cell(3, i + 1).Value = sample2[i];

        // Sheet hướng dẫn
        var guide = wb.Worksheets.Add("Hướng dẫn");
        guide.Cell(1, 1).Value = "HƯỚNG DẪN SỬ DỤNG FILE IMPORT";
        guide.Cell(1, 1).Style.Font.Bold = true;
        guide.Cell(1, 1).Style.Font.FontSize = 14;

        var guideData = new (string field, string format, string example, string note)[] {
            ("Họ tên", "Văn bản", "NGUYEN VAN A", "Bắt buộc. In hoa hoặc thường đều được"),
            ("Giới tính", "Nam / Nữ / Male / Female", "Nam", "Mặc định: Nữ nếu để trống"),
            ("Ngày sinh", "dd/MM/yyyy", "15/03/1985", "Tùy chọn. Nhập đúng định dạng"),
            ("Quốc tịch", "Tên hoặc mã quốc gia", "Trung Quốc hoặc CN", "Hệ thống tự nhận dạng"),
            ("Số hộ chiếu", "Văn bản", "E12345678", "Quan trọng: dùng để kiểm tra trùng lặp"),
            ("Địa chỉ", "Văn bản", "123 Đường ABC", "Tùy chọn"),
            ("Nghề nghiệp", "Tên nghề nghiệp", "Kỹ sư phần mềm", "Hệ thống tự match với danh mục"),
            ("GPLĐ", "Đã có GPLĐ / Miễn GPLĐ / Chưa có", "Đã có GPLĐ", "Mặc định: Miễn GPLĐ"),
            ("Số GPLĐ", "Văn bản", "GP-2024-001", "Tùy chọn"),
            ("Số Visa", "Văn bản", "V-2024-001", "Tùy chọn"),
            ("Hạn tạm trú", "dd/MM/yyyy", "31/12/2025", "Quan trọng: dùng để cảnh báo hết hạn"),
            ("Ghi chú", "Văn bản", "Ghi chú tùy ý", "Tùy chọn"),
            ("Thăm thân", "Có / Không", "Có", "Mặc định: Không"),
            ("Tên người thân", "Văn bản", "Trần Thị B", "Điền nếu thăm thân = Có"),
            ("Quan hệ", "Vợ/Chồng/Con/Cha mẹ", "Vợ/Chồng", "Điền nếu thăm thân = Có"),
            ("CMND người thân", "Số CMND/CCCD", "012345678901", "Điền nếu thăm thân = Có"),
            ("Ngày bắt đầu thăm thân", "dd/MM/yyyy", "01/01/2024", "Tùy chọn"),
            ("Hạn thăm thân", "dd/MM/yyyy", "31/12/2025", "Tùy chọn"),
            ("Ghi chú thăm thân", "Văn bản", "", "Tùy chọn")
        };

        guide.Cell(3, 1).Value = "Tên cột"; guide.Cell(3, 1).Style.Font.Bold = true;
        guide.Cell(3, 2).Value = "Định dạng"; guide.Cell(3, 2).Style.Font.Bold = true;
        guide.Cell(3, 3).Value = "Ví dụ"; guide.Cell(3, 3).Style.Font.Bold = true;
        guide.Cell(3, 4).Value = "Ghi chú"; guide.Cell(3, 4).Style.Font.Bold = true;
        for (int i = 0; i < guideData.Length; i++)
        {
            guide.Cell(4 + i, 1).Value = guideData[i].field;
            guide.Cell(4 + i, 2).Value = guideData[i].format;
            guide.Cell(4 + i, 3).Value = guideData[i].example;
            guide.Cell(4 + i, 4).Value = guideData[i].note;

            // Highlight bắt buộc
            if (guideData[i].note.Contains("Bắt buộc") || guideData[i].note.Contains("Quan trọng"))
            {
                guide.Cell(4 + i, 1).Style.Font.Bold = true;
                guide.Cell(4 + i, 4).Style.Font.FontColor = XLColor.Red;
            }
        }

        guide.Columns().AdjustToContents();
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
