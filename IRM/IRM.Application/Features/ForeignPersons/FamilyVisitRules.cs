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
}
