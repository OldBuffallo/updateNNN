using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

internal static partial class DtoMapper
{
    public static AdministrativeUnitDto ToDto(AdministrativeUnit value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static AdministrativeUnitDto ToDto(AdministrativeUnit value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (AdministrativeUnitDto)existing;
        var result = new AdministrativeUnitDto();
        seen[value] = result;
        result.Id = value.Id;
        result.Code = value.Code;
        result.Name = value.Name;
        result.TypeCode = value.TypeCode;
        result.ParentId = value.ParentId;
        result.PredecessorId = value.PredecessorId;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.IsDeleted = value.IsDeleted;
        result.LegacyDistrictId = value.LegacyDistrictId;
        result.LegacyWardId = value.LegacyWardId;
        result.Parent = value.Parent is null ? null : ToDto(value.Parent, seen);
        result.Predecessor = value.Predecessor is null ? null : ToDto(value.Predecessor, seen);
        return result;
    }

    public static AdministrativeUnit ToEntity(AdministrativeUnitDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static AdministrativeUnit ToEntity(AdministrativeUnitDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (AdministrativeUnit)existing;
        var result = new AdministrativeUnit();
        seen[value] = result;
        result.Id = value.Id;
        result.Code = value.Code;
        result.Name = value.Name;
        result.TypeCode = value.TypeCode;
        result.ParentId = value.ParentId;
        result.PredecessorId = value.PredecessorId;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.IsDeleted = value.IsDeleted;
        result.LegacyDistrictId = value.LegacyDistrictId;
        result.LegacyWardId = value.LegacyWardId;
        result.Parent = value.Parent is null ? null : ToEntity(value.Parent, seen);
        result.Predecessor = value.Predecessor is null ? null : ToEntity(value.Predecessor, seen);
        return result;
    }

    public static void CopyToDto(AdministrativeUnit value, AdministrativeUnitDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.Code = mapped.Code;
        target.Name = mapped.Name;
        target.TypeCode = mapped.TypeCode;
        target.ParentId = mapped.ParentId;
        target.PredecessorId = mapped.PredecessorId;
        target.ValidFrom = mapped.ValidFrom;
        target.ValidTo = mapped.ValidTo;
        target.IsDeleted = mapped.IsDeleted;
        target.LegacyDistrictId = mapped.LegacyDistrictId;
        target.LegacyWardId = mapped.LegacyWardId;
        target.Parent = mapped.Parent;
        target.Predecessor = mapped.Predecessor;
    }

    public static ResidencePeriodDto ToDto(ResidencePeriod value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static ResidencePeriodDto ToDto(ResidencePeriod value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ResidencePeriodDto)existing;
        var result = new ResidencePeriodDto();
        seen[value] = result;
        result.Id = value.Id;
        result.ForeignPersonId = value.ForeignPersonId;
        result.AccommodationId = value.AccommodationId;
        result.AdministrativeUnitId = value.AdministrativeUnitId;
        result.AddressLine = value.AddressLine;
        result.UnitLabel = value.UnitLabel;
        result.ArrangementCode = value.ArrangementCode;
        result.ResponsibleCompanyId = value.ResponsibleCompanyId;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.Note = value.Note;
        result.ForeignPerson = value.ForeignPerson is null ? null : ToDto(value.ForeignPerson, seen);
        result.Accommodation = value.Accommodation is null ? null : ToDto(value.Accommodation, seen);
        result.AdministrativeUnit = value.AdministrativeUnit is null ? null : ToDto(value.AdministrativeUnit, seen);
        result.ResponsibleCompany = value.ResponsibleCompany is null ? null : ToDto(value.ResponsibleCompany, seen);
        return result;
    }

    public static ResidencePeriod ToEntity(ResidencePeriodDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static ResidencePeriod ToEntity(ResidencePeriodDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ResidencePeriod)existing;
        var result = new ResidencePeriod();
        seen[value] = result;
        result.Id = value.Id;
        result.ForeignPersonId = value.ForeignPersonId;
        result.AccommodationId = value.AccommodationId;
        result.AdministrativeUnitId = value.AdministrativeUnitId;
        result.AddressLine = value.AddressLine;
        result.UnitLabel = value.UnitLabel;
        result.ArrangementCode = value.ArrangementCode;
        result.ResponsibleCompanyId = value.ResponsibleCompanyId;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.Note = value.Note;
        result.ForeignPerson = value.ForeignPerson is null ? null : ToEntity(value.ForeignPerson, seen);
        result.Accommodation = value.Accommodation is null ? null : ToEntity(value.Accommodation, seen);
        result.AdministrativeUnit = value.AdministrativeUnit is null ? null : ToEntity(value.AdministrativeUnit, seen);
        result.ResponsibleCompany = value.ResponsibleCompany is null ? null : ToEntity(value.ResponsibleCompany, seen);
        return result;
    }

    public static void CopyToDto(ResidencePeriod value, ResidencePeriodDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.ForeignPersonId = mapped.ForeignPersonId;
        target.AccommodationId = mapped.AccommodationId;
        target.AdministrativeUnitId = mapped.AdministrativeUnitId;
        target.AddressLine = mapped.AddressLine;
        target.UnitLabel = mapped.UnitLabel;
        target.ArrangementCode = mapped.ArrangementCode;
        target.ResponsibleCompanyId = mapped.ResponsibleCompanyId;
        target.ValidFrom = mapped.ValidFrom;
        target.ValidTo = mapped.ValidTo;
        target.Note = mapped.Note;
        target.ForeignPerson = mapped.ForeignPerson;
        target.Accommodation = mapped.Accommodation;
        target.AdministrativeUnit = mapped.AdministrativeUnit;
        target.ResponsibleCompany = mapped.ResponsibleCompany;
    }
}
