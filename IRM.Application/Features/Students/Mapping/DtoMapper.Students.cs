using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

internal static partial class DtoMapper
{
    public static StudentDto ToDto(Student value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static StudentDto ToDto(Student value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (StudentDto)existing;
        var result = new StudentDto();
        seen[value] = result;
        result.IDStudent = value.IDStudent;
        result.FullName = value.FullName;
        result.Gender = value.Gender;
        result.Birthday = value.Birthday;
        result.Nationality = value.Nationality;
        result.Passport = value.Passport;
        result.Address = value.Address;
        result.SchoolName = value.SchoolName;
        result.Major = value.Major;
        result.StudentCode = value.StudentCode;
        result.EducationLevel = value.EducationLevel;
        result.EnrollmentDate = value.EnrollmentDate;
        result.ExpectedGraduation = value.ExpectedGraduation;
        result.VisaNumber = value.VisaNumber;
        result.VisaExpiry = value.VisaExpiry;
        result.TemporaryStay = value.TemporaryStay;
        result.ScholarshipType = value.ScholarshipType;
        result.Status = value.Status;
        result.Note = value.Note;
        result.IDUser = value.IDUser;
        result.DateCreated = value.DateCreated;
        result.Hidden_flag = value.Hidden_flag;
        result.NationalityNav = value.NationalityNav is null ? null : ToDto(value.NationalityNav, seen);
        return result;
    }

    public static Student ToEntity(StudentDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static Student ToEntity(StudentDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (Student)existing;
        var result = new Student();
        seen[value] = result;
        result.IDStudent = value.IDStudent;
        result.FullName = value.FullName;
        result.Gender = value.Gender;
        result.Birthday = value.Birthday;
        result.Nationality = value.Nationality;
        result.Passport = value.Passport;
        result.Address = value.Address;
        result.SchoolName = value.SchoolName;
        result.Major = value.Major;
        result.StudentCode = value.StudentCode;
        result.EducationLevel = value.EducationLevel;
        result.EnrollmentDate = value.EnrollmentDate;
        result.ExpectedGraduation = value.ExpectedGraduation;
        result.VisaNumber = value.VisaNumber;
        result.VisaExpiry = value.VisaExpiry;
        result.TemporaryStay = value.TemporaryStay;
        result.ScholarshipType = value.ScholarshipType;
        result.Status = value.Status;
        result.Note = value.Note;
        result.IDUser = value.IDUser;
        result.DateCreated = value.DateCreated;
        result.Hidden_flag = value.Hidden_flag;
        result.NationalityNav = value.NationalityNav is null ? null : ToEntity(value.NationalityNav, seen);
        return result;
    }

    public static void CopyToDto(Student value, StudentDto target)
    {
        var mapped = ToDto(value);
        target.IDStudent = mapped.IDStudent;
        target.FullName = mapped.FullName;
        target.Gender = mapped.Gender;
        target.Birthday = mapped.Birthday;
        target.Nationality = mapped.Nationality;
        target.Passport = mapped.Passport;
        target.Address = mapped.Address;
        target.SchoolName = mapped.SchoolName;
        target.Major = mapped.Major;
        target.StudentCode = mapped.StudentCode;
        target.EducationLevel = mapped.EducationLevel;
        target.EnrollmentDate = mapped.EnrollmentDate;
        target.ExpectedGraduation = mapped.ExpectedGraduation;
        target.VisaNumber = mapped.VisaNumber;
        target.VisaExpiry = mapped.VisaExpiry;
        target.TemporaryStay = mapped.TemporaryStay;
        target.ScholarshipType = mapped.ScholarshipType;
        target.Status = mapped.Status;
        target.Note = mapped.Note;
        target.IDUser = mapped.IDUser;
        target.DateCreated = mapped.DateCreated;
        target.Hidden_flag = mapped.Hidden_flag;
        target.NationalityNav = mapped.NationalityNav;
    }
}
