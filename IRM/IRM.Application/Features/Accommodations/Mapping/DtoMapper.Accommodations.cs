using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

internal static partial class DtoMapper
{
    public static AccommodationDto ToDto(Accommodation value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static AccommodationDto ToDto(Accommodation value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (AccommodationDto)existing;
        var result = new AccommodationDto();
        seen[value] = result;
        result.Id = value.Id;
        result.Name = value.Name;
        result.TypeCode = value.TypeCode;
        result.AddressLine = value.AddressLine;
        result.AdministrativeUnitId = value.AdministrativeUnitId;
        result.ContactName = value.ContactName;
        result.ContactPhone = value.ContactPhone;
        result.Capacity = value.Capacity;
        result.Note = value.Note;
        result.IsDeleted = value.IsDeleted;
        result.CreatedAt = value.CreatedAt;
        result.UpdatedAt = value.UpdatedAt;
        result.AdministrativeUnit = value.AdministrativeUnit is null ? null : ToDto(value.AdministrativeUnit, seen);
        result.CompanyAgreements = value.CompanyAgreements.Select(item0 => ToDto(item0, seen)).ToList();
        result.ResidencePeriods = value.ResidencePeriods.Select(item0 => ToDto(item0, seen)).ToList();
        return result;
    }

    public static Accommodation ToEntity(AccommodationDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static Accommodation ToEntity(AccommodationDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (Accommodation)existing;
        var result = new Accommodation();
        seen[value] = result;
        result.Id = value.Id;
        result.Name = value.Name;
        result.TypeCode = value.TypeCode;
        result.AddressLine = value.AddressLine;
        result.AdministrativeUnitId = value.AdministrativeUnitId;
        result.ContactName = value.ContactName;
        result.ContactPhone = value.ContactPhone;
        result.Capacity = value.Capacity;
        result.Note = value.Note;
        result.IsDeleted = value.IsDeleted;
        result.CreatedAt = value.CreatedAt;
        result.UpdatedAt = value.UpdatedAt;
        result.AdministrativeUnit = value.AdministrativeUnit is null ? null : ToEntity(value.AdministrativeUnit, seen);
        result.CompanyAgreements = value.CompanyAgreements.Select(item0 => ToEntity(item0, seen)).ToList();
        result.ResidencePeriods = value.ResidencePeriods.Select(item0 => ToEntity(item0, seen)).ToList();
        return result;
    }

    public static void CopyToDto(Accommodation value, AccommodationDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.Name = mapped.Name;
        target.TypeCode = mapped.TypeCode;
        target.AddressLine = mapped.AddressLine;
        target.AdministrativeUnitId = mapped.AdministrativeUnitId;
        target.ContactName = mapped.ContactName;
        target.ContactPhone = mapped.ContactPhone;
        target.Capacity = mapped.Capacity;
        target.Note = mapped.Note;
        target.IsDeleted = mapped.IsDeleted;
        target.CreatedAt = mapped.CreatedAt;
        target.UpdatedAt = mapped.UpdatedAt;
        target.AdministrativeUnit = mapped.AdministrativeUnit;
        target.CompanyAgreements = mapped.CompanyAgreements;
        target.ResidencePeriods = mapped.ResidencePeriods;
    }

    public static CompanyAccommodationAgreementDto ToDto(CompanyAccommodationAgreement value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static CompanyAccommodationAgreementDto ToDto(CompanyAccommodationAgreement value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (CompanyAccommodationAgreementDto)existing;
        var result = new CompanyAccommodationAgreementDto();
        seen[value] = result;
        result.Id = value.Id;
        result.CompanyId = value.CompanyId;
        result.AccommodationId = value.AccommodationId;
        result.ScopeCode = value.ScopeCode;
        result.UnitLabel = value.UnitLabel;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.Note = value.Note;
        result.Company = value.Company is null ? null : ToDto(value.Company, seen);
        result.Accommodation = value.Accommodation is null ? null : ToDto(value.Accommodation, seen);
        return result;
    }

    public static CompanyAccommodationAgreement ToEntity(CompanyAccommodationAgreementDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static CompanyAccommodationAgreement ToEntity(CompanyAccommodationAgreementDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (CompanyAccommodationAgreement)existing;
        var result = new CompanyAccommodationAgreement();
        seen[value] = result;
        result.Id = value.Id;
        result.CompanyId = value.CompanyId;
        result.AccommodationId = value.AccommodationId;
        result.ScopeCode = value.ScopeCode;
        result.UnitLabel = value.UnitLabel;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.Note = value.Note;
        result.Company = value.Company is null ? null : ToEntity(value.Company, seen);
        result.Accommodation = value.Accommodation is null ? null : ToEntity(value.Accommodation, seen);
        return result;
    }

    public static void CopyToDto(CompanyAccommodationAgreement value, CompanyAccommodationAgreementDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.CompanyId = mapped.CompanyId;
        target.AccommodationId = mapped.AccommodationId;
        target.ScopeCode = mapped.ScopeCode;
        target.UnitLabel = mapped.UnitLabel;
        target.ValidFrom = mapped.ValidFrom;
        target.ValidTo = mapped.ValidTo;
        target.Note = mapped.Note;
        target.Company = mapped.Company;
        target.Accommodation = mapped.Accommodation;
    }
}
