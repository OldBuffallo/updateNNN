using IRM.Application.Models;
using IRM.Data;
using IRM.Data.Models;
using IRM.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static IRM.Core.Rules.PassportRules;

namespace IRM.Infrastructure.Persistence.Stores.ForeignPersons;

public sealed partial class SqlFamilyVisitorStore
{
    public async Task<IReadOnlyList<UnifiedForeignPersonDto>> GetUnifiedProfilesAsync(
        string? search,
        DateTime asOfDate,
        CancellationToken cancellationToken = default)
    {
        if (_guard is not null)
            await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);

        var asOf = asOfDate.Date;

        var persons = await _db.ForeignPersons.AsNoTracking()
            .Include(x => x.Nationality)
            .Include(x => x.SourceLinks)
            .Include(x => x.StayCases).ThenInclude(s => s.FamilyVisitDetail)
            .Include(x => x.ResidencePeriods).ThenInclude(r => r.AdministrativeUnit)
            .Include(x => x.ImmigrationDocuments)
            .Include(x => x.ElectronicIdentities)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        var employees = await _db.Employees.AsNoTracking()
            .Include(e => e.Company)
            .Include(e => e.Career)
            .Include(e => e.NationalityNav)
            .Where(e => e.Hidden_flag == 0 && e.WorkingStatus == 0)
            .OrderBy(e => e.StaffName)
            .ToListAsync(cancellationToken);

        var students = await _db.Students.AsNoTracking()
            .Include(s => s.NationalityNav)
            .Where(s => s.Hidden_flag == 0)
            .OrderBy(s => s.FullName)
            .ToListAsync(cancellationToken);

        var nationalities = await _db.Nationality.AsNoTracking()
            .Where(n => n.Delete_flag == 0)
            .ToDictionaryAsync(n => n.NationalityCode, n => n.NationalityName, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var empById = employees.ToDictionary(e => e.IDEmployee);
        var stuById = students.ToDictionary(s => s.IDStudent);
        var matchedEmpIds = new HashSet<int>();
        var matchedStuIds = new HashSet<int>();

        var result = new List<UnifiedForeignPersonDto>();

        foreach (var person in persons)
        {
            Employee? emp = null;
            Student? stu = null;

            foreach (var link in person.SourceLinks)
            {
                if (string.Equals(link.SourceType, "EMPLOYEE", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(link.SourceId, out var empId)
                    && empById.TryGetValue(empId, out var linkedEmp))
                {
                    emp = linkedEmp;
                    matchedEmpIds.Add(empId);
                }
                else if (string.Equals(link.SourceType, "STUDENT", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(link.SourceId, out var stuId)
                    && stuById.TryGetValue(stuId, out var linkedStu))
                {
                    stu = linkedStu;
                    matchedStuIds.Add(stuId);
                }
            }

            if (emp is null && !string.IsNullOrEmpty(person.PassportSearchKey))
            {
                var candidateEmp = employees.FirstOrDefault(e =>
                    !matchedEmpIds.Contains(e.IDEmployee)
                    && SearchKey(e.Passport) == person.PassportSearchKey);
                if (candidateEmp is not null)
                {
                    emp = candidateEmp;
                    matchedEmpIds.Add(candidateEmp.IDEmployee);
                }
            }

            if (stu is null && !string.IsNullOrEmpty(person.PassportSearchKey))
            {
                var candidateStu = students.FirstOrDefault(s =>
                    !matchedStuIds.Contains(s.IDStudent)
                    && SearchKey(s.Passport) == person.PassportSearchKey);
                if (candidateStu is not null)
                {
                    stu = candidateStu;
                    matchedStuIds.Add(candidateStu.IDStudent);
                }
            }

            // If this ForeignPerson was only linked to soft-deleted legacy records and has no active StayCases, skip if orphaned
            var dto = BuildUnifiedDto(person, emp, stu, asOf, nationalities);
            result.Add(dto);
        }

        // Group remaining unlinked Employees and Students (including those without passport or with duplicate passports)
        foreach (var emp in employees.Where(e => !matchedEmpIds.Contains(e.IDEmployee)))
        {
            Student? stu = null;
            var empKey = SearchKey(emp.Passport);
            if (!string.IsNullOrEmpty(empKey))
            {
                stu = students.FirstOrDefault(s =>
                    !matchedStuIds.Contains(s.IDStudent)
                    && SearchKey(s.Passport) == empKey);
                if (stu is not null)
                    matchedStuIds.Add(stu.IDStudent);
            }

            result.Add(BuildUnifiedDto(null, emp, stu, asOf, nationalities));
        }

        foreach (var stu in students.Where(s => !matchedStuIds.Contains(s.IDStudent)))
        {
            result.Add(BuildUnifiedDto(null, null, stu, asOf, nationalities));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var passportKey = SearchKey(term);
            result = result.Where(x =>
                x.FullName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrEmpty(x.PassportNumber) && (x.PassportNumber.Contains(term, StringComparison.OrdinalIgnoreCase) || SearchKey(x.PassportNumber) == passportKey))
                || (!string.IsNullOrEmpty(x.CompanyName) && x.CompanyName.Contains(term, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrEmpty(x.SchoolName) && x.SchoolName.Contains(term, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrEmpty(x.RelativeName) && x.RelativeName.Contains(term, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrEmpty(x.ElectronicIdNumber) && x.ElectronicIdNumber.Contains(term, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrEmpty(x.NationalityCode) && x.NationalityCode.Contains(term, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrEmpty(x.NationalityName) && x.NationalityName.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        return result.OrderBy(x => x.FullName).ToList();
    }

    private static UnifiedForeignPersonDto BuildUnifiedDto(
        ForeignPerson? person,
        Employee? emp,
        Student? stu,
        DateTime asOf,
        Dictionary<string, string> nationalities)
    {
        var activeCases = person?.StayCases
            .Where(c => c.StatusCode != "DELETED" && c.ValidFrom <= asOf && (!c.ValidTo.HasValue || c.ValidTo.Value >= asOf))
            .ToList() ?? [];

        var familyCase = activeCases.FirstOrDefault(c => c.PurposeCode == StayPurposeCodes.FamilyVisit)
            ?? person?.StayCases.Where(c => c.StatusCode != "DELETED" && c.PurposeCode == StayPurposeCodes.FamilyVisit)
                .OrderByDescending(c => c.ValidFrom).FirstOrDefault();

        var activeResidence = person?.ResidencePeriods
            .Where(r => r.ValidFrom <= asOf && (!r.ValidTo.HasValue || r.ValidTo.Value >= asOf))
            .OrderByDescending(r => r.ValidFrom)
            .FirstOrDefault()
            ?? person?.ResidencePeriods.OrderByDescending(r => r.ValidFrom).FirstOrDefault();

        var validDocs = person?.ImmigrationDocuments
            .Where(d => d.StatusCode == "VALID" && d.ValidFrom <= asOf && (!d.ValidTo.HasValue || d.ValidTo.Value >= asOf))
            .OrderByDescending(d => d.ValidFrom)
            .ToList() ?? [];

        var primaryDoc = validDocs.FirstOrDefault()
            ?? person?.ImmigrationDocuments.OrderByDescending(d => d.ValidFrom).FirstOrDefault();

        var activeEid = person?.ElectronicIdentities
            .Where(e => e.StatusCode == "ACTIVE" && e.ValidFrom <= asOf && (!e.ValidTo.HasValue || e.ValidTo.Value >= asOf))
            .OrderByDescending(e => e.ValidFrom)
            .FirstOrDefault()
            ?? person?.ElectronicIdentities.OrderByDescending(e => e.ValidFrom).FirstOrDefault();

        var natCode = person?.NationalityCode ?? emp?.Nationality ?? stu?.Nationality;
        var natName = person?.Nationality?.NationalityName
            ?? emp?.NationalityNav?.NationalityName
            ?? stu?.NationalityNav?.NationalityName
            ?? (natCode is not null && nationalities.TryGetValue(natCode, out var mappedNat) ? mappedNat : natCode);

        var hasWork = emp is not null || activeCases.Any(c => c.PurposeCode == StayPurposeCodes.Work);
        var hasStudy = stu is not null || activeCases.Any(c => c.PurposeCode == StayPurposeCodes.Study);
        var hasFamilyVisit = familyCase is not null || (emp is not null && emp.HasFamilyVisit);

        var purposes = new List<string>();
        if (hasWork) purposes.Add(StayPurposeCodes.Work);
        if (hasStudy) purposes.Add(StayPurposeCodes.Study);
        if (hasFamilyVisit) purposes.Add(StayPurposeCodes.FamilyVisit);
        foreach (var c in activeCases)
        {
            if (!purposes.Contains(c.PurposeCode))
                purposes.Add(c.PurposeCode);
        }

        return new UnifiedForeignPersonDto
        {
            ForeignPersonId = person?.Id,
            EmployeeId = emp?.IDEmployee,
            StudentId = stu?.IDStudent,
            FamilyVisitStayCaseId = familyCase?.Id,
            FullName = !string.IsNullOrWhiteSpace(person?.FullName)
                ? person.FullName
                : (!string.IsNullOrWhiteSpace(emp?.StaffName) ? emp.StaffName : stu?.FullName ?? ""),
            Gender = person is not null ? person.Gender : (emp is not null ? (int)emp.Gender : (stu is not null ? (int)stu.Gender : 1)),
            Birthday = person?.Birthday ?? emp?.Birthday ?? stu?.Birthday,
            NationalityCode = natCode,
            NationalityName = natName,
            PassportNumber = person?.PassportNumber ?? emp?.Passport ?? stu?.Passport,
            ResidenceAddress = activeResidence?.AddressLine ?? emp?.Address ?? stu?.Address,
            AdministrativeUnitId = activeResidence?.AdministrativeUnitId,
            AdministrativeUnitName = activeResidence?.AdministrativeUnit?.Name,
            Note = emp?.Note ?? stu?.Note ?? familyCase?.Note,

            HasWorkProfile = hasWork,
            HasStudyProfile = hasStudy,
            HasFamilyVisitProfile = hasFamilyVisit,
            PurposeCodes = purposes,

            CompanyId = emp?.IDCompany ?? activeCases.FirstOrDefault(c => c.SponsorCompanyId.HasValue)?.SponsorCompanyId,
            CompanyName = emp?.Company?.CompanyName,
            CareerId = emp?.IDCareer,
            CareerName = emp?.Career?.CareerName,
            WorkPermit = emp?.WorkPermit ?? WorkPermitType.WorkerNoPermit,
            WorkPermitNumber = emp?.WorkPermitNumber,
            TemporaryStay = emp?.TemporaryStay ?? stu?.TemporaryStay,
            VisaNumber = emp?.VisaNumber ?? stu?.VisaNumber ?? primaryDoc?.Number,
            VisaSymbol = emp?.VisaSymbol,

            SchoolName = stu?.SchoolName,
            Major = stu?.Major,
            EducationLevel = stu?.EducationLevel ?? EducationLevel.University,
            ScholarshipType = stu?.ScholarshipType ?? ScholarshipType.SelfFunded,
            EnrollmentDate = stu?.EnrollmentDate,
            GraduationDate = stu?.ExpectedGraduation,
            StudentStatus = stu?.Status ?? StudentStatus.Studying,
            VisaExpiry = stu?.VisaExpiry ?? primaryDoc?.ValidTo,
            PhoneNumber = stu?.StudentCode,

            RelativeName = familyCase?.FamilyVisitDetail?.RelativeName ?? emp?.FamilyVisitRelativeName,
            Relationship = familyCase?.FamilyVisitDetail?.Relationship ?? emp?.FamilyVisitRelationship,
            RelativeTypeCode = familyCase?.FamilyVisitDetail?.RelativeTypeCode ?? "VIETNAMESE_CITIZEN",
            RelativeNationalityCode = familyCase?.FamilyVisitDetail?.RelativeNationalityCode,
            RelativeIdNumber = familyCase?.FamilyVisitDetail?.RelativeIdNumber ?? emp?.FamilyVisitRelativeIdCard,
            RelativePhone = familyCase?.FamilyVisitDetail?.RelativePhone,
            RelativeAddress = familyCase?.FamilyVisitDetail?.RelativeAddress,
            VisitStartDate = familyCase?.ValidFrom ?? emp?.FamilyVisitStartDate,
            VisitEndDate = familyCase?.ValidTo ?? emp?.FamilyVisitEndDate,

            ElectronicIdNumber = activeEid?.IdentityNumber,
            ElectronicIdValidFrom = activeEid?.ValidFrom,
            ElectronicIdValidTo = activeEid?.ValidTo,
            ValidDocumentCount = validDocs.Count > 0
                ? validDocs.Count
                : (!string.IsNullOrWhiteSpace(emp?.VisaNumber) || !string.IsNullOrWhiteSpace(stu?.VisaNumber) ? 1 : 0),
            PrimaryDocumentType = primaryDoc?.TypeCode ?? ImmigrationDocumentTypeCodes.Visa,
            PrimaryDocumentNumber = primaryDoc?.Number ?? emp?.VisaNumber ?? stu?.VisaNumber,
            PrimaryDocumentValidFrom = primaryDoc?.ValidFrom,
            PrimaryDocumentValidTo = primaryDoc?.ValidTo ?? emp?.TemporaryStay ?? stu?.VisaExpiry,
            ImmigrationDocuments = person?.ImmigrationDocuments.Select(d => new ImmigrationDocumentDto
            {
                Id = d.Id,
                ForeignPersonId = d.ForeignPersonId,
                TypeCode = d.TypeCode,
                Number = d.Number,
                IssuedAt = d.IssuedAt,
                ValidFrom = d.ValidFrom,
                ValidTo = d.ValidTo,
                IssuedBy = d.IssuedBy,
                StatusCode = d.StatusCode,
                Note = d.Note
            }).ToList() ?? [],
            ElectronicIdentities = person?.ElectronicIdentities.Select(e => new ElectronicIdentityDto
            {
                Id = e.Id,
                ForeignPersonId = e.ForeignPersonId,
                IdentityNumber = e.IdentityNumber,
                IdentitySearchKey = e.IdentitySearchKey,
                IssuedAt = e.IssuedAt,
                ValidFrom = e.ValidFrom,
                ValidTo = e.ValidTo,
                StatusCode = e.StatusCode
            }).ToList() ?? []
        };
    }

    public async Task<int> SaveUnifiedProfileAsync(UnifiedForeignPersonInput input, CancellationToken cancellationToken = default)
    {
        if (_guard is not null)
            await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);

        if (string.IsNullOrWhiteSpace(input.PassportNumber))
            throw new ArgumentException("Số hộ chiếu là bắt buộc.");
        var safeFullName = (input.FullName ?? "").Trim();

        var activeNationalities = await _db.Nationality.AsNoTracking()
            .Where(n => n.Delete_flag == 0)
            .Select(n => n.NationalityCode)
            .ToListAsync(cancellationToken);

        string? resolvedNationality = null;
        if (!string.IsNullOrWhiteSpace(input.NationalityCode))
        {
            var trimmedNat = input.NationalityCode.Trim();
            var matchedNat = activeNationalities.FirstOrDefault(c => string.Equals(c, trimmedNat, StringComparison.OrdinalIgnoreCase));
            if (matchedNat is null)
            {
                // Check if it exists as a deactivated legacy nationality in DB
                var anyExistingNat = await _db.Nationality.AsNoTracking()
                    .Where(n => n.NationalityCode == trimmedNat)
                    .Select(n => n.NationalityCode)
                    .FirstOrDefaultAsync(cancellationToken);
                if (anyExistingNat is null)
                    throw new ArgumentException($"Mã quốc tịch '{trimmedNat}' không có trong danh mục Quốc tịch đang hoạt động.");
                resolvedNationality = anyExistingNat;
            }
            else
            {
                resolvedNationality = matchedNat;
            }
        }

        if (input.HasWorkProfile && (!input.CareerId.HasValue || input.CareerId.Value <= 0))
        {
            var defaultCareerId = await _db.Careers.AsNoTracking()
                .OrderBy(c => c.IDCareer)
                .Select(c => (int?)c.IDCareer)
                .FirstOrDefaultAsync(cancellationToken);
            if (defaultCareerId.HasValue)
                input.CareerId = defaultCareerId.Value;
        }

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            var normalizedPassport = NormalizePassport(input.PassportNumber);
            var passportKey = SearchKey(input.PassportNumber);

            ForeignPerson? person = null;
            if (input.ForeignPersonId.HasValue && input.ForeignPersonId.Value > 0)
            {
                person = await _db.ForeignPersons
                    .Include(x => x.SourceLinks)
                    .Include(x => x.StayCases).ThenInclude(s => s.FamilyVisitDetail)
                    .FirstOrDefaultAsync(x => x.Id == input.ForeignPersonId.Value, cancellationToken);
            }

            if (person is null && !string.IsNullOrEmpty(passportKey))
            {
                person = await _db.ForeignPersons
                    .Include(x => x.SourceLinks)
                    .Include(x => x.StayCases).ThenInclude(s => s.FamilyVisitDetail)
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.PassportSearchKey == passportKey, cancellationToken);
            }

            if (person is null)
            {
                person = new ForeignPerson
                {
                    FullName = safeFullName,
                    Gender = input.Gender,
                    Birthday = input.Birthday,
                    NationalityCode = resolvedNationality,
                    PassportNumber = normalizedPassport,
                    PassportSearchKey = passportKey,
                    IsDataIncomplete = string.IsNullOrWhiteSpace(safeFullName) || passportKey is null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.ForeignPersons.Add(person);
                await _db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                person.FullName = safeFullName;
                person.Gender = input.Gender;
                person.Birthday = input.Birthday;
                person.NationalityCode = resolvedNationality;
                person.PassportNumber = normalizedPassport;
                person.PassportSearchKey = passportKey;
                person.IsDataIncomplete = string.IsNullOrWhiteSpace(safeFullName) || passportKey is null;
                person.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
            }

            // 1. Diện Lao động (Employee + StayCase WORK)
            if (input.HasWorkProfile)
            {
                Employee? emp = null;
                if (input.EmployeeId.HasValue && input.EmployeeId.Value > 0)
                {
                    emp = await _db.Employees.FirstOrDefaultAsync(e => e.IDEmployee == input.EmployeeId.Value, cancellationToken);
                }

                if (emp is null)
                {
                    var existingEmpLink = await _db.ForeignPersonSourceLinks
                        .FirstOrDefaultAsync(l => l.ForeignPersonId == person.Id && l.SourceType == "EMPLOYEE", cancellationToken);
                    if (existingEmpLink is not null && int.TryParse(existingEmpLink.SourceId, out var linkedEmpId))
                    {
                        emp = await _db.Employees.FirstOrDefaultAsync(e => e.IDEmployee == linkedEmpId, cancellationToken);
                    }
                }

                var isNewEmp = emp is null;
                emp ??= new Employee
                {
                    DateCreated = DateTime.Now,
                    IDUser = 1,
                    Hidden_flag = 0,
                    WorkingStatus = 0
                };

                emp.StaffName = safeFullName;
                emp.Gender = input.Gender == 1 ? Gender.Male : Gender.Female;
                emp.Birthday = input.Birthday;
                emp.Nationality = resolvedNationality;
                emp.Passport = normalizedPassport ?? input.PassportNumber;
                emp.Address = input.ResidenceAddress;
                emp.IDCompany = input.CompanyId is > 0 ? input.CompanyId : null;
                emp.IDCareer = input.CareerId is > 0 ? input.CareerId : null;
                emp.WorkPermit = input.WorkPermit;
                emp.WorkPermitNumber = input.WorkPermitNumber;
                emp.TemporaryStay = input.TemporaryStay ?? input.DocumentValidTo;
                emp.VisaNumber = input.DocumentNumber;
                emp.VisaSymbol = input.VisaSymbol;
                emp.Note = input.Note;
                emp.Hidden_flag = 0;

                if (input.HasFamilyVisitProfile)
                {
                    emp.FamilyVisit = 1;
                    emp.FamilyVisitRelativeName = input.RelativeName;
                    emp.FamilyVisitRelationship = input.Relationship;
                    emp.FamilyVisitRelativeIdCard = input.RelativeIdNumber;
                    emp.FamilyVisitStartDate = input.VisitStartDate;
                    emp.FamilyVisitEndDate = input.VisitEndDate;
                    emp.FamilyVisitNote = input.Note;
                }

                if (isNewEmp)
                    _db.Employees.Add(emp);

                await _db.SaveChangesAsync(cancellationToken);

                var empSourceId = emp.IDEmployee.ToString();
                if (!await _db.ForeignPersonSourceLinks.AnyAsync(l => l.SourceType == "EMPLOYEE" && l.SourceId == empSourceId, cancellationToken))
                {
                    _db.ForeignPersonSourceLinks.Add(new ForeignPersonSourceLink
                    {
                        ForeignPersonId = person.Id,
                        SourceType = "EMPLOYEE",
                        SourceId = empSourceId,
                        LastSynchronizedAt = DateTime.UtcNow
                    });
                }

                var workStay = await _db.StayCases.FirstOrDefaultAsync(
                    s => s.ForeignPersonId == person.Id && s.PurposeCode == StayPurposeCodes.Work && s.StatusCode != "DELETED",
                    cancellationToken);
                var normalizedCompanyId = input.CompanyId is > 0 ? input.CompanyId : null;
                if (workStay is null)
                {
                    _db.StayCases.Add(new StayCase
                    {
                        ForeignPersonId = person.Id,
                        PurposeCode = StayPurposeCodes.Work,
                        IsPrimary = true,
                        SponsorCompanyId = normalizedCompanyId,
                        ValidFrom = (input.DocumentValidFrom ?? DateTime.Today).Date,
                        ValidTo = (input.TemporaryStay ?? input.DocumentValidTo)?.Date,
                        StatusCode = "ACTIVE",
                        Note = input.Note
                    });
                }
                else
                {
                    workStay.SponsorCompanyId = normalizedCompanyId;
                    workStay.ValidTo = (input.TemporaryStay ?? input.DocumentValidTo)?.Date;
                    workStay.Note = input.Note;
                    workStay.UpdatedAt = DateTime.UtcNow;
                }
            }

            // 2. Diện Du học sinh (Student + StayCase STUDY)
            if (input.HasStudyProfile)
            {
                Student? stu = null;
                if (input.StudentId.HasValue && input.StudentId.Value > 0)
                {
                    stu = await _db.Students.FirstOrDefaultAsync(s => s.IDStudent == input.StudentId.Value, cancellationToken);
                }

                if (stu is null)
                {
                    var existingStuLink = await _db.ForeignPersonSourceLinks
                        .FirstOrDefaultAsync(l => l.ForeignPersonId == person.Id && l.SourceType == "STUDENT", cancellationToken);
                    if (existingStuLink is not null && int.TryParse(existingStuLink.SourceId, out var linkedStuId))
                    {
                        stu = await _db.Students.FirstOrDefaultAsync(s => s.IDStudent == linkedStuId, cancellationToken);
                    }
                }

                var isNewStu = stu is null;
                stu ??= new Student
                {
                    DateCreated = DateTime.Now,
                    IDUser = 1,
                    Hidden_flag = 0
                };

                stu.FullName = safeFullName;
                stu.Gender = input.Gender == 1 ? Gender.Male : Gender.Female;
                stu.Birthday = input.Birthday;
                stu.Nationality = resolvedNationality;
                stu.Passport = normalizedPassport ?? input.PassportNumber;
                stu.SchoolName = input.SchoolName;
                stu.Major = input.Major;
                stu.EducationLevel = input.EducationLevel;
                stu.ScholarshipType = input.ScholarshipType;
                stu.EnrollmentDate = input.EnrollmentDate;
                stu.ExpectedGraduation = input.GraduationDate;
                stu.Status = input.StudentStatus;
                stu.VisaNumber = input.DocumentNumber;
                stu.VisaExpiry = input.VisaExpiry ?? input.DocumentValidTo;
                stu.TemporaryStay = input.TemporaryStay ?? input.VisaExpiry ?? input.DocumentValidTo;
                stu.Address = input.ResidenceAddress;
                stu.StudentCode = input.PhoneNumber;
                stu.Note = input.Note;
                stu.Hidden_flag = 0;

                if (isNewStu)
                    _db.Students.Add(stu);

                await _db.SaveChangesAsync(cancellationToken);

                var stuSourceId = stu.IDStudent.ToString();
                if (!await _db.ForeignPersonSourceLinks.AnyAsync(l => l.SourceType == "STUDENT" && l.SourceId == stuSourceId, cancellationToken))
                {
                    _db.ForeignPersonSourceLinks.Add(new ForeignPersonSourceLink
                    {
                        ForeignPersonId = person.Id,
                        SourceType = "STUDENT",
                        SourceId = stuSourceId,
                        LastSynchronizedAt = DateTime.UtcNow
                    });
                }

                var studyStay = await _db.StayCases.FirstOrDefaultAsync(
                    s => s.ForeignPersonId == person.Id && s.PurposeCode == StayPurposeCodes.Study && s.StatusCode != "DELETED",
                    cancellationToken);
                if (studyStay is null)
                {
                    _db.StayCases.Add(new StayCase
                    {
                        ForeignPersonId = person.Id,
                        PurposeCode = StayPurposeCodes.Study,
                        IsPrimary = !input.HasWorkProfile,
                        ValidFrom = (input.EnrollmentDate ?? DateTime.Today).Date,
                        ValidTo = (input.VisaExpiry ?? input.GraduationDate)?.Date,
                        StatusCode = "ACTIVE",
                        Note = input.Note
                    });
                }
                else
                {
                    studyStay.ValidTo = (input.VisaExpiry ?? input.GraduationDate)?.Date;
                    studyStay.Note = input.Note;
                    studyStay.UpdatedAt = DateTime.UtcNow;
                }
            }

            // 3. Diện Thăm thân (StayCase FAMILY_VISIT + FamilyVisitDetail)
            if (input.HasFamilyVisitProfile)
            {
                var fvStay = input.FamilyVisitStayCaseId.HasValue
                    ? await _db.StayCases.Include(x => x.FamilyVisitDetail)
                        .FirstOrDefaultAsync(x => x.Id == input.FamilyVisitStayCaseId.Value, cancellationToken)
                    : await _db.StayCases.Include(x => x.FamilyVisitDetail)
                        .FirstOrDefaultAsync(x => x.ForeignPersonId == person.Id && x.PurposeCode == StayPurposeCodes.FamilyVisit && x.StatusCode != "DELETED", cancellationToken);

                if (fvStay is null)
                {
                    fvStay = new StayCase
                    {
                        ForeignPersonId = person.Id,
                        PurposeCode = StayPurposeCodes.FamilyVisit,
                        IsPrimary = !input.HasWorkProfile && !input.HasStudyProfile
                    };
                    _db.StayCases.Add(fvStay);
                }

                fvStay.ValidFrom = (input.VisitStartDate ?? DateTime.Today).Date;
                fvStay.ValidTo = input.VisitEndDate?.Date;
                fvStay.SponsorCompanyId = (input.SponsorCompanyId is > 0 ? input.SponsorCompanyId : null)
                    ?? (input.CompanyId is > 0 ? input.CompanyId : null);
                fvStay.SponsorForeignPersonId = input.SponsorForeignPersonId;
                fvStay.StatusCode = "ACTIVE";
                fvStay.Note = input.Note;
                fvStay.UpdatedAt = DateTime.UtcNow;

                fvStay.FamilyVisitDetail ??= new FamilyVisitDetail { StayCase = fvStay };
                fvStay.FamilyVisitDetail.RelativeName = (input.RelativeName ?? "").Trim();
                fvStay.FamilyVisitDetail.Relationship = (input.Relationship ?? "").Trim();
                fvStay.FamilyVisitDetail.RelativeTypeCode = string.IsNullOrWhiteSpace(input.RelativeTypeCode) ? "VIETNAMESE_CITIZEN" : input.RelativeTypeCode;
                fvStay.FamilyVisitDetail.RelativeNationalityCode = input.RelativeNationalityCode;
                fvStay.FamilyVisitDetail.RelativeIdNumber = input.RelativeIdNumber;
                fvStay.FamilyVisitDetail.RelativePhone = input.RelativePhone;
                fvStay.FamilyVisitDetail.RelativeAddress = input.RelativeAddress;
            }

            // Nếu không tích diện cụ thể nào, vẫn đảm bảo có 1 StayCase theo PrimaryPurposeCode
            if (!input.HasWorkProfile && !input.HasStudyProfile && !input.HasFamilyVisitProfile)
            {
                var purpose = string.IsNullOrWhiteSpace(input.PrimaryPurposeCode) ? StayPurposeCodes.Other : input.PrimaryPurposeCode;
                var anyStay = await _db.StayCases.AnyAsync(s => s.ForeignPersonId == person.Id && s.StatusCode != "DELETED", cancellationToken);
                if (!anyStay)
                {
                    _db.StayCases.Add(new StayCase
                    {
                        ForeignPersonId = person.Id,
                        PurposeCode = purpose,
                        IsPrimary = true,
                        ValidFrom = DateTime.Today,
                        StatusCode = "ACTIVE",
                        Note = input.Note
                    });
                }
            }

            // 4. Địa bàn & Địa chỉ tạm trú (ResidencePeriod)
            if (input.AdministrativeUnitId.HasValue && input.AdministrativeUnitId.Value > 0 && !string.IsNullOrWhiteSpace(input.ResidenceAddress))
            {
                var existingRes = await _db.ResidencePeriods
                    .OrderByDescending(r => r.ValidFrom)
                    .FirstOrDefaultAsync(r => r.ForeignPersonId == person.Id, cancellationToken);

                var normalizedResponsibleCompanyId = input.CompanyId is > 0 ? input.CompanyId : null;
                if (existingRes is null)
                {
                    _db.ResidencePeriods.Add(new ResidencePeriod
                    {
                        ForeignPersonId = person.Id,
                        AdministrativeUnitId = input.AdministrativeUnitId.Value,
                        AddressLine = input.ResidenceAddress.Trim(),
                        ResponsibleCompanyId = normalizedResponsibleCompanyId,
                        ValidFrom = (input.VisitStartDate ?? input.DocumentValidFrom ?? DateTime.Today).Date,
                        ValidTo = (input.TemporaryStay ?? input.VisitEndDate ?? input.DocumentValidTo)?.Date
                    });
                }
                else
                {
                    existingRes.AdministrativeUnitId = input.AdministrativeUnitId.Value;
                    existingRes.AddressLine = input.ResidenceAddress.Trim();
                    existingRes.ResponsibleCompanyId = normalizedResponsibleCompanyId;
                    existingRes.ValidTo = (input.TemporaryStay ?? input.VisitEndDate ?? input.DocumentValidTo)?.Date;
                }
            }

            // 5. Giấy tờ XNC (ImmigrationDocument)
            if (!string.IsNullOrWhiteSpace(input.DocumentNumber))
            {
                var docNum = input.DocumentNumber.Trim();
                var docType = string.IsNullOrWhiteSpace(input.DocumentTypeCode) ? ImmigrationDocumentTypeCodes.Visa : input.DocumentTypeCode;
                var existingDoc = await _db.ImmigrationDocuments
                    .FirstOrDefaultAsync(d => d.ForeignPersonId == person.Id && d.Number == docNum, cancellationToken);

                if (existingDoc is null)
                {
                    _db.ImmigrationDocuments.Add(new ImmigrationDocument
                    {
                        ForeignPersonId = person.Id,
                        TypeCode = docType,
                        Number = docNum,
                        ValidFrom = (input.DocumentValidFrom ?? DateTime.Today).Date,
                        ValidTo = (input.DocumentValidTo ?? input.TemporaryStay ?? input.VisaExpiry)?.Date,
                        StatusCode = "VALID",
                        Note = input.VisaSymbol
                    });
                }
                else
                {
                    existingDoc.TypeCode = docType;
                    existingDoc.ValidFrom = (input.DocumentValidFrom ?? existingDoc.ValidFrom).Date;
                    existingDoc.ValidTo = (input.DocumentValidTo ?? input.TemporaryStay ?? input.VisaExpiry)?.Date;
                    existingDoc.Note = input.VisaSymbol ?? existingDoc.Note;
                }
            }

            // 6. Định danh điện tử (ElectronicIdentity)
            if (!string.IsNullOrWhiteSpace(input.ElectronicIdentityNumber))
            {
                var eidNum = input.ElectronicIdentityNumber.Trim();
                var eidKey = SearchKey(eidNum);
                if (!string.IsNullOrEmpty(eidKey))
                {
                    var existingEid = await _db.ElectronicIdentities
                        .FirstOrDefaultAsync(e => e.ForeignPersonId == person.Id && e.IdentitySearchKey == eidKey, cancellationToken);

                    if (existingEid is null)
                    {
                        _db.ElectronicIdentities.Add(new ElectronicIdentity
                        {
                            ForeignPersonId = person.Id,
                            IdentityNumber = eidNum,
                            IdentitySearchKey = eidKey,
                            ValidFrom = (input.ElectronicIdentityValidFrom ?? DateTime.Today).Date,
                            ValidTo = input.ElectronicIdentityValidTo?.Date,
                            StatusCode = "ACTIVE"
                        });
                    }
                    else
                    {
                        existingEid.IdentityNumber = eidNum;
                        existingEid.ValidTo = input.ElectronicIdentityValidTo?.Date;
                        existingEid.StatusCode = "ACTIVE";
                    }
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            await _audit.LogAsync(input.ForeignPersonId.HasValue ? "UPDATE" : "CREATE", "ForeignPerson", person.Id,
                $"Lưu hồ sơ hợp nhất: {person.FullName}");
            await transaction.CommitAsync(cancellationToken);
            return person.Id;
        });
    }

    public async Task DeleteUnifiedProfileAsync(int? foreignPersonId, int? employeeId, int? studentId, CancellationToken cancellationToken = default)
    {
        if (_guard is not null)
            await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);

        if (foreignPersonId.HasValue && foreignPersonId.Value > 0)
        {
            var person = await _db.ForeignPersons
                .Include(x => x.SourceLinks)
                .FirstOrDefaultAsync(x => x.Id == foreignPersonId.Value, cancellationToken);
            if (person is not null)
            {
                person.IsDeleted = true;
                person.UpdatedAt = DateTime.UtcNow;
                foreach (var stay in await _db.StayCases.Where(x => x.ForeignPersonId == person.Id).ToListAsync(cancellationToken))
                    stay.StatusCode = "DELETED";

                foreach (var link in person.SourceLinks)
                {
                    if (string.Equals(link.SourceType, "EMPLOYEE", StringComparison.OrdinalIgnoreCase) && int.TryParse(link.SourceId, out var linkedEmpId))
                        employeeId ??= linkedEmpId;
                    if (string.Equals(link.SourceType, "STUDENT", StringComparison.OrdinalIgnoreCase) && int.TryParse(link.SourceId, out var linkedStuId))
                        studentId ??= linkedStuId;
                }
            }
        }

        if (employeeId.HasValue && employeeId.Value > 0)
        {
            var emp = await _db.Employees.FindAsync([employeeId.Value], cancellationToken);
            if (emp is not null)
                emp.Hidden_flag = 1;
        }

        if (studentId.HasValue && studentId.Value > 0)
        {
            var stu = await _db.Students.FindAsync([studentId.Value], cancellationToken);
            if (stu is not null)
                stu.Hidden_flag = 1;
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("DELETE", "ForeignPerson", foreignPersonId ?? employeeId ?? studentId, "Xóa mềm hồ sơ NNN hợp nhất");
    }

    public async Task<LegacyDatabaseImportResult> ImportFromLegacyDatabaseAsync(
        string sourceConnectionStringOrDbName,
        CancellationToken cancellationToken = default)
    {
        if (_guard is not null)
            await _guard.RequireAnyRoleAsync(IrmRoles.Admin);

        if (string.IsNullOrWhiteSpace(sourceConnectionStringOrDbName))
            throw new ArgumentException("Vui lòng nhập tên database cũ (VD: ReportManagerDB) hoặc chuỗi kết nối SQL Server.");

        var input = sourceConnectionStringOrDbName.Trim();
        string sourceConnStr;
        if (input.Contains('=', StringComparison.Ordinal))
        {
            sourceConnStr = input;
        }
        else
        {
            var currentConnStr = _db.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Không lấy được chuỗi kết nối SQL Server hiện tại.");
            var builder = new SqlConnectionStringBuilder(currentConnStr)
            {
                InitialCatalog = input
            };
            sourceConnStr = builder.ConnectionString;
        }

        int natAdded = 0, fieldsAdded = 0, careersAdded = 0, districtsAdded = 0, wardsAdded = 0, companiesAdded = 0, employeesAdded = 0;

        await using var conn = new SqlConnection(sourceConnStr);
        await conn.OpenAsync(cancellationToken);

        // 1. Nationality
        if (await TableExistsAsync(conn, "Nationality", cancellationToken))
        {
            var existingNats = (await _db.Nationality.Select(n => n.NationalityCode).ToListAsync(cancellationToken))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            await using var cmd = new SqlCommand("SELECT NationalityCode, NationalityName, ISNULL(Delete_flag, 0) FROM dbo.Nationality", conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var code = reader.IsDBNull(0) ? null : reader.GetString(0).Trim();
                var name = reader.IsDBNull(1) ? "" : reader.GetString(1).Trim();
                var del = reader.GetInt32(2);
                if (!string.IsNullOrWhiteSpace(code) && !existingNats.Contains(code))
                {
                    _db.Nationality.Add(new NationalityEntity
                    {
                        NationalityCode = code,
                        NationalityName = string.IsNullOrWhiteSpace(name) ? code : name,
                        Delete_flag = del
                    });
                    existingNats.Add(code);
                    natAdded++;
                }
            }
            await _db.SaveChangesAsync(cancellationToken);
        }

        // 2. Fields
        var fieldMap = new Dictionary<int, int>();
        if (await TableExistsAsync(conn, "Fields", cancellationToken))
        {
            var existingFields = await _db.Fields.ToListAsync(cancellationToken);
            await using var cmd = new SqlCommand("SELECT IDField, FieldName, Description, ISNULL(Delete_flag, 0) FROM dbo.Fields", conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var rows = new List<(int Id, string Name, string? Desc, int Del)>();
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add((reader.GetInt32(0), reader.IsDBNull(1) ? "" : reader.GetString(1).Trim(),
                    reader.IsDBNull(2) ? null : reader.GetString(2), reader.GetInt32(3)));
            }
            await reader.CloseAsync();

            foreach (var r in rows)
            {
                var match = existingFields.FirstOrDefault(f => string.Equals(f.FieldName, r.Name, StringComparison.OrdinalIgnoreCase));
                if (match is null && !string.IsNullOrWhiteSpace(r.Name))
                {
                    match = new Field { FieldName = r.Name, Description = r.Desc, Delete_flag = r.Del };
                    _db.Fields.Add(match);
                    await _db.SaveChangesAsync(cancellationToken);
                    existingFields.Add(match);
                    fieldsAdded++;
                }
                if (match is not null)
                    fieldMap[r.Id] = match.IDField;
            }
        }

        // 3. CareerGroups & Careers
        var careerGroupMap = new Dictionary<int, int>();
        if (await TableExistsAsync(conn, "CareerGroups", cancellationToken))
        {
            var existingGroups = await _db.CareerGroups.ToListAsync(cancellationToken);
            await using var cmd = new SqlCommand("SELECT IDCG, CareerGroupName FROM dbo.CareerGroups", conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var rows = new List<(int Id, string Name)>();
            while (await reader.ReadAsync(cancellationToken))
                rows.Add((reader.GetInt32(0), reader.IsDBNull(1) ? "Khác" : reader.GetString(1).Trim()));
            await reader.CloseAsync();

            foreach (var r in rows)
            {
                var match = existingGroups.FirstOrDefault(g => string.Equals(g.CareerGroupName, r.Name, StringComparison.OrdinalIgnoreCase));
                if (match is null)
                {
                    match = new CareerGroup { CareerGroupName = r.Name };
                    _db.CareerGroups.Add(match);
                    await _db.SaveChangesAsync(cancellationToken);
                    existingGroups.Add(match);
                }
                careerGroupMap[r.Id] = match.IDCG;
            }
        }

        var defaultCareerGroupId = (await _db.CareerGroups.Select(g => (int?)g.IDCG).FirstOrDefaultAsync(cancellationToken)) ?? 1;
        var careerMap = new Dictionary<int, int>();
        if (await TableExistsAsync(conn, "Careers", cancellationToken))
        {
            var existingCareers = await _db.Careers.ToListAsync(cancellationToken);
            await using var cmd = new SqlCommand("SELECT IDCareer, CareerName, IDCG FROM dbo.Careers", conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var rows = new List<(int Id, string Name, int GroupId)>();
            while (await reader.ReadAsync(cancellationToken))
                rows.Add((reader.GetInt32(0), reader.IsDBNull(1) ? "" : reader.GetString(1).Trim(), reader.IsDBNull(2) ? defaultCareerGroupId : reader.GetInt32(2)));
            await reader.CloseAsync();

            foreach (var r in rows)
            {
                var match = existingCareers.FirstOrDefault(c => string.Equals(c.CareerName, r.Name, StringComparison.OrdinalIgnoreCase));
                if (match is null && !string.IsNullOrWhiteSpace(r.Name))
                {
                    match = new Career
                    {
                        CareerName = r.Name,
                        IDCG = careerGroupMap.GetValueOrDefault(r.GroupId, defaultCareerGroupId)
                    };
                    _db.Careers.Add(match);
                    await _db.SaveChangesAsync(cancellationToken);
                    existingCareers.Add(match);
                    careersAdded++;
                }
                if (match is not null)
                    careerMap[r.Id] = match.IDCareer;
            }
        }

        // 4. Districts & Wards
        var districtMap = new Dictionary<int, int>();
        if (await TableExistsAsync(conn, "Districts", cancellationToken))
        {
            var existingDistricts = await _db.Districts.ToListAsync(cancellationToken);
            await using var cmd = new SqlCommand("SELECT IDDistrict, DisTrictName, ISNULL(Delete_flag, 0) FROM dbo.Districts", conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var rows = new List<(int Id, string Name, int Del)>();
            while (await reader.ReadAsync(cancellationToken))
                rows.Add((reader.GetInt32(0), reader.IsDBNull(1) ? "" : reader.GetString(1).Trim(), reader.GetInt32(2)));
            await reader.CloseAsync();

            foreach (var r in rows)
            {
                var match = existingDistricts.FirstOrDefault(d => string.Equals(d.DisTrictName, r.Name, StringComparison.OrdinalIgnoreCase));
                if (match is null && !string.IsNullOrWhiteSpace(r.Name))
                {
                    match = new District { DisTrictName = r.Name, Delete_flag = r.Del };
                    _db.Districts.Add(match);
                    await _db.SaveChangesAsync(cancellationToken);
                    existingDistricts.Add(match);
                    districtsAdded++;
                }
                if (match is not null)
                    districtMap[r.Id] = match.IDDistrict;
            }
        }

        if (await TableExistsAsync(conn, "Wards", cancellationToken))
        {
            var existingWards = await _db.Wards.ToListAsync(cancellationToken);
            var existingAdminUnits = await _db.AdministrativeUnits.ToListAsync(cancellationToken);

            await using var cmd = new SqlCommand("SELECT IDWard, WardName, ISNULL(Delete_flag, 0) FROM dbo.Wards", conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var rows = new List<(int Id, string Name, int Del)>();
            while (await reader.ReadAsync(cancellationToken))
                rows.Add((reader.GetInt32(0), reader.IsDBNull(1) ? "" : reader.GetString(1).Trim(), reader.GetInt32(2)));
            await reader.CloseAsync();

            foreach (var r in rows)
            {
                var match = existingWards.FirstOrDefault(w => string.Equals(w.WardName, r.Name, StringComparison.OrdinalIgnoreCase));
                if (match is null && !string.IsNullOrWhiteSpace(r.Name))
                {
                    match = new Ward
                    {
                        WardName = r.Name,
                        Delete_flag = r.Del
                    };
                    _db.Wards.Add(match);
                    await _db.SaveChangesAsync(cancellationToken);
                    existingWards.Add(match);
                    wardsAdded++;
                }

                if (r.Del == 0 && !string.IsNullOrWhiteSpace(r.Name)
                    && !existingAdminUnits.Any(u => string.Equals(u.Name, r.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    var code = $"LEGACY-W-{r.Id:D4}";
                    if (!existingAdminUnits.Any(u => string.Equals(u.Code, code, StringComparison.OrdinalIgnoreCase)))
                    {
                        var unit = new AdministrativeUnit
                        {
                            Code = code,
                            Name = r.Name,
                            TypeCode = r.Name.StartsWith("Xã", StringComparison.OrdinalIgnoreCase)
                                ? AdministrativeUnitTypeCodes.Commune
                                : AdministrativeUnitTypeCodes.Ward,
                            ValidFrom = new DateTime(2020, 1, 1),
                            LegacyWardId = match?.IDWard
                        };
                        _db.AdministrativeUnits.Add(unit);
                        existingAdminUnits.Add(unit);
                    }
                }
            }
            await _db.SaveChangesAsync(cancellationToken);
        }

        // 5. Companies
        var companyMap = new Dictionary<int, int>();
        var defaultFieldId = (await _db.Fields.Select(f => (int?)f.IDField).FirstOrDefaultAsync(cancellationToken)) ?? 1;
        var defaultUserId = (await _db.Accounts.Select(a => (int?)a.IDUser).FirstOrDefaultAsync(cancellationToken)) ?? 1;
        if (await TableExistsAsync(conn, "Companies", cancellationToken))
        {
            var existingCompanies = await _db.Companies.ToListAsync(cancellationToken);
            await using var cmd = new SqlCommand(
                "SELECT IDCompany, CompanyName, TypeOfBusiniess, IDField, LegalRepresentative, Address, Note, ISNULL(Delete_flag, 0) FROM dbo.Companies",
                conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var rows = new List<(int Id, string Name, string? Type, int FieldId, string? Rep, string? Addr, string? Note, int Del)>();
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add((
                    reader.GetInt32(0),
                    reader.IsDBNull(1) ? "" : reader.GetString(1).Trim(),
                    reader.IsDBNull(2) ? null : reader.GetString(2),
                    reader.IsDBNull(3) ? defaultFieldId : reader.GetInt32(3),
                    reader.IsDBNull(4) ? null : reader.GetString(4),
                    reader.IsDBNull(5) ? null : reader.GetString(5),
                    reader.IsDBNull(6) ? null : reader.GetString(6),
                    reader.GetInt32(7)));
            }
            await reader.CloseAsync();

            foreach (var r in rows)
            {
                var match = existingCompanies.FirstOrDefault(c => string.Equals(c.CompanyName, r.Name, StringComparison.OrdinalIgnoreCase));
                if (match is null && !string.IsNullOrWhiteSpace(r.Name))
                {
                    match = new Company
                    {
                        CompanyName = r.Name,
                        TypeOfBusiniess = r.Type,
                        IDField = fieldMap.GetValueOrDefault(r.FieldId, defaultFieldId),
                        LegalRepresentative = r.Rep,
                        Address = r.Addr,
                        Note = r.Note,
                        TrackerID = defaultUserId,
                        Delete_flag = r.Del
                    };
                    _db.Companies.Add(match);
                    await _db.SaveChangesAsync(cancellationToken);
                    existingCompanies.Add(match);
                    companiesAdded++;
                }
                if (match is not null)
                    companyMap[r.Id] = match.IDCompany;
            }
        }

        // 6. Employees
        var defaultCompanyId = (await _db.Companies.Select(c => (int?)c.IDCompany).FirstOrDefaultAsync(cancellationToken)) ?? 1;
        var defaultCareerId = (await _db.Careers.Select(c => (int?)c.IDCareer).FirstOrDefaultAsync(cancellationToken)) ?? 1;
        if (await TableExistsAsync(conn, "Employees", cancellationToken))
        {
            var existingEmployees = await _db.Employees.ToListAsync(cancellationToken);
            var hasVisaSymbol = await ColumnExistsAsync(conn, "Employees", "VisaSymbol", cancellationToken);
            var hasFamilyVisit = await ColumnExistsAsync(conn, "Employees", "FamilyVisit", cancellationToken);

            var sql = $"""
                SELECT IDEmployee, StaffName, ISNULL(Gender, 1), Birthday, Nationality, Passport, Address,
                       ISNULL(IDCareer, 0), ISNULL(WorkPermit, 0), WorkPermitNumber, VisaNumber, TemporaryStay,
                       Note, ISNULL(IDCompany, 0), DateCreated, ISNULL(WorkingStatus, 0), ISNULL(Hidden_flag, 0),
                       {(hasVisaSymbol ? "VisaSymbol" : "CAST(NULL AS nvarchar(max))")},
                       {(hasFamilyVisit ? "ISNULL(FamilyVisit, 0), FamilyVisitRelativeName, FamilyVisitRelationship, FamilyVisitRelativeIdCard, FamilyVisitStartDate, FamilyVisitEndDate" : "0, NULL, NULL, NULL, NULL, NULL")}
                FROM dbo.Employees
                WHERE ISNULL(Hidden_flag, 0) = 0
                """;

            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var name = reader.IsDBNull(1) ? "" : reader.GetString(1).Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;

                var passport = reader.IsDBNull(5) ? null : reader.GetString(5).Trim();
                var oldCompId = reader.GetInt32(13);
                var mappedCompId = companyMap.GetValueOrDefault(oldCompId, defaultCompanyId);

                var duplicate = existingEmployees.Any(e =>
                    e.Hidden_flag == 0
                    && string.Equals(e.StaffName, name, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(NormalizePassport(e.Passport), NormalizePassport(passport), StringComparison.OrdinalIgnoreCase)
                    && e.IDCompany == mappedCompId);

                if (duplicate) continue;

                var oldCareerId = reader.GetInt32(7);
                var emp = new Employee
                {
                    StaffName = name,
                    Gender = (Gender)reader.GetInt32(2),
                    Birthday = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    Nationality = reader.IsDBNull(4) ? null : reader.GetString(4).Trim(),
                    Passport = passport,
                    Address = reader.IsDBNull(6) ? null : reader.GetString(6),
                    IDCareer = careerMap.GetValueOrDefault(oldCareerId, defaultCareerId),
                    WorkPermit = (WorkPermitType)reader.GetInt32(8),
                    WorkPermitNumber = reader.IsDBNull(9) ? null : reader.GetString(9),
                    VisaNumber = reader.IsDBNull(10) ? null : reader.GetString(10),
                    TemporaryStay = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                    Note = reader.IsDBNull(12) ? null : reader.GetString(12),
                    IDCompany = mappedCompId,
                    DateCreated = reader.IsDBNull(14) ? DateTime.Now : reader.GetDateTime(14),
                    WorkingStatus = reader.GetInt32(15),
                    Hidden_flag = reader.GetInt32(16),
                    IDUser = defaultUserId,
                    VisaSymbol = reader.IsDBNull(17) ? null : reader.GetString(17),
                    FamilyVisit = reader.GetInt32(18),
                    FamilyVisitRelativeName = reader.IsDBNull(19) ? null : reader.GetString(19),
                    FamilyVisitRelationship = reader.IsDBNull(20) ? null : reader.GetString(20),
                    FamilyVisitRelativeIdCard = reader.IsDBNull(21) ? null : reader.GetString(21),
                    FamilyVisitStartDate = reader.IsDBNull(22) ? null : reader.GetDateTime(22),
                    FamilyVisitEndDate = reader.IsDBNull(23) ? null : reader.GetDateTime(23)
                };

                _db.Employees.Add(emp);
                existingEmployees.Add(emp);
                employeesAdded++;
            }
            await reader.CloseAsync();
            await _db.SaveChangesAsync(cancellationToken);
        }

        var backfill = await BackfillLegacyAsync(cancellationToken);
        await _audit.LogAsync("IMPORT_LEGACY_DB", "Database", null,
            $"Nhập từ DB cũ '{input}': +{natAdded} quốc tịch, +{companiesAdded} công ty, +{employeesAdded} nhân viên, liên kết {backfill.Linked} hồ sơ");

        return new LegacyDatabaseImportResult(
            natAdded, fieldsAdded, careersAdded, districtsAdded, wardsAdded,
            companiesAdded, employeesAdded, backfill.Linked);
    }

    private static async Task<bool> TableExistsAsync(SqlConnection conn, string tableName, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("SELECT CASE WHEN OBJECT_ID(@name, N'U') IS NOT NULL THEN 1 ELSE 0 END", conn);
        cmd.Parameters.AddWithValue("@name", $"dbo.{tableName}");
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)) == 1;
    }

    private static async Task<bool> ColumnExistsAsync(SqlConnection conn, string tableName, string columnName, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("SELECT CASE WHEN COL_LENGTH(@table, @col) IS NOT NULL THEN 1 ELSE 0 END", conn);
        cmd.Parameters.AddWithValue("@table", $"dbo.{tableName}");
        cmd.Parameters.AddWithValue("@col", columnName);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)) == 1;
    }
}
