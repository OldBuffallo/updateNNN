

namespace IRM.Data.Models;

public class CompanyLegalDocument
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

    public Company? Company { get; set; }
    public StoredFile? StoredFile { get; set; }
    public Attach? LegacyAttach { get; set; }
}
