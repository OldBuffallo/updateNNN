using IRM.Data.Models;

namespace IRM.Services;

public sealed class TemporalReportFilter
{
    public DateTime? AsOfDate { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? AdministrativeUnitId { get; set; }
    public string? StayPurposeCode { get; set; }
    public int? EconomicZoneId { get; set; }
    public string? CompanyOwnershipTypeCode { get; set; }
    public string? DocumentTypeCode { get; set; }
    public bool? HasElectronicIdentity { get; set; }

    public void Validate()
    {
        if (AsOfDate.HasValue && (From.HasValue || To.HasValue))
            throw new ArgumentException("AsOfDate không được dùng đồng thời với From/To.");
        if (From.HasValue && To.HasValue && From.Value.Date > To.Value.Date)
            throw new ArgumentException("From phải nhỏ hơn hoặc bằng To.");
    }
}
