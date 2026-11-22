using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

internal static partial class DtoMapper
{
    public static AccountDto ToDto(Account value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static AccountDto ToDto(Account value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (AccountDto)existing;
        var result = new AccountDto();
        seen[value] = result;
        result.IDUser = value.IDUser;
        result.Username = value.Username;
        result.Name = value.Name;
        result.Password = value.Password;
        result.Permission = value.Permission;
        result.Delete_flag = value.Delete_flag;
        return result;
    }

    public static Account ToEntity(AccountDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static Account ToEntity(AccountDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (Account)existing;
        var result = new Account();
        seen[value] = result;
        result.IDUser = value.IDUser;
        result.Username = value.Username;
        result.Name = value.Name;
        result.Password = value.Password;
        result.Permission = value.Permission;
        result.Delete_flag = value.Delete_flag;
        return result;
    }

    public static void CopyToDto(Account value, AccountDto target)
    {
        var mapped = ToDto(value);
        target.IDUser = mapped.IDUser;
        target.Username = mapped.Username;
        target.Name = mapped.Name;
        target.Password = mapped.Password;
        target.Permission = mapped.Permission;
        target.Delete_flag = mapped.Delete_flag;
    }

    public static AuditLogDto ToDto(AuditLog value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static AuditLogDto ToDto(AuditLog value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (AuditLogDto)existing;
        var result = new AuditLogDto();
        seen[value] = result;
        result.Id = value.Id;
        result.Action = value.Action;
        result.EntityType = value.EntityType;
        result.EntityId = value.EntityId;
        result.Description = value.Description;
        result.ChangesJson = value.ChangesJson;
        result.Username = value.Username;
        result.Timestamp = value.Timestamp;
        result.IpAddress = value.IpAddress;
        return result;
    }

    public static AuditLog ToEntity(AuditLogDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static AuditLog ToEntity(AuditLogDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (AuditLog)existing;
        var result = new AuditLog();
        seen[value] = result;
        result.Id = value.Id;
        result.Action = value.Action;
        result.EntityType = value.EntityType;
        result.EntityId = value.EntityId;
        result.Description = value.Description;
        result.ChangesJson = value.ChangesJson;
        result.Username = value.Username;
        result.Timestamp = value.Timestamp;
        result.IpAddress = value.IpAddress;
        return result;
    }

    public static void CopyToDto(AuditLog value, AuditLogDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.Action = mapped.Action;
        target.EntityType = mapped.EntityType;
        target.EntityId = mapped.EntityId;
        target.Description = mapped.Description;
        target.ChangesJson = mapped.ChangesJson;
        target.Username = mapped.Username;
        target.Timestamp = mapped.Timestamp;
        target.IpAddress = mapped.IpAddress;
    }

    public static CareerDto ToDto(Career value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static CareerDto ToDto(Career value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (CareerDto)existing;
        var result = new CareerDto();
        seen[value] = result;
        result.IDCareer = value.IDCareer;
        result.CareerName = value.CareerName;
        result.IDCG = value.IDCG;
        result.Delete_flag = value.Delete_flag;
        result.CareerGroup = value.CareerGroup is null ? null : ToDto(value.CareerGroup, seen);
        return result;
    }

    public static Career ToEntity(CareerDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static Career ToEntity(CareerDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (Career)existing;
        var result = new Career();
        seen[value] = result;
        result.IDCareer = value.IDCareer;
        result.CareerName = value.CareerName;
        result.IDCG = value.IDCG;
        result.Delete_flag = value.Delete_flag;
        result.CareerGroup = value.CareerGroup is null ? null : ToEntity(value.CareerGroup, seen);
        return result;
    }

    public static void CopyToDto(Career value, CareerDto target)
    {
        var mapped = ToDto(value);
        target.IDCareer = mapped.IDCareer;
        target.CareerName = mapped.CareerName;
        target.IDCG = mapped.IDCG;
        target.Delete_flag = mapped.Delete_flag;
        target.CareerGroup = mapped.CareerGroup;
    }

    public static CareerGroupDto ToDto(CareerGroup value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static CareerGroupDto ToDto(CareerGroup value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (CareerGroupDto)existing;
        var result = new CareerGroupDto();
        seen[value] = result;
        result.IDCG = value.IDCG;
        result.CareerGroupName = value.CareerGroupName;
        result.Careers = value.Careers.Select(item0 => ToDto(item0, seen)).ToList();
        return result;
    }

    public static CareerGroup ToEntity(CareerGroupDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static CareerGroup ToEntity(CareerGroupDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (CareerGroup)existing;
        var result = new CareerGroup();
        seen[value] = result;
        result.IDCG = value.IDCG;
        result.CareerGroupName = value.CareerGroupName;
        result.Careers = value.Careers.Select(item0 => ToEntity(item0, seen)).ToList();
        return result;
    }

    public static void CopyToDto(CareerGroup value, CareerGroupDto target)
    {
        var mapped = ToDto(value);
        target.IDCG = mapped.IDCG;
        target.CareerGroupName = mapped.CareerGroupName;
        target.Careers = mapped.Careers;
    }

    public static DistrictDto ToDto(District value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static DistrictDto ToDto(District value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (DistrictDto)existing;
        var result = new DistrictDto();
        seen[value] = result;
        result.IDDistrict = value.IDDistrict;
        result.DisTrictName = value.DisTrictName;
        result.Delete_flag = value.Delete_flag;
        return result;
    }

    public static District ToEntity(DistrictDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static District ToEntity(DistrictDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (District)existing;
        var result = new District();
        seen[value] = result;
        result.IDDistrict = value.IDDistrict;
        result.DisTrictName = value.DisTrictName;
        result.Delete_flag = value.Delete_flag;
        return result;
    }

    public static void CopyToDto(District value, DistrictDto target)
    {
        var mapped = ToDto(value);
        target.IDDistrict = mapped.IDDistrict;
        target.DisTrictName = mapped.DisTrictName;
        target.Delete_flag = mapped.Delete_flag;
    }

    public static EconomicZoneDto ToDto(EconomicZone value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static EconomicZoneDto ToDto(EconomicZone value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (EconomicZoneDto)existing;
        var result = new EconomicZoneDto();
        seen[value] = result;
        result.Id = value.Id;
        result.Code = value.Code;
        result.Name = value.Name;
        result.TypeCode = value.TypeCode;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.IsDeleted = value.IsDeleted;
        result.SiteMemberships = value.SiteMemberships.Select(item0 => ToDto(item0, seen)).ToList();
        return result;
    }

    public static EconomicZone ToEntity(EconomicZoneDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static EconomicZone ToEntity(EconomicZoneDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (EconomicZone)existing;
        var result = new EconomicZone();
        seen[value] = result;
        result.Id = value.Id;
        result.Code = value.Code;
        result.Name = value.Name;
        result.TypeCode = value.TypeCode;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.IsDeleted = value.IsDeleted;
        result.SiteMemberships = value.SiteMemberships.Select(item0 => ToEntity(item0, seen)).ToList();
        return result;
    }

    public static void CopyToDto(EconomicZone value, EconomicZoneDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.Code = mapped.Code;
        target.Name = mapped.Name;
        target.TypeCode = mapped.TypeCode;
        target.ValidFrom = mapped.ValidFrom;
        target.ValidTo = mapped.ValidTo;
        target.IsDeleted = mapped.IsDeleted;
        target.SiteMemberships = mapped.SiteMemberships;
    }

    public static FieldDto ToDto(Field value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static FieldDto ToDto(Field value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (FieldDto)existing;
        var result = new FieldDto();
        seen[value] = result;
        result.IDField = value.IDField;
        result.FieldName = value.FieldName;
        result.Description = value.Description;
        result.Delete_flag = value.Delete_flag;
        result.Companies = value.Companies.Select(item0 => ToDto(item0, seen)).ToList();
        return result;
    }

    public static Field ToEntity(FieldDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static Field ToEntity(FieldDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (Field)existing;
        var result = new Field();
        seen[value] = result;
        result.IDField = value.IDField;
        result.FieldName = value.FieldName;
        result.Description = value.Description;
        result.Delete_flag = value.Delete_flag;
        result.Companies = value.Companies.Select(item0 => ToEntity(item0, seen)).ToList();
        return result;
    }

    public static void CopyToDto(Field value, FieldDto target)
    {
        var mapped = ToDto(value);
        target.IDField = mapped.IDField;
        target.FieldName = mapped.FieldName;
        target.Description = mapped.Description;
        target.Delete_flag = mapped.Delete_flag;
        target.Companies = mapped.Companies;
    }

    public static LegacyChangeEventDto ToDto(LegacyChangeEvent value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static LegacyChangeEventDto ToDto(LegacyChangeEvent value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (LegacyChangeEventDto)existing;
        var result = new LegacyChangeEventDto();
        seen[value] = result;
        result.Id = value.Id;
        result.EntityType = value.EntityType;
        result.EntityId = value.EntityId;
        result.Operation = value.Operation;
        result.BeforeXml = value.BeforeXml;
        result.AfterXml = value.AfterXml;
        result.OccurredAt = value.OccurredAt;
        result.ProcessedAt = value.ProcessedAt;
        result.RetryCount = value.RetryCount;
        result.StatusCode = value.StatusCode;
        result.LastError = value.LastError;
        return result;
    }

    public static LegacyChangeEvent ToEntity(LegacyChangeEventDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static LegacyChangeEvent ToEntity(LegacyChangeEventDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (LegacyChangeEvent)existing;
        var result = new LegacyChangeEvent();
        seen[value] = result;
        result.Id = value.Id;
        result.EntityType = value.EntityType;
        result.EntityId = value.EntityId;
        result.Operation = value.Operation;
        result.BeforeXml = value.BeforeXml;
        result.AfterXml = value.AfterXml;
        result.OccurredAt = value.OccurredAt;
        result.ProcessedAt = value.ProcessedAt;
        result.RetryCount = value.RetryCount;
        result.StatusCode = value.StatusCode;
        result.LastError = value.LastError;
        return result;
    }

    public static void CopyToDto(LegacyChangeEvent value, LegacyChangeEventDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.EntityType = mapped.EntityType;
        target.EntityId = mapped.EntityId;
        target.Operation = mapped.Operation;
        target.BeforeXml = mapped.BeforeXml;
        target.AfterXml = mapped.AfterXml;
        target.OccurredAt = mapped.OccurredAt;
        target.ProcessedAt = mapped.ProcessedAt;
        target.RetryCount = mapped.RetryCount;
        target.StatusCode = mapped.StatusCode;
        target.LastError = mapped.LastError;
    }

    public static MigrationIssueDto ToDto(MigrationIssue value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static MigrationIssueDto ToDto(MigrationIssue value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (MigrationIssueDto)existing;
        var result = new MigrationIssueDto();
        seen[value] = result;
        result.Id = value.Id;
        result.SourceType = value.SourceType;
        result.SourceId = value.SourceId;
        result.IssueCode = value.IssueCode;
        result.Description = value.Description;
        result.StatusCode = value.StatusCode;
        result.CreatedAt = value.CreatedAt;
        result.ResolvedAt = value.ResolvedAt;
        return result;
    }

    public static MigrationIssue ToEntity(MigrationIssueDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static MigrationIssue ToEntity(MigrationIssueDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (MigrationIssue)existing;
        var result = new MigrationIssue();
        seen[value] = result;
        result.Id = value.Id;
        result.SourceType = value.SourceType;
        result.SourceId = value.SourceId;
        result.IssueCode = value.IssueCode;
        result.Description = value.Description;
        result.StatusCode = value.StatusCode;
        result.CreatedAt = value.CreatedAt;
        result.ResolvedAt = value.ResolvedAt;
        return result;
    }

    public static void CopyToDto(MigrationIssue value, MigrationIssueDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.SourceType = mapped.SourceType;
        target.SourceId = mapped.SourceId;
        target.IssueCode = mapped.IssueCode;
        target.Description = mapped.Description;
        target.StatusCode = mapped.StatusCode;
        target.CreatedAt = mapped.CreatedAt;
        target.ResolvedAt = mapped.ResolvedAt;
    }

    public static NationalityEntityDto ToDto(NationalityEntity value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static NationalityEntityDto ToDto(NationalityEntity value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (NationalityEntityDto)existing;
        var result = new NationalityEntityDto();
        seen[value] = result;
        result.IDNationality = value.IDNationality;
        result.NationalityCode = value.NationalityCode;
        result.NationalityName = value.NationalityName;
        result.Delete_flag = value.Delete_flag;
        return result;
    }

    public static NationalityEntity ToEntity(NationalityEntityDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static NationalityEntity ToEntity(NationalityEntityDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (NationalityEntity)existing;
        var result = new NationalityEntity();
        seen[value] = result;
        result.IDNationality = value.IDNationality;
        result.NationalityCode = value.NationalityCode;
        result.NationalityName = value.NationalityName;
        result.Delete_flag = value.Delete_flag;
        return result;
    }

    public static void CopyToDto(NationalityEntity value, NationalityEntityDto target)
    {
        var mapped = ToDto(value);
        target.IDNationality = mapped.IDNationality;
        target.NationalityCode = mapped.NationalityCode;
        target.NationalityName = mapped.NationalityName;
        target.Delete_flag = mapped.Delete_flag;
    }

    public static SchemaVersionDto ToDto(SchemaVersion value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static SchemaVersionDto ToDto(SchemaVersion value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (SchemaVersionDto)existing;
        var result = new SchemaVersionDto();
        seen[value] = result;
        result.Id = value.Id;
        result.Version = value.Version;
        result.Description = value.Description;
        result.Checksum = value.Checksum;
        result.AppliedAt = value.AppliedAt;
        result.AppliedBy = value.AppliedBy;
        return result;
    }

    public static SchemaVersion ToEntity(SchemaVersionDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static SchemaVersion ToEntity(SchemaVersionDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (SchemaVersion)existing;
        var result = new SchemaVersion();
        seen[value] = result;
        result.Id = value.Id;
        result.Version = value.Version;
        result.Description = value.Description;
        result.Checksum = value.Checksum;
        result.AppliedAt = value.AppliedAt;
        result.AppliedBy = value.AppliedBy;
        return result;
    }

    public static void CopyToDto(SchemaVersion value, SchemaVersionDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.Version = mapped.Version;
        target.Description = mapped.Description;
        target.Checksum = mapped.Checksum;
        target.AppliedAt = mapped.AppliedAt;
        target.AppliedBy = mapped.AppliedBy;
    }

    public static WardDto ToDto(Ward value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static WardDto ToDto(Ward value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (WardDto)existing;
        var result = new WardDto();
        seen[value] = result;
        result.IDWard = value.IDWard;
        result.WardName = value.WardName;
        result.Delete_flag = value.Delete_flag;
        return result;
    }

    public static Ward ToEntity(WardDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static Ward ToEntity(WardDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (Ward)existing;
        var result = new Ward();
        seen[value] = result;
        result.IDWard = value.IDWard;
        result.WardName = value.WardName;
        result.Delete_flag = value.Delete_flag;
        return result;
    }

    public static void CopyToDto(Ward value, WardDto target)
    {
        var mapped = ToDto(value);
        target.IDWard = mapped.IDWard;
        target.WardName = mapped.WardName;
        target.Delete_flag = mapped.Delete_flag;
    }

    public static WebCredentialDto ToDto(WebCredential value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static WebCredentialDto ToDto(WebCredential value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (WebCredentialDto)existing;
        var result = new WebCredentialDto();
        seen[value] = result;
        result.AccountId = value.AccountId;
        result.PasswordHash = value.PasswordHash;
        result.MustChangePassword = value.MustChangePassword;
        result.FailedLoginCount = value.FailedLoginCount;
        result.LockedUntil = value.LockedUntil;
        result.UpdatedAt = value.UpdatedAt;
        result.Account = value.Account is null ? null : ToDto(value.Account, seen);
        return result;
    }

    public static WebCredential ToEntity(WebCredentialDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static WebCredential ToEntity(WebCredentialDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (WebCredential)existing;
        var result = new WebCredential();
        seen[value] = result;
        result.AccountId = value.AccountId;
        result.PasswordHash = value.PasswordHash;
        result.MustChangePassword = value.MustChangePassword;
        result.FailedLoginCount = value.FailedLoginCount;
        result.LockedUntil = value.LockedUntil;
        result.UpdatedAt = value.UpdatedAt;
        result.Account = value.Account is null ? null : ToEntity(value.Account, seen);
        return result;
    }

    public static void CopyToDto(WebCredential value, WebCredentialDto target)
    {
        var mapped = ToDto(value);
        target.AccountId = mapped.AccountId;
        target.PasswordHash = mapped.PasswordHash;
        target.MustChangePassword = mapped.MustChangePassword;
        target.FailedLoginCount = mapped.FailedLoginCount;
        target.LockedUntil = mapped.LockedUntil;
        target.UpdatedAt = mapped.UpdatedAt;
        target.Account = mapped.Account;
    }

    public static WebRoleAssignmentDto ToDto(WebRoleAssignment value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static WebRoleAssignmentDto ToDto(WebRoleAssignment value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (WebRoleAssignmentDto)existing;
        var result = new WebRoleAssignmentDto();
        seen[value] = result;
        result.Id = value.Id;
        result.AccountId = value.AccountId;
        result.RoleCode = value.RoleCode;
        result.AdministrativeUnitId = value.AdministrativeUnitId;
        result.Account = value.Account is null ? null : ToDto(value.Account, seen);
        result.AdministrativeUnit = value.AdministrativeUnit is null ? null : ToDto(value.AdministrativeUnit, seen);
        return result;
    }

    public static WebRoleAssignment ToEntity(WebRoleAssignmentDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static WebRoleAssignment ToEntity(WebRoleAssignmentDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (WebRoleAssignment)existing;
        var result = new WebRoleAssignment();
        seen[value] = result;
        result.Id = value.Id;
        result.AccountId = value.AccountId;
        result.RoleCode = value.RoleCode;
        result.AdministrativeUnitId = value.AdministrativeUnitId;
        result.Account = value.Account is null ? null : ToEntity(value.Account, seen);
        result.AdministrativeUnit = value.AdministrativeUnit is null ? null : ToEntity(value.AdministrativeUnit, seen);
        return result;
    }

    public static void CopyToDto(WebRoleAssignment value, WebRoleAssignmentDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.AccountId = mapped.AccountId;
        target.RoleCode = mapped.RoleCode;
        target.AdministrativeUnitId = mapped.AdministrativeUnitId;
        target.Account = mapped.Account;
        target.AdministrativeUnit = mapped.AdministrativeUnit;
    }
}
