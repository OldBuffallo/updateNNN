using IRM.Services;
using IRM.Application.Features.ImportExport;
using static IRM.Application.Features.ImportExport.ImportMapping;
using System.Text.Json;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Infrastructure.Persistence.Stores.ImportExport;

/// <summary>
/// Service xử lý nghiệp vụ Import Excel vào CSDL
/// </summary>
public partial class SqlImportStore
{

    // ══════════════════════════════════════════
    // PRIVATE HELPERS
    // ══════════════════════════════════════════

    private async Task<V010ImportChanges> UpsertV010ExtensionsAsync(Employee employee, Dictionary<string, string> fields, int companyId)
    {
        var changes = new V010ImportChanges();
        if (fields.Count == 0) return changes;

        var sourceId = employee.IDEmployee.ToString();
        var link = await _db.ForeignPersonSourceLinks
            .Include(item => item.ForeignPerson)
            .SingleOrDefaultAsync(item => item.SourceType == "EMPLOYEE" && item.SourceId == sourceId);
        var person = link?.ForeignPerson;
        var createdSourceLink = false;
        var passport = IRM.Core.Rules.PassportRules.NormalizePassport(employee.Passport);
        var passportKey = IRM.Core.Rules.PassportRules.SearchKey(passport);
        if (person is null && passportKey is not null)
            person = await _db.ForeignPersons.SingleOrDefaultAsync(item => item.PassportSearchKey == passportKey && !item.IsDeleted);
        if (person is null)
        {
            person = new ForeignPerson
            {
                FullName = employee.StaffName.Trim(),
                Gender = (int)employee.Gender,
                Birthday = employee.Birthday,
                NationalityCode = employee.Nationality,
                PassportNumber = passport,
                PassportSearchKey = passportKey,
                IsDataIncomplete = passportKey is null
            };
            _db.ForeignPersons.Add(person);
            changes.CreatedForeignPerson = true;
        }
        if (link is null)
        {
            link = new ForeignPersonSourceLink
            {
                ForeignPerson = person,
                SourceType = "EMPLOYEE",
                SourceId = sourceId,
                SourceFingerprint = $"IMPORT:{employee.DateCreated:O}"
            };
            _db.ForeignPersonSourceLinks.Add(link);
            createdSourceLink = true;
        }
        await _db.SaveChangesAsync();
        changes.ForeignPersonId = person.Id;
        if (createdSourceLink) changes.CreatedSourceLinkId = link.Id;

        if (fields.TryGetValue("StayPurposeCode", out var purposeValue))
        {
            var purpose = purposeValue.Trim().ToUpperInvariant();
            if (!StayPurposeCodes.All.Contains(purpose)) throw new InvalidOperationException($"Diện cư trú không hợp lệ: {purposeValue}.");
            var validFrom = ParseImportDate(fields.GetValueOrDefault("StayValidFrom"), employee.DateCreated ?? DateTime.Today);
            var validTo = ParseNullableImportDate(fields.GetValueOrDefault("StayValidTo"));
            if (validTo.HasValue && validTo < validFrom) throw new InvalidOperationException("Khoảng hiệu lực diện cư trú không hợp lệ.");
            var overlap = await _db.StayCases.FirstOrDefaultAsync(item => item.ForeignPersonId == person.Id && item.IsPrimary
                && item.StatusCode != "DELETED" && item.ValidFrom <= (validTo ?? DateTime.MaxValue)
                && (!item.ValidTo.HasValue || item.ValidTo.Value >= validFrom));
            if (overlap is null)
            {
                var stay = new StayCase
                {
                    ForeignPersonId = person.Id,
                    PurposeCode = purpose,
                    IsPrimary = true,
                    SponsorCompanyId = purpose == StayPurposeCodes.Work ? companyId : null,
                    ValidFrom = validFrom,
                    ValidTo = validTo
                };
                _db.StayCases.Add(stay); changes.CreatedStayCases.Add(stay);
            }
            else if (overlap.PurposeCode != purpose)
                throw new InvalidOperationException("Diện cư trú import chồng lấn một diện chính khác.");
        }

        if (fields.TryGetValue("ElectronicIdentityNumber", out var identityNumber))
        {
            var identityKey = IRM.Core.Rules.PassportRules.SearchKey(identityNumber)!;
            if (!await _db.ElectronicIdentities.AnyAsync(item => item.ForeignPersonId == person.Id && item.IdentitySearchKey == identityKey))
            {
                var identity = new ElectronicIdentity
                {
                    ForeignPersonId = person.Id,
                    IdentityNumber = identityNumber.Trim(),
                    IdentitySearchKey = identityKey,
                    ValidFrom = ParseImportDate(fields.GetValueOrDefault("StayValidFrom"), employee.DateCreated ?? DateTime.Today)
                };
                _db.ElectronicIdentities.Add(identity); changes.CreatedIdentities.Add(identity);
            }
        }

        if (fields.TryGetValue("DocumentNumber", out var documentValue))
        {
            var numbers = SplitImportValues(documentValue);
            var types = SplitImportValues(fields.GetValueOrDefault("DocumentTypeCode"));
            var validToValues = SplitImportValues(fields.GetValueOrDefault("DocumentValidTo"));
            var validFrom = ParseImportDate(fields.GetValueOrDefault("DocumentValidFrom"), employee.DateCreated ?? DateTime.Today);
            for (var index = 0; index < numbers.Length; index++)
            {
                var type = (index < types.Length ? types[index] : ImmigrationDocumentTypeCodes.Visa).ToUpperInvariant();
                var allowedTypes = new[] { ImmigrationDocumentTypeCodes.Visa, ImmigrationDocumentTypeCodes.VisaExtension,
                    ImmigrationDocumentTypeCodes.TemporaryResidenceCard, ImmigrationDocumentTypeCodes.VisaExemption, ImmigrationDocumentTypeCodes.Other };
                if (!allowedTypes.Contains(type)) throw new InvalidOperationException($"Loại giấy tờ không hợp lệ: {type}.");
                var number = numbers[index];
                if (await _db.ImmigrationDocuments.AnyAsync(item => item.ForeignPersonId == person.Id && item.TypeCode == type && item.Number == number)) continue;
                var document = new ImmigrationDocument
                {
                    ForeignPersonId = person.Id,
                    TypeCode = type,
                    Number = number,
                    ValidFrom = validFrom,
                    ValidTo = index < validToValues.Length ? ParseNullableImportDate(validToValues[index]) : null
                };
                _db.ImmigrationDocuments.Add(document); changes.CreatedDocuments.Add(document);
            }
        }

        if (fields.TryGetValue("AdministrativeUnitCode", out var unitCode)
            && fields.TryGetValue("ResidenceAddress", out var residenceAddress))
        {
            var unit = await _db.AdministrativeUnits.SingleOrDefaultAsync(item => item.Code == unitCode && !item.IsDeleted)
                ?? throw new InvalidOperationException($"Không tìm thấy mã địa bàn: {unitCode}.");
            var residenceFrom = ParseImportDate(fields.GetValueOrDefault("ResidenceValidFrom"), employee.DateCreated ?? DateTime.Today);
            var residenceTo = ParseNullableImportDate(fields.GetValueOrDefault("ResidenceValidTo"));
            if (residenceTo.HasValue && residenceTo < residenceFrom) throw new InvalidOperationException("Khoảng lịch sử lưu trú không hợp lệ.");
            if (!await _db.ResidencePeriods.AnyAsync(item => item.ForeignPersonId == person.Id
                && item.AdministrativeUnitId == unit.Id && item.AddressLine == residenceAddress && item.ValidFrom == residenceFrom))
            {
                var residence = new ResidencePeriod
                {
                    ForeignPersonId = person.Id,
                    AdministrativeUnitId = unit.Id,
                    AddressLine = residenceAddress,
                    ResponsibleCompanyId = companyId,
                    ValidFrom = residenceFrom,
                    ValidTo = residenceTo
                };
                _db.ResidencePeriods.Add(residence); changes.CreatedResidences.Add(residence);
            }
        }

        await _db.SaveChangesAsync();
        changes.CreatedStayCaseIds = changes.CreatedStayCases.Select(item => item.Id).ToList();
        changes.CreatedDocumentIds = changes.CreatedDocuments.Select(item => item.Id).ToList();
        changes.CreatedIdentityIds = changes.CreatedIdentities.Select(item => item.Id).ToList();
        changes.CreatedResidenceIds = changes.CreatedResidences.Select(item => item.Id).ToList();
        return changes;
    }

    private async Task RollbackV010ChangesAsync(string? oldData)
    {
        if (string.IsNullOrWhiteSpace(oldData)) return;
        using var document = JsonDocument.Parse(oldData);
        if (!document.RootElement.TryGetProperty("V010Changes", out var element)) return;
        var changes = element.Deserialize<V010ImportChanges>();
        if (changes is null) return;
        _db.StayCases.RemoveRange(await _db.StayCases.Where(item => changes.CreatedStayCaseIds.Contains(item.Id)).ToListAsync());
        _db.ImmigrationDocuments.RemoveRange(await _db.ImmigrationDocuments.Where(item => changes.CreatedDocumentIds.Contains(item.Id)).ToListAsync());
        _db.ElectronicIdentities.RemoveRange(await _db.ElectronicIdentities.Where(item => changes.CreatedIdentityIds.Contains(item.Id)).ToListAsync());
        _db.ResidencePeriods.RemoveRange(await _db.ResidencePeriods.Where(item => changes.CreatedResidenceIds.Contains(item.Id)).ToListAsync());
        if (changes.CreatedSourceLinkId.HasValue)
        {
            var sourceLink = await _db.ForeignPersonSourceLinks.FindAsync(changes.CreatedSourceLinkId.Value);
            if (sourceLink is not null) _db.ForeignPersonSourceLinks.Remove(sourceLink);
        }
        if (changes.CreatedForeignPerson && changes.ForeignPersonId.HasValue)
        {
            var createdLinkId = changes.CreatedSourceLinkId ?? -1;
            var hasAnotherSource = await _db.ForeignPersonSourceLinks.AnyAsync(item => item.ForeignPersonId == changes.ForeignPersonId
                && item.Id != createdLinkId);
            var hasInspection = await _db.InspectionSubjects.AnyAsync(item => item.ForeignPersonId == changes.ForeignPersonId);
            if (!hasAnotherSource && !hasInspection)
            {
                var person = await _db.ForeignPersons.FindAsync(changes.ForeignPersonId.Value);
                if (person is not null) _db.ForeignPersons.Remove(person);
            }
        }
        await _db.SaveChangesAsync();
    }

    private sealed class V010ImportChanges
    {
        public V010ImportChanges() { }
        public int? ForeignPersonId { get; set; }
        public bool CreatedForeignPerson { get; set; }
        public int? CreatedSourceLinkId { get; set; }
        public List<int> CreatedStayCaseIds { get; set; } = [];
        public List<int> CreatedDocumentIds { get; set; } = [];
        public List<int> CreatedIdentityIds { get; set; } = [];
        public List<int> CreatedResidenceIds { get; set; } = [];
        [System.Text.Json.Serialization.JsonIgnore] public List<StayCase> CreatedStayCases { get; } = [];
        [System.Text.Json.Serialization.JsonIgnore] public List<ImmigrationDocument> CreatedDocuments { get; } = [];
        [System.Text.Json.Serialization.JsonIgnore] public List<ElectronicIdentity> CreatedIdentities { get; } = [];
        [System.Text.Json.Serialization.JsonIgnore] public List<ResidencePeriod> CreatedResidences { get; } = [];
    }
}
