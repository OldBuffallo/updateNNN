using ClosedXML.Excel;

namespace IRM.Services;

public sealed class StatisticsExportService : IStatisticsExportService
{
    private readonly IStatisticsService _statistics;
    private readonly AuditService _audit;
    private readonly IServiceAuthorizationGuard? _guard;
    public StatisticsExportService(IStatisticsService statistics, AuditService audit) { _statistics = statistics; _audit = audit; }
    public StatisticsExportService(IStatisticsService statistics, AuditService audit, IServiceAuthorizationGuard guard) : this(statistics, audit) => _guard = guard;

    public async Task<byte[]> ExportTerritoryAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.Reporter);
        var data = await _statistics.GetTerritoryAsync(filter, cancellationToken);
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Thong ke dia ban");
        sheet.Cell(1, 1).Value = "Địa bàn"; sheet.Cell(1, 2).Value = Safe(data.AdministrativeUnitName);
        sheet.Cell(2, 1).Value = "Tổng người nước ngoài"; sheet.Cell(2, 2).Value = data.TotalForeigners;
        var row = 4;
        sheet.Cell(row, 1).Value = "Diện cư trú"; sheet.Cell(row, 2).Value = "Số người (không trùng)";
        foreach (var item in data.PeopleByPurpose.OrderBy(x => x.Key))
        { row++; sheet.Cell(row, 1).Value = Safe(item.Key); sheet.Cell(row, 2).Value = item.Value; }
        row += 2; sheet.Cell(row, 1).Value = "Doanh nghiệp"; sheet.Cell(row, 2).Value = "Loại"; sheet.Cell(row, 3).Value = "Lao động";
        foreach (var company in data.Companies)
        { row++; sheet.Cell(row, 1).Value = Safe(company.CompanyName); sheet.Cell(row, 2).Value = Safe(company.OwnershipTypeCode); sheet.Cell(row, 3).Value = company.CurrentWorkerCount; }
        row += 2; sheet.Cell(row, 1).Value = "Cơ sở lưu trú"; sheet.Cell(row, 2).Value = "Loại"; sheet.Cell(row, 3).Value = "Người ở"; sheet.Cell(row, 4).Value = "Doanh nghiệp thuê";
        foreach (var accommodation in data.Accommodations)
        { row++; sheet.Cell(row, 1).Value = Safe(accommodation.Name); sheet.Cell(row, 2).Value = Safe(accommodation.TypeCode); sheet.Cell(row, 3).Value = accommodation.CurrentResidentCount; sheet.Cell(row, 4).Value = Safe(accommodation.Companies); }
        sheet.Columns().AdjustToContents();
        await using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        await _audit.LogAsync("EXPORT", "TerritoryStatistics", filter.AdministrativeUnitId,
            $"AsOf={filter.AsOfDate:yyyy-MM-dd}; Rows={row}");
        return stream.ToArray();
    }

    public static string Safe(string? value)
    {
        value ??= "";
        return value.Length > 0 && "=+-@\t\r".Contains(value[0]) ? "'" + value : value;
    }
}
