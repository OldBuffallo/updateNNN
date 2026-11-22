using IRM.Data.Models;

namespace IRM.Application.Features.ImportExport;

/// <summary>Kết quả phân tích 1 dòng Excel: giữ toàn bộ lỗi và cảnh báo thay vì dừng ở lỗi đầu tiên.</summary>
public sealed record ParseRowResult(Employee? Employee, List<string> Errors, List<string> Warnings);
