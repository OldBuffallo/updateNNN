using IRM.Application.Features.ImportExport;
using static IRM.Application.Features.ImportExport.ImportMapping;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Infrastructure.Persistence.Stores.ImportExport;

public partial class SqlImportStore
{
    private static void PreserveUnmappedExtensions(Employee value, Employee old, ISet<string> mapped)
    {
        if (!mapped.Contains("VisaSymbol")) value.VisaSymbol = old.VisaSymbol;
        if (!mapped.Contains("CardCreationDate") && !mapped.Contains("SettlementResults")) value.CardCreationDate = old.CardCreationDate;
        if (!mapped.Contains("FamilyVisit")) value.FamilyVisit = old.FamilyVisit;
        if (!mapped.Contains("FamilyVisitRelativeName")) value.FamilyVisitRelativeName = old.FamilyVisitRelativeName;
        if (!mapped.Contains("FamilyVisitRelationship")) value.FamilyVisitRelationship = old.FamilyVisitRelationship;
        if (!mapped.Contains("FamilyVisitRelativeIdCard")) value.FamilyVisitRelativeIdCard = old.FamilyVisitRelativeIdCard;
        if (!mapped.Contains("FamilyVisitStartDate")) value.FamilyVisitStartDate = old.FamilyVisitStartDate;
        if (!mapped.Contains("FamilyVisitEndDate")) value.FamilyVisitEndDate = old.FamilyVisitEndDate;
        if (!mapped.Contains("FamilyVisitNote")) value.FamilyVisitNote = old.FamilyVisitNote;
    }

    private async Task<bool> HasMappedExtensionChangesAsync(Employee employee, Dictionary<string,string> fields)
    {
        if (!fields.Keys.Any(V010ImportFields.Contains)) return false;
        var source = employee.IDEmployee.ToString();
        var person = await _db.ForeignPersonSourceLinks.AsNoTracking().Where(l => l.SourceType == "EMPLOYEE" && l.SourceId == source)
            .Select(l => l.ForeignPerson).FirstOrDefaultAsync();
        var passportKey = IRM.Core.Rules.PassportRules.SearchKey(employee.Passport);
        person ??= await _db.ForeignPersons.AsNoTracking().FirstOrDefaultAsync(p => p.PassportSearchKey == passportKey && !p.IsDeleted);
        if (person == null) return true;
        var id = person.Id;
        if (fields.TryGetValue("ElectronicIdentityNumber",out var identity))
        {
            var key = IRM.Core.Rules.PassportRules.SearchKey(identity);
            if (!await _db.ElectronicIdentities.AnyAsync(i => i.ForeignPersonId == id && i.IdentitySearchKey == key)) return true;
        }
        if (fields.TryGetValue("DocumentNumber",out var numbersText))
        {
            var numbers = SplitImportValues(numbersText);
            var types = SplitImportValues(fields.GetValueOrDefault("DocumentTypeCode"));
            var expires = SplitImportValues(fields.GetValueOrDefault("DocumentValidTo"));
            var from = ParseNullableImportDate(fields.GetValueOrDefault("DocumentValidFrom"));
            var documents = await _db.ImmigrationDocuments.AsNoTracking().Where(d => d.ForeignPersonId == id).ToListAsync();
            for (int index = 0; index < numbers.Length; index++)
            {
                var type = index < types.Length ? types[index].ToUpperInvariant() : ImmigrationDocumentTypeCodes.Visa;
                var to = index < expires.Length ? ParseNullableImportDate(expires[index]) : null;
                if (!documents.Any(d => d.TypeCode == type && d.Number == numbers[index] &&
                    (!from.HasValue || d.ValidFrom == from) && (index >= expires.Length || d.ValidTo == to))) return true;
            }
        }
        if (fields.TryGetValue("StayPurposeCode",out var purpose))
        {
            purpose = purpose.Trim().ToUpperInvariant();
            var from = ParseNullableImportDate(fields.GetValueOrDefault("StayValidFrom"));
            var to = ParseNullableImportDate(fields.GetValueOrDefault("StayValidTo"));
            if (!await _db.StayCases.AnyAsync(s => s.ForeignPersonId == id && s.IsPrimary && s.StatusCode != "DELETED" &&
                s.PurposeCode == purpose && (!from.HasValue || s.ValidFrom == from) &&
                (!to.HasValue || s.ValidTo == to) && (purpose != StayPurposeCodes.Work || s.SponsorCompanyId == employee.IDCompany))) return true;
        }
        if (fields.TryGetValue("AdministrativeUnitCode",out var code) && fields.TryGetValue("ResidenceAddress",out var address))
        {
            var from = ParseNullableImportDate(fields.GetValueOrDefault("ResidenceValidFrom"));
            var to = ParseNullableImportDate(fields.GetValueOrDefault("ResidenceValidTo"));
            if (!await _db.ResidencePeriods.AnyAsync(r => r.ForeignPersonId == id && r.AdministrativeUnit!.Code == code &&
                r.AddressLine == address && (!from.HasValue || r.ValidFrom == from) && (!to.HasValue || r.ValidTo == to))) return true;
        }
        return false;
    }
}
