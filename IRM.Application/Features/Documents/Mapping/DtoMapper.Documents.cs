using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

internal static partial class DtoMapper
{
    public static CompanyLegalDocumentDto ToDto(CompanyLegalDocument value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static CompanyLegalDocumentDto ToDto(CompanyLegalDocument value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (CompanyLegalDocumentDto)existing;
        var result = new CompanyLegalDocumentDto();
        seen[value] = result;
        result.Id = value.Id;
        result.CompanyId = value.CompanyId;
        result.TypeCode = value.TypeCode;
        result.DocumentNumber = value.DocumentNumber;
        result.IssueDate = value.IssueDate;
        result.IssuedBy = value.IssuedBy;
        result.ExpiryDate = value.ExpiryDate;
        result.StoredFileId = value.StoredFileId;
        result.LegacyAttachId = value.LegacyAttachId;
        result.Note = value.Note;
        result.IsDeleted = value.IsDeleted;
        result.Company = value.Company is null ? null : ToDto(value.Company, seen);
        result.StoredFile = value.StoredFile is null ? null : ToDto(value.StoredFile, seen);
        result.LegacyAttach = value.LegacyAttach is null ? null : ToDto(value.LegacyAttach, seen);
        return result;
    }

    public static CompanyLegalDocument ToEntity(CompanyLegalDocumentDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static CompanyLegalDocument ToEntity(CompanyLegalDocumentDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (CompanyLegalDocument)existing;
        var result = new CompanyLegalDocument();
        seen[value] = result;
        result.Id = value.Id;
        result.CompanyId = value.CompanyId;
        result.TypeCode = value.TypeCode;
        result.DocumentNumber = value.DocumentNumber;
        result.IssueDate = value.IssueDate;
        result.IssuedBy = value.IssuedBy;
        result.ExpiryDate = value.ExpiryDate;
        result.StoredFileId = value.StoredFileId;
        result.LegacyAttachId = value.LegacyAttachId;
        result.Note = value.Note;
        result.IsDeleted = value.IsDeleted;
        result.Company = value.Company is null ? null : ToEntity(value.Company, seen);
        result.StoredFile = value.StoredFile is null ? null : ToEntity(value.StoredFile, seen);
        result.LegacyAttach = value.LegacyAttach is null ? null : ToEntity(value.LegacyAttach, seen);
        return result;
    }

    public static void CopyToDto(CompanyLegalDocument value, CompanyLegalDocumentDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.CompanyId = mapped.CompanyId;
        target.TypeCode = mapped.TypeCode;
        target.DocumentNumber = mapped.DocumentNumber;
        target.IssueDate = mapped.IssueDate;
        target.IssuedBy = mapped.IssuedBy;
        target.ExpiryDate = mapped.ExpiryDate;
        target.StoredFileId = mapped.StoredFileId;
        target.LegacyAttachId = mapped.LegacyAttachId;
        target.Note = mapped.Note;
        target.IsDeleted = mapped.IsDeleted;
        target.Company = mapped.Company;
        target.StoredFile = mapped.StoredFile;
        target.LegacyAttach = mapped.LegacyAttach;
    }

    public static LegalDocumentDto ToDto(LegalDocument value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static LegalDocumentDto ToDto(LegalDocument value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (LegalDocumentDto)existing;
        var result = new LegalDocumentDto();
        seen[value] = result;
        result.Id = value.Id;
        result.CompanyId = value.CompanyId;
        result.DocumentTypeCode = value.DocumentTypeCode;
        result.DocumentNumber = value.DocumentNumber;
        result.IssueDate = value.IssueDate;
        result.IssuedBy = value.IssuedBy;
        result.ExpiryDate = value.ExpiryDate;
        result.StoredFileId = value.StoredFileId;
        result.Note = value.Note;
        return result;
    }

    public static LegalDocument ToEntity(LegalDocumentDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static LegalDocument ToEntity(LegalDocumentDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (LegalDocument)existing;
        var result = new LegalDocument();
        seen[value] = result;
        result.Id = value.Id;
        result.CompanyId = value.CompanyId;
        result.DocumentTypeCode = value.DocumentTypeCode;
        result.DocumentNumber = value.DocumentNumber;
        result.IssueDate = value.IssueDate;
        result.IssuedBy = value.IssuedBy;
        result.ExpiryDate = value.ExpiryDate;
        result.StoredFileId = value.StoredFileId;
        result.Note = value.Note;
        return result;
    }

    public static void CopyToDto(LegalDocument value, LegalDocumentDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.CompanyId = mapped.CompanyId;
        target.DocumentTypeCode = mapped.DocumentTypeCode;
        target.DocumentNumber = mapped.DocumentNumber;
        target.IssueDate = mapped.IssueDate;
        target.IssuedBy = mapped.IssuedBy;
        target.ExpiryDate = mapped.ExpiryDate;
        target.StoredFileId = mapped.StoredFileId;
        target.Note = mapped.Note;
    }

    public static StoredFileDto ToDto(StoredFile value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static StoredFileDto ToDto(StoredFile value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (StoredFileDto)existing;
        var result = new StoredFileDto();
        seen[value] = result;
        result.Id = value.Id;
        result.StorageName = value.StorageName;
        result.OriginalName = value.OriginalName;
        result.ContentType = value.ContentType;
        result.Size = value.Size;
        result.Sha256 = value.Sha256;
        result.ScanStatusCode = value.ScanStatusCode;
        result.CreatedAt = value.CreatedAt;
        result.CreatedByAccountId = value.CreatedByAccountId;
        result.IsDeleted = value.IsDeleted;
        return result;
    }

    public static StoredFile ToEntity(StoredFileDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static StoredFile ToEntity(StoredFileDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (StoredFile)existing;
        var result = new StoredFile();
        seen[value] = result;
        result.Id = value.Id;
        result.StorageName = value.StorageName;
        result.OriginalName = value.OriginalName;
        result.ContentType = value.ContentType;
        result.Size = value.Size;
        result.Sha256 = value.Sha256;
        result.ScanStatusCode = value.ScanStatusCode;
        result.CreatedAt = value.CreatedAt;
        result.CreatedByAccountId = value.CreatedByAccountId;
        result.IsDeleted = value.IsDeleted;
        return result;
    }

    public static void CopyToDto(StoredFile value, StoredFileDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.StorageName = mapped.StorageName;
        target.OriginalName = mapped.OriginalName;
        target.ContentType = mapped.ContentType;
        target.Size = mapped.Size;
        target.Sha256 = mapped.Sha256;
        target.ScanStatusCode = mapped.ScanStatusCode;
        target.CreatedAt = mapped.CreatedAt;
        target.CreatedByAccountId = mapped.CreatedByAccountId;
        target.IsDeleted = mapped.IsDeleted;
    }
}
