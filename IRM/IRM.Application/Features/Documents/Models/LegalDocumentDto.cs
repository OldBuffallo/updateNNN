using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for LegalDocument; never tracked by EF.</summary>
public sealed class LegalDocumentDto
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
