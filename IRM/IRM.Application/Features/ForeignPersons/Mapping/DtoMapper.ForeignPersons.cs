using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

internal static partial class DtoMapper
{
    public static ElectronicIdentityDto ToDto(ElectronicIdentity value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static ElectronicIdentityDto ToDto(ElectronicIdentity value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ElectronicIdentityDto)existing;
        var result = new ElectronicIdentityDto();
        seen[value] = result;
        result.Id = value.Id;
        result.ForeignPersonId = value.ForeignPersonId;
        result.IdentityNumber = value.IdentityNumber;
        result.IdentitySearchKey = value.IdentitySearchKey;
        result.IssuedAt = value.IssuedAt;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.StatusCode = value.StatusCode;
        result.ForeignPerson = value.ForeignPerson is null ? null : ToDto(value.ForeignPerson, seen);
        return result;
    }

    public static ElectronicIdentity ToEntity(ElectronicIdentityDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static ElectronicIdentity ToEntity(ElectronicIdentityDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ElectronicIdentity)existing;
        var result = new ElectronicIdentity();
        seen[value] = result;
        result.Id = value.Id;
        result.ForeignPersonId = value.ForeignPersonId;
        result.IdentityNumber = value.IdentityNumber;
        result.IdentitySearchKey = value.IdentitySearchKey;
        result.IssuedAt = value.IssuedAt;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.StatusCode = value.StatusCode;
        result.ForeignPerson = value.ForeignPerson is null ? null : ToEntity(value.ForeignPerson, seen);
        return result;
    }

    public static void CopyToDto(ElectronicIdentity value, ElectronicIdentityDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.ForeignPersonId = mapped.ForeignPersonId;
        target.IdentityNumber = mapped.IdentityNumber;
        target.IdentitySearchKey = mapped.IdentitySearchKey;
        target.IssuedAt = mapped.IssuedAt;
        target.ValidFrom = mapped.ValidFrom;
        target.ValidTo = mapped.ValidTo;
        target.StatusCode = mapped.StatusCode;
        target.ForeignPerson = mapped.ForeignPerson;
    }

    public static FamilyVisitDetailDto ToDto(FamilyVisitDetail value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static FamilyVisitDetailDto ToDto(FamilyVisitDetail value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (FamilyVisitDetailDto)existing;
        var result = new FamilyVisitDetailDto();
        seen[value] = result;
        result.StayCaseId = value.StayCaseId;
        result.RelativeName = value.RelativeName;
        result.RelativeIdNumber = value.RelativeIdNumber;
        result.RelativePhone = value.RelativePhone;
        result.RelativeAddress = value.RelativeAddress;
        result.Relationship = value.Relationship;
        result.RelativeTypeCode = value.RelativeTypeCode;
        result.RelativeNationalityCode = value.RelativeNationalityCode;
        result.StayCase = value.StayCase is null ? null : ToDto(value.StayCase, seen);
        return result;
    }

    public static FamilyVisitDetail ToEntity(FamilyVisitDetailDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static FamilyVisitDetail ToEntity(FamilyVisitDetailDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (FamilyVisitDetail)existing;
        var result = new FamilyVisitDetail();
        seen[value] = result;
        result.StayCaseId = value.StayCaseId;
        result.RelativeName = value.RelativeName;
        result.RelativeIdNumber = value.RelativeIdNumber;
        result.RelativePhone = value.RelativePhone;
        result.RelativeAddress = value.RelativeAddress;
        result.Relationship = value.Relationship;
        result.RelativeTypeCode = value.RelativeTypeCode;
        result.RelativeNationalityCode = value.RelativeNationalityCode;
        result.StayCase = value.StayCase is null ? null : ToEntity(value.StayCase, seen);
        return result;
    }

    public static void CopyToDto(FamilyVisitDetail value, FamilyVisitDetailDto target)
    {
        var mapped = ToDto(value);
        target.StayCaseId = mapped.StayCaseId;
        target.RelativeName = mapped.RelativeName;
        target.RelativeIdNumber = mapped.RelativeIdNumber;
        target.RelativePhone = mapped.RelativePhone;
        target.RelativeAddress = mapped.RelativeAddress;
        target.Relationship = mapped.Relationship;
        target.RelativeTypeCode = mapped.RelativeTypeCode;
        target.RelativeNationalityCode = mapped.RelativeNationalityCode;
        target.StayCase = mapped.StayCase;
    }

    public static FamilyVisitorDto ToDto(FamilyVisitor value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static FamilyVisitorDto ToDto(FamilyVisitor value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (FamilyVisitorDto)existing;
        var result = new FamilyVisitorDto();
        seen[value] = result;
        result.ForeignPersonId = value.ForeignPersonId;
        result.StayCaseId = value.StayCaseId;
        result.FullName = value.FullName;
        result.Gender = value.Gender;
        result.Birthday = value.Birthday;
        result.Nationality = value.Nationality;
        result.Passport = value.Passport;
        result.Address = value.Address;
        result.RelativeName = value.RelativeName;
        result.RelativeIdCard = value.RelativeIdCard;
        result.RelativePhone = value.RelativePhone;
        result.RelativeAddress = value.RelativeAddress;
        result.Relationship = value.Relationship;
        result.RelativeTypeCode = value.RelativeTypeCode;
        result.RelativeNationality = value.RelativeNationality;
        result.SponsorForeignPersonId = value.SponsorForeignPersonId;
        result.SponsorCompanyId = value.SponsorCompanyId;
        result.VisitStartDate = value.VisitStartDate;
        result.VisitEndDate = value.VisitEndDate;
        result.ElectronicIdNumber = value.ElectronicIdNumber;
        result.Note = value.Note;
        return result;
    }

    public static FamilyVisitor ToEntity(FamilyVisitorDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static FamilyVisitor ToEntity(FamilyVisitorDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (FamilyVisitor)existing;
        var result = new FamilyVisitor();
        seen[value] = result;
        result.ForeignPersonId = value.ForeignPersonId;
        result.StayCaseId = value.StayCaseId;
        result.FullName = value.FullName;
        result.Gender = value.Gender;
        result.Birthday = value.Birthday;
        result.Nationality = value.Nationality;
        result.Passport = value.Passport;
        result.Address = value.Address;
        result.RelativeName = value.RelativeName;
        result.RelativeIdCard = value.RelativeIdCard;
        result.RelativePhone = value.RelativePhone;
        result.RelativeAddress = value.RelativeAddress;
        result.Relationship = value.Relationship;
        result.RelativeTypeCode = value.RelativeTypeCode;
        result.RelativeNationality = value.RelativeNationality;
        result.SponsorForeignPersonId = value.SponsorForeignPersonId;
        result.SponsorCompanyId = value.SponsorCompanyId;
        result.VisitStartDate = value.VisitStartDate;
        result.VisitEndDate = value.VisitEndDate;
        result.ElectronicIdNumber = value.ElectronicIdNumber;
        result.Note = value.Note;
        return result;
    }

    public static void CopyToDto(FamilyVisitor value, FamilyVisitorDto target)
    {
        var mapped = ToDto(value);
        target.ForeignPersonId = mapped.ForeignPersonId;
        target.StayCaseId = mapped.StayCaseId;
        target.FullName = mapped.FullName;
        target.Gender = mapped.Gender;
        target.Birthday = mapped.Birthday;
        target.Nationality = mapped.Nationality;
        target.Passport = mapped.Passport;
        target.Address = mapped.Address;
        target.RelativeName = mapped.RelativeName;
        target.RelativeIdCard = mapped.RelativeIdCard;
        target.RelativePhone = mapped.RelativePhone;
        target.RelativeAddress = mapped.RelativeAddress;
        target.Relationship = mapped.Relationship;
        target.RelativeTypeCode = mapped.RelativeTypeCode;
        target.RelativeNationality = mapped.RelativeNationality;
        target.SponsorForeignPersonId = mapped.SponsorForeignPersonId;
        target.SponsorCompanyId = mapped.SponsorCompanyId;
        target.VisitStartDate = mapped.VisitStartDate;
        target.VisitEndDate = mapped.VisitEndDate;
        target.ElectronicIdNumber = mapped.ElectronicIdNumber;
        target.Note = mapped.Note;
    }

    public static ForeignPersonDto ToDto(ForeignPerson value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static ForeignPersonDto ToDto(ForeignPerson value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ForeignPersonDto)existing;
        var result = new ForeignPersonDto();
        seen[value] = result;
        result.Id = value.Id;
        result.FullName = value.FullName;
        result.Gender = value.Gender;
        result.Birthday = value.Birthday;
        result.NationalityCode = value.NationalityCode;
        result.PassportNumber = value.PassportNumber;
        result.PassportSearchKey = value.PassportSearchKey;
        result.IsDataIncomplete = value.IsDataIncomplete;
        result.IsDeleted = value.IsDeleted;
        result.CreatedAt = value.CreatedAt;
        result.UpdatedAt = value.UpdatedAt;
        result.Nationality = value.Nationality is null ? null : ToDto(value.Nationality, seen);
        result.SourceLinks = value.SourceLinks.Select(item0 => ToDto(item0, seen)).ToList();
        result.StayCases = value.StayCases.Select(item0 => ToDto(item0, seen)).ToList();
        result.ResidencePeriods = value.ResidencePeriods.Select(item0 => ToDto(item0, seen)).ToList();
        result.ImmigrationDocuments = value.ImmigrationDocuments.Select(item0 => ToDto(item0, seen)).ToList();
        result.ElectronicIdentities = value.ElectronicIdentities.Select(item0 => ToDto(item0, seen)).ToList();
        return result;
    }

    public static ForeignPerson ToEntity(ForeignPersonDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static ForeignPerson ToEntity(ForeignPersonDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ForeignPerson)existing;
        var result = new ForeignPerson();
        seen[value] = result;
        result.Id = value.Id;
        result.FullName = value.FullName;
        result.Gender = value.Gender;
        result.Birthday = value.Birthday;
        result.NationalityCode = value.NationalityCode;
        result.PassportNumber = value.PassportNumber;
        result.PassportSearchKey = value.PassportSearchKey;
        result.IsDataIncomplete = value.IsDataIncomplete;
        result.IsDeleted = value.IsDeleted;
        result.CreatedAt = value.CreatedAt;
        result.UpdatedAt = value.UpdatedAt;
        result.Nationality = value.Nationality is null ? null : ToEntity(value.Nationality, seen);
        result.SourceLinks = value.SourceLinks.Select(item0 => ToEntity(item0, seen)).ToList();
        result.StayCases = value.StayCases.Select(item0 => ToEntity(item0, seen)).ToList();
        result.ResidencePeriods = value.ResidencePeriods.Select(item0 => ToEntity(item0, seen)).ToList();
        result.ImmigrationDocuments = value.ImmigrationDocuments.Select(item0 => ToEntity(item0, seen)).ToList();
        result.ElectronicIdentities = value.ElectronicIdentities.Select(item0 => ToEntity(item0, seen)).ToList();
        return result;
    }

    public static void CopyToDto(ForeignPerson value, ForeignPersonDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.FullName = mapped.FullName;
        target.Gender = mapped.Gender;
        target.Birthday = mapped.Birthday;
        target.NationalityCode = mapped.NationalityCode;
        target.PassportNumber = mapped.PassportNumber;
        target.PassportSearchKey = mapped.PassportSearchKey;
        target.IsDataIncomplete = mapped.IsDataIncomplete;
        target.IsDeleted = mapped.IsDeleted;
        target.CreatedAt = mapped.CreatedAt;
        target.UpdatedAt = mapped.UpdatedAt;
        target.Nationality = mapped.Nationality;
        target.SourceLinks = mapped.SourceLinks;
        target.StayCases = mapped.StayCases;
        target.ResidencePeriods = mapped.ResidencePeriods;
        target.ImmigrationDocuments = mapped.ImmigrationDocuments;
        target.ElectronicIdentities = mapped.ElectronicIdentities;
    }

    public static ForeignPersonSourceLinkDto ToDto(ForeignPersonSourceLink value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static ForeignPersonSourceLinkDto ToDto(ForeignPersonSourceLink value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ForeignPersonSourceLinkDto)existing;
        var result = new ForeignPersonSourceLinkDto();
        seen[value] = result;
        result.Id = value.Id;
        result.ForeignPersonId = value.ForeignPersonId;
        result.SourceType = value.SourceType;
        result.SourceId = value.SourceId;
        result.SourceFingerprint = value.SourceFingerprint;
        result.LastSynchronizedAt = value.LastSynchronizedAt;
        result.ForeignPerson = value.ForeignPerson is null ? null : ToDto(value.ForeignPerson, seen);
        return result;
    }

    public static ForeignPersonSourceLink ToEntity(ForeignPersonSourceLinkDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static ForeignPersonSourceLink ToEntity(ForeignPersonSourceLinkDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ForeignPersonSourceLink)existing;
        var result = new ForeignPersonSourceLink();
        seen[value] = result;
        result.Id = value.Id;
        result.ForeignPersonId = value.ForeignPersonId;
        result.SourceType = value.SourceType;
        result.SourceId = value.SourceId;
        result.SourceFingerprint = value.SourceFingerprint;
        result.LastSynchronizedAt = value.LastSynchronizedAt;
        result.ForeignPerson = value.ForeignPerson is null ? null : ToEntity(value.ForeignPerson, seen);
        return result;
    }

    public static void CopyToDto(ForeignPersonSourceLink value, ForeignPersonSourceLinkDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.ForeignPersonId = mapped.ForeignPersonId;
        target.SourceType = mapped.SourceType;
        target.SourceId = mapped.SourceId;
        target.SourceFingerprint = mapped.SourceFingerprint;
        target.LastSynchronizedAt = mapped.LastSynchronizedAt;
        target.ForeignPerson = mapped.ForeignPerson;
    }

    public static ImmigrationDocumentDto ToDto(ImmigrationDocument value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static ImmigrationDocumentDto ToDto(ImmigrationDocument value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ImmigrationDocumentDto)existing;
        var result = new ImmigrationDocumentDto();
        seen[value] = result;
        result.Id = value.Id;
        result.ForeignPersonId = value.ForeignPersonId;
        result.TypeCode = value.TypeCode;
        result.Number = value.Number;
        result.IssuedAt = value.IssuedAt;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.IssuedBy = value.IssuedBy;
        result.StatusCode = value.StatusCode;
        result.Note = value.Note;
        result.ForeignPerson = value.ForeignPerson is null ? null : ToDto(value.ForeignPerson, seen);
        return result;
    }

    public static ImmigrationDocument ToEntity(ImmigrationDocumentDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static ImmigrationDocument ToEntity(ImmigrationDocumentDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ImmigrationDocument)existing;
        var result = new ImmigrationDocument();
        seen[value] = result;
        result.Id = value.Id;
        result.ForeignPersonId = value.ForeignPersonId;
        result.TypeCode = value.TypeCode;
        result.Number = value.Number;
        result.IssuedAt = value.IssuedAt;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.IssuedBy = value.IssuedBy;
        result.StatusCode = value.StatusCode;
        result.Note = value.Note;
        result.ForeignPerson = value.ForeignPerson is null ? null : ToEntity(value.ForeignPerson, seen);
        return result;
    }

    public static void CopyToDto(ImmigrationDocument value, ImmigrationDocumentDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.ForeignPersonId = mapped.ForeignPersonId;
        target.TypeCode = mapped.TypeCode;
        target.Number = mapped.Number;
        target.IssuedAt = mapped.IssuedAt;
        target.ValidFrom = mapped.ValidFrom;
        target.ValidTo = mapped.ValidTo;
        target.IssuedBy = mapped.IssuedBy;
        target.StatusCode = mapped.StatusCode;
        target.Note = mapped.Note;
        target.ForeignPerson = mapped.ForeignPerson;
    }

    public static StayCaseDto ToDto(StayCase value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static StayCaseDto ToDto(StayCase value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (StayCaseDto)existing;
        var result = new StayCaseDto();
        seen[value] = result;
        result.Id = value.Id;
        result.ForeignPersonId = value.ForeignPersonId;
        result.PurposeCode = value.PurposeCode;
        result.IsPrimary = value.IsPrimary;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.SponsorCompanyId = value.SponsorCompanyId;
        result.SponsorForeignPersonId = value.SponsorForeignPersonId;
        result.StatusCode = value.StatusCode;
        result.Note = value.Note;
        result.CreatedAt = value.CreatedAt;
        result.UpdatedAt = value.UpdatedAt;
        result.ForeignPerson = value.ForeignPerson is null ? null : ToDto(value.ForeignPerson, seen);
        result.SponsorCompany = value.SponsorCompany is null ? null : ToDto(value.SponsorCompany, seen);
        result.SponsorForeignPerson = value.SponsorForeignPerson is null ? null : ToDto(value.SponsorForeignPerson, seen);
        result.FamilyVisitDetail = value.FamilyVisitDetail is null ? null : ToDto(value.FamilyVisitDetail, seen);
        return result;
    }

    public static StayCase ToEntity(StayCaseDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static StayCase ToEntity(StayCaseDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (StayCase)existing;
        var result = new StayCase();
        seen[value] = result;
        result.Id = value.Id;
        result.ForeignPersonId = value.ForeignPersonId;
        result.PurposeCode = value.PurposeCode;
        result.IsPrimary = value.IsPrimary;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.SponsorCompanyId = value.SponsorCompanyId;
        result.SponsorForeignPersonId = value.SponsorForeignPersonId;
        result.StatusCode = value.StatusCode;
        result.Note = value.Note;
        result.CreatedAt = value.CreatedAt;
        result.UpdatedAt = value.UpdatedAt;
        result.ForeignPerson = value.ForeignPerson is null ? null : ToEntity(value.ForeignPerson, seen);
        result.SponsorCompany = value.SponsorCompany is null ? null : ToEntity(value.SponsorCompany, seen);
        result.SponsorForeignPerson = value.SponsorForeignPerson is null ? null : ToEntity(value.SponsorForeignPerson, seen);
        result.FamilyVisitDetail = value.FamilyVisitDetail is null ? null : ToEntity(value.FamilyVisitDetail, seen);
        return result;
    }

    public static void CopyToDto(StayCase value, StayCaseDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.ForeignPersonId = mapped.ForeignPersonId;
        target.PurposeCode = mapped.PurposeCode;
        target.IsPrimary = mapped.IsPrimary;
        target.ValidFrom = mapped.ValidFrom;
        target.ValidTo = mapped.ValidTo;
        target.SponsorCompanyId = mapped.SponsorCompanyId;
        target.SponsorForeignPersonId = mapped.SponsorForeignPersonId;
        target.StatusCode = mapped.StatusCode;
        target.Note = mapped.Note;
        target.CreatedAt = mapped.CreatedAt;
        target.UpdatedAt = mapped.UpdatedAt;
        target.ForeignPerson = mapped.ForeignPerson;
        target.SponsorCompany = mapped.SponsorCompany;
        target.SponsorForeignPerson = mapped.SponsorForeignPerson;
        target.FamilyVisitDetail = mapped.FamilyVisitDetail;
    }
}
