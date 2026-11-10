using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

internal static partial class DtoMapper
{
    public static ColumnMappingTemplateDto ToDto(ColumnMappingTemplate value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static ColumnMappingTemplateDto ToDto(ColumnMappingTemplate value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ColumnMappingTemplateDto)existing;
        var result = new ColumnMappingTemplateDto();
        seen[value] = result;
        result.Id = value.Id;
        result.TemplateName = value.TemplateName;
        result.CompanyId = value.CompanyId;
        result.MappingJson = value.MappingJson;
        result.CreatedBy = value.CreatedBy;
        result.CreatedAt = value.CreatedAt;
        result.UpdatedAt = value.UpdatedAt;
        return result;
    }

    public static ColumnMappingTemplate ToEntity(ColumnMappingTemplateDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static ColumnMappingTemplate ToEntity(ColumnMappingTemplateDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ColumnMappingTemplate)existing;
        var result = new ColumnMappingTemplate();
        seen[value] = result;
        result.Id = value.Id;
        result.TemplateName = value.TemplateName;
        result.CompanyId = value.CompanyId;
        result.MappingJson = value.MappingJson;
        result.CreatedBy = value.CreatedBy;
        result.CreatedAt = value.CreatedAt;
        result.UpdatedAt = value.UpdatedAt;
        return result;
    }

    public static void CopyToDto(ColumnMappingTemplate value, ColumnMappingTemplateDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.TemplateName = mapped.TemplateName;
        target.CompanyId = mapped.CompanyId;
        target.MappingJson = mapped.MappingJson;
        target.CreatedBy = mapped.CreatedBy;
        target.CreatedAt = mapped.CreatedAt;
        target.UpdatedAt = mapped.UpdatedAt;
    }

    public static ImportBackupDto ToDto(ImportBackup value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static ImportBackupDto ToDto(ImportBackup value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ImportBackupDto)existing;
        var result = new ImportBackupDto();
        seen[value] = result;
        result.Id = value.Id;
        result.ImportSessionId = value.ImportSessionId;
        result.ActionType = value.ActionType;
        result.EntityType = value.EntityType;
        result.EntityId = value.EntityId;
        result.OldData = value.OldData;
        result.CreatedAt = value.CreatedAt;
        return result;
    }

    public static ImportBackup ToEntity(ImportBackupDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static ImportBackup ToEntity(ImportBackupDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ImportBackup)existing;
        var result = new ImportBackup();
        seen[value] = result;
        result.Id = value.Id;
        result.ImportSessionId = value.ImportSessionId;
        result.ActionType = value.ActionType;
        result.EntityType = value.EntityType;
        result.EntityId = value.EntityId;
        result.OldData = value.OldData;
        result.CreatedAt = value.CreatedAt;
        return result;
    }

    public static void CopyToDto(ImportBackup value, ImportBackupDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.ImportSessionId = mapped.ImportSessionId;
        target.ActionType = mapped.ActionType;
        target.EntityType = mapped.EntityType;
        target.EntityId = mapped.EntityId;
        target.OldData = mapped.OldData;
        target.CreatedAt = mapped.CreatedAt;
    }

    public static ImportHistoryDto ToDto(ImportHistory value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static ImportHistoryDto ToDto(ImportHistory value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ImportHistoryDto)existing;
        var result = new ImportHistoryDto();
        seen[value] = result;
        result.Id = value.Id;
        result.SessionId = value.SessionId;
        result.FileName = value.FileName;
        result.CompanyId = value.CompanyId;
        result.CompanyName = value.CompanyName;
        result.TotalRows = value.TotalRows;
        result.AddedRows = value.AddedRows;
        result.UpdatedRows = value.UpdatedRows;
        result.ErrorRows = value.ErrorRows;
        result.Status = value.Status;
        result.Username = value.Username;
        result.ImportDate = value.ImportDate;
        result.ErrorDetails = value.ErrorDetails;
        return result;
    }

    public static ImportHistory ToEntity(ImportHistoryDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static ImportHistory ToEntity(ImportHistoryDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ImportHistory)existing;
        var result = new ImportHistory();
        seen[value] = result;
        result.Id = value.Id;
        result.SessionId = value.SessionId;
        result.FileName = value.FileName;
        result.CompanyId = value.CompanyId;
        result.CompanyName = value.CompanyName;
        result.TotalRows = value.TotalRows;
        result.AddedRows = value.AddedRows;
        result.UpdatedRows = value.UpdatedRows;
        result.ErrorRows = value.ErrorRows;
        result.Status = value.Status;
        result.Username = value.Username;
        result.ImportDate = value.ImportDate;
        result.ErrorDetails = value.ErrorDetails;
        return result;
    }

    public static void CopyToDto(ImportHistory value, ImportHistoryDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.SessionId = mapped.SessionId;
        target.FileName = mapped.FileName;
        target.CompanyId = mapped.CompanyId;
        target.CompanyName = mapped.CompanyName;
        target.TotalRows = mapped.TotalRows;
        target.AddedRows = mapped.AddedRows;
        target.UpdatedRows = mapped.UpdatedRows;
        target.ErrorRows = mapped.ErrorRows;
        target.Status = mapped.Status;
        target.Username = mapped.Username;
        target.ImportDate = mapped.ImportDate;
        target.ErrorDetails = mapped.ErrorDetails;
    }

    public static ImportPreviewRowDto ToDto(ImportPreviewRow value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static ImportPreviewRowDto ToDto(ImportPreviewRow value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ImportPreviewRowDto)existing;
        var result = new ImportPreviewRowDto();
        seen[value] = result;
        result.RowNumber = value.RowNumber;
        result.Status = value.Status;
        result.ErrorMessage = value.ErrorMessage;
        result.ExistingEmployeeId = value.ExistingEmployeeId;
        result.StaffName = value.StaffName;
        result.GenderDisplay = value.GenderDisplay;
        result.BirthdayDisplay = value.BirthdayDisplay;
        result.NationalityDisplay = value.NationalityDisplay;
        result.Passport = value.Passport;
        result.Address = value.Address;
        result.CareerDisplay = value.CareerDisplay;
        result.WorkPermitDisplay = value.WorkPermitDisplay;
        result.VisaNumber = value.VisaNumber;
        result.VisaSymbol = value.VisaSymbol;
        result.TemporaryStayDisplay = value.TemporaryStayDisplay;
        result.CompanyDisplay = value.CompanyDisplay;
        result.ResolvedCompanyId = value.ResolvedCompanyId;
        result.ParsedEmployee = value.ParsedEmployee is null ? null : ToDto(value.ParsedEmployee, seen);
        result.ExtendedFields = value.ExtendedFields;
        return result;
    }

    public static ImportPreviewRow ToEntity(ImportPreviewRowDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static ImportPreviewRow ToEntity(ImportPreviewRowDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ImportPreviewRow)existing;
        var result = new ImportPreviewRow();
        seen[value] = result;
        result.RowNumber = value.RowNumber;
        result.Status = value.Status;
        result.ErrorMessage = value.ErrorMessage;
        result.ExistingEmployeeId = value.ExistingEmployeeId;
        result.StaffName = value.StaffName;
        result.GenderDisplay = value.GenderDisplay;
        result.BirthdayDisplay = value.BirthdayDisplay;
        result.NationalityDisplay = value.NationalityDisplay;
        result.Passport = value.Passport;
        result.Address = value.Address;
        result.CareerDisplay = value.CareerDisplay;
        result.WorkPermitDisplay = value.WorkPermitDisplay;
        result.VisaNumber = value.VisaNumber;
        result.VisaSymbol = value.VisaSymbol;
        result.TemporaryStayDisplay = value.TemporaryStayDisplay;
        result.CompanyDisplay = value.CompanyDisplay;
        result.ResolvedCompanyId = value.ResolvedCompanyId;
        result.ParsedEmployee = value.ParsedEmployee is null ? null : ToEntity(value.ParsedEmployee, seen);
        result.ExtendedFields = value.ExtendedFields;
        return result;
    }

    public static void CopyToDto(ImportPreviewRow value, ImportPreviewRowDto target)
    {
        var mapped = ToDto(value);
        target.RowNumber = mapped.RowNumber;
        target.Status = mapped.Status;
        target.ErrorMessage = mapped.ErrorMessage;
        target.ExistingEmployeeId = mapped.ExistingEmployeeId;
        target.StaffName = mapped.StaffName;
        target.GenderDisplay = mapped.GenderDisplay;
        target.BirthdayDisplay = mapped.BirthdayDisplay;
        target.NationalityDisplay = mapped.NationalityDisplay;
        target.Passport = mapped.Passport;
        target.Address = mapped.Address;
        target.CareerDisplay = mapped.CareerDisplay;
        target.WorkPermitDisplay = mapped.WorkPermitDisplay;
        target.VisaNumber = mapped.VisaNumber;
        target.VisaSymbol = mapped.VisaSymbol;
        target.TemporaryStayDisplay = mapped.TemporaryStayDisplay;
        target.CompanyDisplay = mapped.CompanyDisplay;
        target.ResolvedCompanyId = mapped.ResolvedCompanyId;
        target.ParsedEmployee = mapped.ParsedEmployee;
        target.ExtendedFields = mapped.ExtendedFields;
    }
}
