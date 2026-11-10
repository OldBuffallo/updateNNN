using IRM.Services;

namespace IRM.Application.Features.ForeignPersons;

public static class FamilyVisitRules
{

    public static void Validate(FamilyVisitInput input)
    {
        if (string.IsNullOrWhiteSpace(input.FullName)) throw new ArgumentException("Họ tên là bắt buộc.");
        if (string.IsNullOrWhiteSpace(input.RelativeName)) throw new ArgumentException("Thông tin thân nhân là bắt buộc.");
        if (input.ValidTo.HasValue && input.ValidTo.Value.Date < input.ValidFrom.Date)
            throw new ArgumentException("Ngày kết thúc không được trước ngày bắt đầu.");
    }

    /// <summary>
    /// Extended validation that also checks the nationality code against the
    /// active catalog.  Call this overload when the caller has already loaded the
    /// set of valid nationality codes from the database.
    /// </summary>
    public static void Validate(FamilyVisitInput input, IReadOnlyCollection<string> validNationalityCodes)
    {
        Validate(input);

        if (!string.IsNullOrWhiteSpace(input.NationalityCode)
            && !validNationalityCodes.Contains(input.NationalityCode))
        {
            throw new ArgumentException(
                $"Quốc tịch \"{input.NationalityCode}\" không có trong danh mục. " +
                "Vui lòng thêm quốc tịch tại Quản trị → Quốc tịch trước khi lưu hồ sơ.");
        }
    }
}
