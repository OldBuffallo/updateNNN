using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for CompanyLegalDocument; never tracked by EF.</summary>
public sealed class CompanyLegalDocumentDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string TypeCode { get; set; } = "BUSINESS_REGISTRATION";
    public string? DocumentNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public string? IssuedBy { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? StoredFileId { get; set; }
    public int? LegacyAttachId { get; set; }
    public string? Note { get; set; }
    public bool IsDeleted { get; set; }
    public CompanyDto? Company { get; set; }
    public StoredFileDto? StoredFile { get; set; }
    public AttachDto? LegacyAttach { get; set; }
}
