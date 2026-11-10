using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

internal static partial class DtoMapper
{
    public static InspectionDto ToDto(Inspection value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static InspectionDto ToDto(Inspection value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (InspectionDto)existing;
        var result = new InspectionDto();
        seen[value] = result;
        result.Id = value.Id;
        result.InspectedAt = value.InspectedAt;
        result.LocationText = value.LocationText;
        result.AdministrativeUnitId = value.AdministrativeUnitId;
        result.AccommodationId = value.AccommodationId;
        result.CompanySiteId = value.CompanySiteId;
        result.InspectorNames = value.InspectorNames;
        result.InspectionUnit = value.InspectionUnit;
        result.TypeCode = value.TypeCode;
        result.Summary = value.Summary;
        result.Note = value.Note;
        result.CreatedByAccountId = value.CreatedByAccountId;
        result.CreatedAt = value.CreatedAt;
        result.AdministrativeUnit = value.AdministrativeUnit is null ? null : ToDto(value.AdministrativeUnit, seen);
        result.Accommodation = value.Accommodation is null ? null : ToDto(value.Accommodation, seen);
        result.CompanySite = value.CompanySite is null ? null : ToDto(value.CompanySite, seen);
        result.Subjects = value.Subjects.Select(item0 => ToDto(item0, seen)).ToList();
        return result;
    }

    public static Inspection ToEntity(InspectionDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static Inspection ToEntity(InspectionDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (Inspection)existing;
        var result = new Inspection();
        seen[value] = result;
        result.Id = value.Id;
        result.InspectedAt = value.InspectedAt;
        result.LocationText = value.LocationText;
        result.AdministrativeUnitId = value.AdministrativeUnitId;
        result.AccommodationId = value.AccommodationId;
        result.CompanySiteId = value.CompanySiteId;
        result.InspectorNames = value.InspectorNames;
        result.InspectionUnit = value.InspectionUnit;
        result.TypeCode = value.TypeCode;
        result.Summary = value.Summary;
        result.Note = value.Note;
        result.CreatedByAccountId = value.CreatedByAccountId;
        result.CreatedAt = value.CreatedAt;
        result.AdministrativeUnit = value.AdministrativeUnit is null ? null : ToEntity(value.AdministrativeUnit, seen);
        result.Accommodation = value.Accommodation is null ? null : ToEntity(value.Accommodation, seen);
        result.CompanySite = value.CompanySite is null ? null : ToEntity(value.CompanySite, seen);
        result.Subjects = value.Subjects.Select(item0 => ToEntity(item0, seen)).ToList();
        return result;
    }

    public static void CopyToDto(Inspection value, InspectionDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.InspectedAt = mapped.InspectedAt;
        target.LocationText = mapped.LocationText;
        target.AdministrativeUnitId = mapped.AdministrativeUnitId;
        target.AccommodationId = mapped.AccommodationId;
        target.CompanySiteId = mapped.CompanySiteId;
        target.InspectorNames = mapped.InspectorNames;
        target.InspectionUnit = mapped.InspectionUnit;
        target.TypeCode = mapped.TypeCode;
        target.Summary = mapped.Summary;
        target.Note = mapped.Note;
        target.CreatedByAccountId = mapped.CreatedByAccountId;
        target.CreatedAt = mapped.CreatedAt;
        target.AdministrativeUnit = mapped.AdministrativeUnit;
        target.Accommodation = mapped.Accommodation;
        target.CompanySite = mapped.CompanySite;
        target.Subjects = mapped.Subjects;
    }

    public static InspectionSubjectDto ToDto(InspectionSubject value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static InspectionSubjectDto ToDto(InspectionSubject value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (InspectionSubjectDto)existing;
        var result = new InspectionSubjectDto();
        seen[value] = result;
        result.Id = value.Id;
        result.InspectionId = value.InspectionId;
        result.ForeignPersonId = value.ForeignPersonId;
        result.PurposeCodeSnapshot = value.PurposeCodeSnapshot;
        result.DocumentsSnapshot = value.DocumentsSnapshot;
        result.WorkDescriptionSnapshot = value.WorkDescriptionSnapshot;
        result.AddressSnapshot = value.AddressSnapshot;
        result.ResultCode = value.ResultCode;
        result.ViolationDetails = value.ViolationDetails;
        result.ActionTaken = value.ActionTaken;
        result.Note = value.Note;
        result.Inspection = value.Inspection is null ? null : ToDto(value.Inspection, seen);
        result.ForeignPerson = value.ForeignPerson is null ? null : ToDto(value.ForeignPerson, seen);
        return result;
    }

    public static InspectionSubject ToEntity(InspectionSubjectDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static InspectionSubject ToEntity(InspectionSubjectDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (InspectionSubject)existing;
        var result = new InspectionSubject();
        seen[value] = result;
        result.Id = value.Id;
        result.InspectionId = value.InspectionId;
        result.ForeignPersonId = value.ForeignPersonId;
        result.PurposeCodeSnapshot = value.PurposeCodeSnapshot;
        result.DocumentsSnapshot = value.DocumentsSnapshot;
        result.WorkDescriptionSnapshot = value.WorkDescriptionSnapshot;
        result.AddressSnapshot = value.AddressSnapshot;
        result.ResultCode = value.ResultCode;
        result.ViolationDetails = value.ViolationDetails;
        result.ActionTaken = value.ActionTaken;
        result.Note = value.Note;
        result.Inspection = value.Inspection is null ? null : ToEntity(value.Inspection, seen);
        result.ForeignPerson = value.ForeignPerson is null ? null : ToEntity(value.ForeignPerson, seen);
        return result;
    }

    public static void CopyToDto(InspectionSubject value, InspectionSubjectDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.InspectionId = mapped.InspectionId;
        target.ForeignPersonId = mapped.ForeignPersonId;
        target.PurposeCodeSnapshot = mapped.PurposeCodeSnapshot;
        target.DocumentsSnapshot = mapped.DocumentsSnapshot;
        target.WorkDescriptionSnapshot = mapped.WorkDescriptionSnapshot;
        target.AddressSnapshot = mapped.AddressSnapshot;
        target.ResultCode = mapped.ResultCode;
        target.ViolationDetails = mapped.ViolationDetails;
        target.ActionTaken = mapped.ActionTaken;
        target.Note = mapped.Note;
        target.Inspection = mapped.Inspection;
        target.ForeignPerson = mapped.ForeignPerson;
    }
}
