namespace IRM.Data.Models;

/// <summary>DTO tương thích UI; dữ liệu được lưu bằng CompanyLegalDocument.</summary>
public class LegalDocument
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string DocumentTypeCode { get; set; } = "BUSINESS_REGISTRATION";
    public string? DocumentNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public string? IssuedBy { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? StoredFileId { get; set; }
    public string? Note { get; set; }
}
