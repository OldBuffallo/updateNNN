using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

internal static partial class DtoMapper
{
    public static ArchivedEmployeeDto ToDto(ArchivedEmployee value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static ArchivedEmployeeDto ToDto(ArchivedEmployee value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ArchivedEmployeeDto)existing;
        var result = new ArchivedEmployeeDto();
        seen[value] = result;
        result.Id = value.Id;
        result.OriginalId = value.OriginalId;
        result.StaffName = value.StaffName;
        result.Gender = value.Gender;
        result.Birthday = value.Birthday;
        result.Nationality = value.Nationality;
        result.Passport = value.Passport;
        result.Address = value.Address;
        result.IDCareer = value.IDCareer;
        result.WorkPermit = value.WorkPermit;
        result.WorkPermitNumber = value.WorkPermitNumber;
        result.VisaNumber = value.VisaNumber;
        result.VisaSymbol = value.VisaSymbol;
        result.TemporaryStay = value.TemporaryStay;
        result.Note = value.Note;
        result.SettlementResults = value.SettlementResults;
        result.SettlementResultsString = value.SettlementResultsString;
        result.IDUser = value.IDUser;
        result.IDCompany = value.IDCompany;
        result.DateCreated = value.DateCreated;
        result.CardCreationDate = value.CardCreationDate;
        result.WorkingStatus = value.WorkingStatus;
        result.DateOfJoin = value.DateOfJoin;
        result.DateOfLeave = value.DateOfLeave;
        result.FamilyVisit = value.FamilyVisit;
        result.FamilyVisitRelativeName = value.FamilyVisitRelativeName;
        result.FamilyVisitRelationship = value.FamilyVisitRelationship;
        result.FamilyVisitRelativeIdCard = value.FamilyVisitRelativeIdCard;
        result.FamilyVisitStartDate = value.FamilyVisitStartDate;
        result.FamilyVisitEndDate = value.FamilyVisitEndDate;
        result.FamilyVisitNote = value.FamilyVisitNote;
        result.CompanyName = value.CompanyName;
        result.CareerName = value.CareerName;
        result.ArchiveReason = value.ArchiveReason;
        result.ArchivedBy = value.ArchivedBy;
        result.ArchivedAt = value.ArchivedAt;
        return result;
    }

    public static ArchivedEmployee ToEntity(ArchivedEmployeeDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static ArchivedEmployee ToEntity(ArchivedEmployeeDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (ArchivedEmployee)existing;
        var result = new ArchivedEmployee();
        seen[value] = result;
        result.Id = value.Id;
        result.OriginalId = value.OriginalId;
        result.StaffName = value.StaffName;
        result.Gender = value.Gender;
        result.Birthday = value.Birthday;
        result.Nationality = value.Nationality;
        result.Passport = value.Passport;
        result.Address = value.Address;
        result.IDCareer = value.IDCareer;
        result.WorkPermit = value.WorkPermit;
        result.WorkPermitNumber = value.WorkPermitNumber;
        result.VisaNumber = value.VisaNumber;
        result.VisaSymbol = value.VisaSymbol;
        result.TemporaryStay = value.TemporaryStay;
        result.Note = value.Note;
        result.SettlementResults = value.SettlementResults;
        result.SettlementResultsString = value.SettlementResultsString;
        result.IDUser = value.IDUser;
        result.IDCompany = value.IDCompany;
        result.DateCreated = value.DateCreated;
        result.CardCreationDate = value.CardCreationDate;
        result.WorkingStatus = value.WorkingStatus;
        result.DateOfJoin = value.DateOfJoin;
        result.DateOfLeave = value.DateOfLeave;
        result.FamilyVisit = value.FamilyVisit;
        result.FamilyVisitRelativeName = value.FamilyVisitRelativeName;
        result.FamilyVisitRelationship = value.FamilyVisitRelationship;
        result.FamilyVisitRelativeIdCard = value.FamilyVisitRelativeIdCard;
        result.FamilyVisitStartDate = value.FamilyVisitStartDate;
        result.FamilyVisitEndDate = value.FamilyVisitEndDate;
        result.FamilyVisitNote = value.FamilyVisitNote;
        result.CompanyName = value.CompanyName;
        result.CareerName = value.CareerName;
        result.ArchiveReason = value.ArchiveReason;
        result.ArchivedBy = value.ArchivedBy;
        result.ArchivedAt = value.ArchivedAt;
        return result;
    }

    public static void CopyToDto(ArchivedEmployee value, ArchivedEmployeeDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.OriginalId = mapped.OriginalId;
        target.StaffName = mapped.StaffName;
        target.Gender = mapped.Gender;
        target.Birthday = mapped.Birthday;
        target.Nationality = mapped.Nationality;
        target.Passport = mapped.Passport;
        target.Address = mapped.Address;
        target.IDCareer = mapped.IDCareer;
        target.WorkPermit = mapped.WorkPermit;
        target.WorkPermitNumber = mapped.WorkPermitNumber;
        target.VisaNumber = mapped.VisaNumber;
        target.VisaSymbol = mapped.VisaSymbol;
        target.TemporaryStay = mapped.TemporaryStay;
        target.Note = mapped.Note;
        target.SettlementResults = mapped.SettlementResults;
        target.SettlementResultsString = mapped.SettlementResultsString;
        target.IDUser = mapped.IDUser;
        target.IDCompany = mapped.IDCompany;
        target.DateCreated = mapped.DateCreated;
        target.CardCreationDate = mapped.CardCreationDate;
        target.WorkingStatus = mapped.WorkingStatus;
        target.DateOfJoin = mapped.DateOfJoin;
        target.DateOfLeave = mapped.DateOfLeave;
        target.FamilyVisit = mapped.FamilyVisit;
        target.FamilyVisitRelativeName = mapped.FamilyVisitRelativeName;
        target.FamilyVisitRelationship = mapped.FamilyVisitRelationship;
        target.FamilyVisitRelativeIdCard = mapped.FamilyVisitRelativeIdCard;
        target.FamilyVisitStartDate = mapped.FamilyVisitStartDate;
        target.FamilyVisitEndDate = mapped.FamilyVisitEndDate;
        target.FamilyVisitNote = mapped.FamilyVisitNote;
        target.CompanyName = mapped.CompanyName;
        target.CareerName = mapped.CareerName;
        target.ArchiveReason = mapped.ArchiveReason;
        target.ArchivedBy = mapped.ArchivedBy;
        target.ArchivedAt = mapped.ArchivedAt;
    }

    public static EmployeeDto ToDto(Employee value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static EmployeeDto ToDto(Employee value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (EmployeeDto)existing;
        var result = new EmployeeDto();
        seen[value] = result;
        result.IDEmployee = value.IDEmployee;
        result.StaffName = value.StaffName;
        result.Gender = value.Gender;
        result.Birthday = value.Birthday;
        result.Nationality = value.Nationality;
        result.Passport = value.Passport;
        result.Address = value.Address;
        result.IDCareer = value.IDCareer;
        result.WorkPermit = value.WorkPermit;
        result.WorkPermitNumber = value.WorkPermitNumber;
        result.VisaNumber = value.VisaNumber;
        result.VisaSymbol = value.VisaSymbol;
        result.TemporaryStay = value.TemporaryStay;
        result.Note = value.Note;
        result.SettlementResults = value.SettlementResults;
        result.SettlementResultsString = value.SettlementResultsString;
        result.IDUser = value.IDUser;
        result.IDCompany = value.IDCompany;
        result.DateCreated = value.DateCreated;
        result.CardCreationDate = value.CardCreationDate;
        result.WorkingStatus = value.WorkingStatus;
        result.DateOfJoin = value.DateOfJoin;
        result.DateOfLeave = value.DateOfLeave;
        result.Hidden_flag = value.Hidden_flag;
        result.FamilyVisit = value.FamilyVisit;
        result.FamilyVisitRelativeName = value.FamilyVisitRelativeName;
        result.FamilyVisitRelationship = value.FamilyVisitRelationship;
        result.FamilyVisitRelativeIdCard = value.FamilyVisitRelativeIdCard;
        result.FamilyVisitStartDate = value.FamilyVisitStartDate;
        result.FamilyVisitEndDate = value.FamilyVisitEndDate;
        result.FamilyVisitNote = value.FamilyVisitNote;
        result.Company = value.Company is null ? null : ToDto(value.Company, seen);
        result.Career = value.Career is null ? null : ToDto(value.Career, seen);
        result.NationalityNav = value.NationalityNav is null ? null : ToDto(value.NationalityNav, seen);
        return result;
    }

    public static Employee ToEntity(EmployeeDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static Employee ToEntity(EmployeeDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (Employee)existing;
        var result = new Employee();
        seen[value] = result;
        result.IDEmployee = value.IDEmployee;
        result.StaffName = value.StaffName;
        result.Gender = value.Gender;
        result.Birthday = value.Birthday;
        result.Nationality = value.Nationality;
        result.Passport = value.Passport;
        result.Address = value.Address;
        result.IDCareer = value.IDCareer;
        result.WorkPermit = value.WorkPermit;
        result.WorkPermitNumber = value.WorkPermitNumber;
        result.VisaNumber = value.VisaNumber;
        result.VisaSymbol = value.VisaSymbol;
        result.TemporaryStay = value.TemporaryStay;
        result.Note = value.Note;
        result.SettlementResults = value.SettlementResults;
        result.SettlementResultsString = value.SettlementResultsString;
        result.IDUser = value.IDUser;
        result.IDCompany = value.IDCompany;
        result.DateCreated = value.DateCreated;
        result.CardCreationDate = value.CardCreationDate;
        result.WorkingStatus = value.WorkingStatus;
        result.DateOfJoin = value.DateOfJoin;
        result.DateOfLeave = value.DateOfLeave;
        result.Hidden_flag = value.Hidden_flag;
        result.FamilyVisit = value.FamilyVisit;
        result.FamilyVisitRelativeName = value.FamilyVisitRelativeName;
        result.FamilyVisitRelationship = value.FamilyVisitRelationship;
        result.FamilyVisitRelativeIdCard = value.FamilyVisitRelativeIdCard;
        result.FamilyVisitStartDate = value.FamilyVisitStartDate;
        result.FamilyVisitEndDate = value.FamilyVisitEndDate;
        result.FamilyVisitNote = value.FamilyVisitNote;
        result.Company = value.Company is null ? null : ToEntity(value.Company, seen);
        result.Career = value.Career is null ? null : ToEntity(value.Career, seen);
        result.NationalityNav = value.NationalityNav is null ? null : ToEntity(value.NationalityNav, seen);
        return result;
    }

    public static void CopyToDto(Employee value, EmployeeDto target)
    {
        var mapped = ToDto(value);
        target.IDEmployee = mapped.IDEmployee;
        target.StaffName = mapped.StaffName;
        target.Gender = mapped.Gender;
        target.Birthday = mapped.Birthday;
        target.Nationality = mapped.Nationality;
        target.Passport = mapped.Passport;
        target.Address = mapped.Address;
        target.IDCareer = mapped.IDCareer;
        target.WorkPermit = mapped.WorkPermit;
        target.WorkPermitNumber = mapped.WorkPermitNumber;
        target.VisaNumber = mapped.VisaNumber;
        target.VisaSymbol = mapped.VisaSymbol;
        target.TemporaryStay = mapped.TemporaryStay;
        target.Note = mapped.Note;
        target.SettlementResults = mapped.SettlementResults;
        target.SettlementResultsString = mapped.SettlementResultsString;
        target.IDUser = mapped.IDUser;
        target.IDCompany = mapped.IDCompany;
        target.DateCreated = mapped.DateCreated;
        target.CardCreationDate = mapped.CardCreationDate;
        target.WorkingStatus = mapped.WorkingStatus;
        target.DateOfJoin = mapped.DateOfJoin;
        target.DateOfLeave = mapped.DateOfLeave;
        target.Hidden_flag = mapped.Hidden_flag;
        target.FamilyVisit = mapped.FamilyVisit;
        target.FamilyVisitRelativeName = mapped.FamilyVisitRelativeName;
        target.FamilyVisitRelationship = mapped.FamilyVisitRelationship;
        target.FamilyVisitRelativeIdCard = mapped.FamilyVisitRelativeIdCard;
        target.FamilyVisitStartDate = mapped.FamilyVisitStartDate;
        target.FamilyVisitEndDate = mapped.FamilyVisitEndDate;
        target.FamilyVisitNote = mapped.FamilyVisitNote;
        target.Company = mapped.Company;
        target.Career = mapped.Career;
        target.NationalityNav = mapped.NationalityNav;
    }
}
