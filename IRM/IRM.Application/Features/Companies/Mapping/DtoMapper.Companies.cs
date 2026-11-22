using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

internal static partial class DtoMapper
{
    public static AttachDto ToDto(Attach value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static AttachDto ToDto(Attach value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (AttachDto)existing;
        var result = new AttachDto();
        seen[value] = result;
        result.IDAttach = value.IDAttach;
        result.IDCompany = value.IDCompany;
        result.Type = value.Type;
        result.Name = value.Name;
        result.Folder = value.Folder;
        result.DateCreated = value.DateCreated;
        result.DateModified = value.DateModified;
        result.Delete_flag = value.Delete_flag;
        result.Company = value.Company is null ? null : ToDto(value.Company, seen);
        return result;
    }

    public static Attach ToEntity(AttachDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static Attach ToEntity(AttachDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (Attach)existing;
        var result = new Attach();
        seen[value] = result;
        result.IDAttach = value.IDAttach;
        result.IDCompany = value.IDCompany;
        result.Type = value.Type;
        result.Name = value.Name;
        result.Folder = value.Folder;
        result.DateCreated = value.DateCreated;
        result.DateModified = value.DateModified;
        result.Delete_flag = value.Delete_flag;
        result.Company = value.Company is null ? null : ToEntity(value.Company, seen);
        return result;
    }

    public static void CopyToDto(Attach value, AttachDto target)
    {
        var mapped = ToDto(value);
        target.IDAttach = mapped.IDAttach;
        target.IDCompany = mapped.IDCompany;
        target.Type = mapped.Type;
        target.Name = mapped.Name;
        target.Folder = mapped.Folder;
        target.DateCreated = mapped.DateCreated;
        target.DateModified = mapped.DateModified;
        target.Delete_flag = mapped.Delete_flag;
        target.Company = mapped.Company;
    }

    public static CompanyDto ToDto(Company value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static CompanyDto ToDto(Company value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (CompanyDto)existing;
        var result = new CompanyDto();
        seen[value] = result;
        result.IDCompany = value.IDCompany;
        result.CompanyName = value.CompanyName;
        result.TypeOfBusiniess = value.TypeOfBusiniess;
        result.IDField = value.IDField;
        result.LegalRepresentative = value.LegalRepresentative;
        result.Address = value.Address;
        result.TotalAmount = value.TotalAmount;
        result.AmountOfExemption = value.AmountOfExemption;
        result.QuantityAvailable = value.QuantityAvailable;
        result.QuantityNotYet = value.QuantityNotYet;
        result.NumberOfPersonalities = value.NumberOfPersonalities;
        result.RegistrationProfile = value.RegistrationProfile;
        result.RegistrationProfileIndex = value.RegistrationProfileIndex;
        result.DescriptionOfActivities = value.DescriptionOfActivities;
        result.TrackerID = value.TrackerID;
        result.Note = value.Note;
        result.UpdateDay = value.UpdateDay;
        result.Delete_flag = value.Delete_flag;
        result.Field = value.Field is null ? null : ToDto(value.Field, seen);
        result.Tracker = value.Tracker is null ? null : ToDto(value.Tracker, seen);
        result.Employees = value.Employees.Select(item0 => ToDto(item0, seen)).ToList();
        result.PhoneNumbers = value.PhoneNumbers.Select(item0 => ToDto(item0, seen)).ToList();
        result.Emails = value.Emails.Select(item0 => ToDto(item0, seen)).ToList();
        result.Investments = value.Investments.Select(item0 => ToDto(item0, seen)).ToList();
        result.Attachments = value.Attachments.Select(item0 => ToDto(item0, seen)).ToList();
        return result;
    }

    public static Company ToEntity(CompanyDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static Company ToEntity(CompanyDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (Company)existing;
        var result = new Company();
        seen[value] = result;
        result.IDCompany = value.IDCompany;
        result.CompanyName = value.CompanyName;
        result.TypeOfBusiniess = value.TypeOfBusiniess;
        result.IDField = value.IDField;
        result.LegalRepresentative = value.LegalRepresentative;
        result.Address = value.Address;
        result.TotalAmount = value.TotalAmount;
        result.AmountOfExemption = value.AmountOfExemption;
        result.QuantityAvailable = value.QuantityAvailable;
        result.QuantityNotYet = value.QuantityNotYet;
        result.NumberOfPersonalities = value.NumberOfPersonalities;
        result.RegistrationProfile = value.RegistrationProfile;
        result.RegistrationProfileIndex = value.RegistrationProfileIndex;
        result.DescriptionOfActivities = value.DescriptionOfActivities;
        result.TrackerID = value.TrackerID;
        result.Note = value.Note;
        result.UpdateDay = value.UpdateDay;
        result.Delete_flag = value.Delete_flag;
        result.Field = value.Field is null ? null : ToEntity(value.Field, seen);
        result.Tracker = value.Tracker is null ? null : ToEntity(value.Tracker, seen);
        result.Employees = value.Employees.Select(item0 => ToEntity(item0, seen)).ToList();
        result.PhoneNumbers = value.PhoneNumbers.Select(item0 => ToEntity(item0, seen)).ToList();
        result.Emails = value.Emails.Select(item0 => ToEntity(item0, seen)).ToList();
        result.Investments = value.Investments.Select(item0 => ToEntity(item0, seen)).ToList();
        result.Attachments = value.Attachments.Select(item0 => ToEntity(item0, seen)).ToList();
        return result;
    }

    public static void CopyToDto(Company value, CompanyDto target)
    {
        var mapped = ToDto(value);
        target.IDCompany = mapped.IDCompany;
        target.CompanyName = mapped.CompanyName;
        target.TypeOfBusiniess = mapped.TypeOfBusiniess;
        target.IDField = mapped.IDField;
        target.LegalRepresentative = mapped.LegalRepresentative;
        target.Address = mapped.Address;
        target.TotalAmount = mapped.TotalAmount;
        target.AmountOfExemption = mapped.AmountOfExemption;
        target.QuantityAvailable = mapped.QuantityAvailable;
        target.QuantityNotYet = mapped.QuantityNotYet;
        target.NumberOfPersonalities = mapped.NumberOfPersonalities;
        target.RegistrationProfile = mapped.RegistrationProfile;
        target.RegistrationProfileIndex = mapped.RegistrationProfileIndex;
        target.DescriptionOfActivities = mapped.DescriptionOfActivities;
        target.TrackerID = mapped.TrackerID;
        target.Note = mapped.Note;
        target.UpdateDay = mapped.UpdateDay;
        target.Delete_flag = mapped.Delete_flag;
        target.Field = mapped.Field;
        target.Tracker = mapped.Tracker;
        target.Employees = mapped.Employees;
        target.PhoneNumbers = mapped.PhoneNumbers;
        target.Emails = mapped.Emails;
        target.Investments = mapped.Investments;
        target.Attachments = mapped.Attachments;
    }

    public static CompanyProfileDto ToDto(CompanyProfile value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static CompanyProfileDto ToDto(CompanyProfile value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (CompanyProfileDto)existing;
        var result = new CompanyProfileDto();
        seen[value] = result;
        result.CompanyId = value.CompanyId;
        result.OwnershipTypeCode = value.OwnershipTypeCode;
        result.TaxCode = value.TaxCode;
        result.BusinessRegistrationNumber = value.BusinessRegistrationNumber;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.Company = value.Company is null ? null : ToDto(value.Company, seen);
        return result;
    }

    public static CompanyProfile ToEntity(CompanyProfileDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static CompanyProfile ToEntity(CompanyProfileDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (CompanyProfile)existing;
        var result = new CompanyProfile();
        seen[value] = result;
        result.CompanyId = value.CompanyId;
        result.OwnershipTypeCode = value.OwnershipTypeCode;
        result.TaxCode = value.TaxCode;
        result.BusinessRegistrationNumber = value.BusinessRegistrationNumber;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.Company = value.Company is null ? null : ToEntity(value.Company, seen);
        return result;
    }

    public static void CopyToDto(CompanyProfile value, CompanyProfileDto target)
    {
        var mapped = ToDto(value);
        target.CompanyId = mapped.CompanyId;
        target.OwnershipTypeCode = mapped.OwnershipTypeCode;
        target.TaxCode = mapped.TaxCode;
        target.BusinessRegistrationNumber = mapped.BusinessRegistrationNumber;
        target.ValidFrom = mapped.ValidFrom;
        target.ValidTo = mapped.ValidTo;
        target.Company = mapped.Company;
    }

    public static CompanyProfileAggregateDto ToDto(CompanyProfileAggregate value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static CompanyProfileAggregateDto ToDto(CompanyProfileAggregate value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (CompanyProfileAggregateDto)existing;
        var result = new CompanyProfileAggregateDto();
        seen[value] = result;
        result.Company = value.Company is null ? null : ToDto(value.Company, seen);
        result.Profile = value.Profile is null ? null : ToDto(value.Profile, seen);
        result.Sites = value.Sites.Select(item0 => ToDto(item0, seen)).ToList();
        result.AccommodationAgreements = value.AccommodationAgreements.Select(item0 => ToDto(item0, seen)).ToList();
        result.Representatives = value.Representatives.Select(item0 => ToDto(item0, seen)).ToList();
        result.LegalDocuments = value.LegalDocuments.Select(item0 => ToDto(item0, seen)).ToList();
        result.SponsoredFamilyVisitors = value.SponsoredFamilyVisitors.Select(item0 => ToDto(item0, seen)).ToList();
        return result;
    }

    public static CompanyProfileAggregate ToEntity(CompanyProfileAggregateDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static CompanyProfileAggregate ToEntity(CompanyProfileAggregateDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (CompanyProfileAggregate)existing;
        var result = new CompanyProfileAggregate();
        seen[value] = result;
        result.Company = value.Company is null ? null : ToEntity(value.Company, seen);
        result.Profile = value.Profile is null ? null : ToEntity(value.Profile, seen);
        result.Sites = value.Sites.Select(item0 => ToEntity(item0, seen)).ToList();
        result.AccommodationAgreements = value.AccommodationAgreements.Select(item0 => ToEntity(item0, seen)).ToList();
        result.Representatives = value.Representatives.Select(item0 => ToEntity(item0, seen)).ToList();
        result.LegalDocuments = value.LegalDocuments.Select(item0 => ToEntity(item0, seen)).ToList();
        result.SponsoredFamilyVisitors = value.SponsoredFamilyVisitors.Select(item0 => ToEntity(item0, seen)).ToList();
        return result;
    }

    public static void CopyToDto(CompanyProfileAggregate value, CompanyProfileAggregateDto target)
    {
        var mapped = ToDto(value);
        target.Company = mapped.Company;
        target.Profile = mapped.Profile;
        target.Sites = mapped.Sites;
        target.AccommodationAgreements = mapped.AccommodationAgreements;
        target.Representatives = mapped.Representatives;
        target.LegalDocuments = mapped.LegalDocuments;
        target.SponsoredFamilyVisitors = mapped.SponsoredFamilyVisitors;
    }

    public static CompanyRepresentativeDto ToDto(CompanyRepresentative value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static CompanyRepresentativeDto ToDto(CompanyRepresentative value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (CompanyRepresentativeDto)existing;
        var result = new CompanyRepresentativeDto();
        seen[value] = result;
        result.Id = value.Id;
        result.CompanyId = value.CompanyId;
        result.FullName = value.FullName;
        result.Title = value.Title;
        result.IdNumber = value.IdNumber;
        result.Phone = value.Phone;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.Company = value.Company is null ? null : ToDto(value.Company, seen);
        return result;
    }

    public static CompanyRepresentative ToEntity(CompanyRepresentativeDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static CompanyRepresentative ToEntity(CompanyRepresentativeDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (CompanyRepresentative)existing;
        var result = new CompanyRepresentative();
        seen[value] = result;
        result.Id = value.Id;
        result.CompanyId = value.CompanyId;
        result.FullName = value.FullName;
        result.Title = value.Title;
        result.IdNumber = value.IdNumber;
        result.Phone = value.Phone;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.Company = value.Company is null ? null : ToEntity(value.Company, seen);
        return result;
    }

    public static void CopyToDto(CompanyRepresentative value, CompanyRepresentativeDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.CompanyId = mapped.CompanyId;
        target.FullName = mapped.FullName;
        target.Title = mapped.Title;
        target.IdNumber = mapped.IdNumber;
        target.Phone = mapped.Phone;
        target.ValidFrom = mapped.ValidFrom;
        target.ValidTo = mapped.ValidTo;
        target.Company = mapped.Company;
    }

    public static CompanySiteDto ToDto(CompanySite value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static CompanySiteDto ToDto(CompanySite value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (CompanySiteDto)existing;
        var result = new CompanySiteDto();
        seen[value] = result;
        result.Id = value.Id;
        result.CompanyId = value.CompanyId;
        result.Name = value.Name;
        result.SiteTypeCode = value.SiteTypeCode;
        result.AddressLine = value.AddressLine;
        result.AdministrativeUnitId = value.AdministrativeUnitId;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.IsDeleted = value.IsDeleted;
        result.Company = value.Company is null ? null : ToDto(value.Company, seen);
        result.AdministrativeUnit = value.AdministrativeUnit is null ? null : ToDto(value.AdministrativeUnit, seen);
        result.ZoneMemberships = value.ZoneMemberships.Select(item0 => ToDto(item0, seen)).ToList();
        return result;
    }

    public static CompanySite ToEntity(CompanySiteDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static CompanySite ToEntity(CompanySiteDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (CompanySite)existing;
        var result = new CompanySite();
        seen[value] = result;
        result.Id = value.Id;
        result.CompanyId = value.CompanyId;
        result.Name = value.Name;
        result.SiteTypeCode = value.SiteTypeCode;
        result.AddressLine = value.AddressLine;
        result.AdministrativeUnitId = value.AdministrativeUnitId;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.IsDeleted = value.IsDeleted;
        result.Company = value.Company is null ? null : ToEntity(value.Company, seen);
        result.AdministrativeUnit = value.AdministrativeUnit is null ? null : ToEntity(value.AdministrativeUnit, seen);
        result.ZoneMemberships = value.ZoneMemberships.Select(item0 => ToEntity(item0, seen)).ToList();
        return result;
    }

    public static void CopyToDto(CompanySite value, CompanySiteDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.CompanyId = mapped.CompanyId;
        target.Name = mapped.Name;
        target.SiteTypeCode = mapped.SiteTypeCode;
        target.AddressLine = mapped.AddressLine;
        target.AdministrativeUnitId = mapped.AdministrativeUnitId;
        target.ValidFrom = mapped.ValidFrom;
        target.ValidTo = mapped.ValidTo;
        target.IsDeleted = mapped.IsDeleted;
        target.Company = mapped.Company;
        target.AdministrativeUnit = mapped.AdministrativeUnit;
        target.ZoneMemberships = mapped.ZoneMemberships;
    }

    public static EmailDto ToDto(Email value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static EmailDto ToDto(Email value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (EmailDto)existing;
        var result = new EmailDto();
        seen[value] = result;
        result.IDEmail = value.IDEmail;
        result.Name = value.Name;
        result.Mail = value.Mail;
        result.IDCompany = value.IDCompany;
        result.Delete_flag = value.Delete_flag;
        result.Company = value.Company is null ? null : ToDto(value.Company, seen);
        return result;
    }

    public static Email ToEntity(EmailDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static Email ToEntity(EmailDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (Email)existing;
        var result = new Email();
        seen[value] = result;
        result.IDEmail = value.IDEmail;
        result.Name = value.Name;
        result.Mail = value.Mail;
        result.IDCompany = value.IDCompany;
        result.Delete_flag = value.Delete_flag;
        result.Company = value.Company is null ? null : ToEntity(value.Company, seen);
        return result;
    }

    public static void CopyToDto(Email value, EmailDto target)
    {
        var mapped = ToDto(value);
        target.IDEmail = mapped.IDEmail;
        target.Name = mapped.Name;
        target.Mail = mapped.Mail;
        target.IDCompany = mapped.IDCompany;
        target.Delete_flag = mapped.Delete_flag;
        target.Company = mapped.Company;
    }

    public static InvestmentDto ToDto(Investment value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static InvestmentDto ToDto(Investment value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (InvestmentDto)existing;
        var result = new InvestmentDto();
        seen[value] = result;
        result.IDInvestment = value.IDInvestment;
        result.Name = value.Name;
        result.Nationality = value.Nationality;
        result.AmountOfMoney = value.AmountOfMoney;
        result.IDCompany = value.IDCompany;
        result.Passport = value.Passport;
        result.Company = value.Company is null ? null : ToDto(value.Company, seen);
        return result;
    }

    public static Investment ToEntity(InvestmentDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static Investment ToEntity(InvestmentDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (Investment)existing;
        var result = new Investment();
        seen[value] = result;
        result.IDInvestment = value.IDInvestment;
        result.Name = value.Name;
        result.Nationality = value.Nationality;
        result.AmountOfMoney = value.AmountOfMoney;
        result.IDCompany = value.IDCompany;
        result.Passport = value.Passport;
        result.Company = value.Company is null ? null : ToEntity(value.Company, seen);
        return result;
    }

    public static void CopyToDto(Investment value, InvestmentDto target)
    {
        var mapped = ToDto(value);
        target.IDInvestment = mapped.IDInvestment;
        target.Name = mapped.Name;
        target.Nationality = mapped.Nationality;
        target.AmountOfMoney = mapped.AmountOfMoney;
        target.IDCompany = mapped.IDCompany;
        target.Passport = mapped.Passport;
        target.Company = mapped.Company;
    }

    public static PhoneNumberDto ToDto(PhoneNumber value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static PhoneNumberDto ToDto(PhoneNumber value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (PhoneNumberDto)existing;
        var result = new PhoneNumberDto();
        seen[value] = result;
        result.IDPhoneNumber = value.IDPhoneNumber;
        result.Name = value.Name;
        result.Phone = value.Phone;
        result.IDCompany = value.IDCompany;
        result.Delete_flag = value.Delete_flag;
        result.Company = value.Company is null ? null : ToDto(value.Company, seen);
        return result;
    }

    public static PhoneNumber ToEntity(PhoneNumberDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static PhoneNumber ToEntity(PhoneNumberDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (PhoneNumber)existing;
        var result = new PhoneNumber();
        seen[value] = result;
        result.IDPhoneNumber = value.IDPhoneNumber;
        result.Name = value.Name;
        result.Phone = value.Phone;
        result.IDCompany = value.IDCompany;
        result.Delete_flag = value.Delete_flag;
        result.Company = value.Company is null ? null : ToEntity(value.Company, seen);
        return result;
    }

    public static void CopyToDto(PhoneNumber value, PhoneNumberDto target)
    {
        var mapped = ToDto(value);
        target.IDPhoneNumber = mapped.IDPhoneNumber;
        target.Name = mapped.Name;
        target.Phone = mapped.Phone;
        target.IDCompany = mapped.IDCompany;
        target.Delete_flag = mapped.Delete_flag;
        target.Company = mapped.Company;
    }

    public static SiteZoneMembershipDto ToDto(SiteZoneMembership value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static SiteZoneMembershipDto ToDto(SiteZoneMembership value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (SiteZoneMembershipDto)existing;
        var result = new SiteZoneMembershipDto();
        seen[value] = result;
        result.Id = value.Id;
        result.CompanySiteId = value.CompanySiteId;
        result.EconomicZoneId = value.EconomicZoneId;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.CompanySite = value.CompanySite is null ? null : ToDto(value.CompanySite, seen);
        result.EconomicZone = value.EconomicZone is null ? null : ToDto(value.EconomicZone, seen);
        return result;
    }

    public static SiteZoneMembership ToEntity(SiteZoneMembershipDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static SiteZoneMembership ToEntity(SiteZoneMembershipDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (SiteZoneMembership)existing;
        var result = new SiteZoneMembership();
        seen[value] = result;
        result.Id = value.Id;
        result.CompanySiteId = value.CompanySiteId;
        result.EconomicZoneId = value.EconomicZoneId;
        result.ValidFrom = value.ValidFrom;
        result.ValidTo = value.ValidTo;
        result.CompanySite = value.CompanySite is null ? null : ToEntity(value.CompanySite, seen);
        result.EconomicZone = value.EconomicZone is null ? null : ToEntity(value.EconomicZone, seen);
        return result;
    }

    public static void CopyToDto(SiteZoneMembership value, SiteZoneMembershipDto target)
    {
        var mapped = ToDto(value);
        target.Id = mapped.Id;
        target.CompanySiteId = mapped.CompanySiteId;
        target.EconomicZoneId = mapped.EconomicZoneId;
        target.ValidFrom = mapped.ValidFrom;
        target.ValidTo = mapped.ValidTo;
        target.CompanySite = mapped.CompanySite;
        target.EconomicZone = mapped.EconomicZone;
    }
}
