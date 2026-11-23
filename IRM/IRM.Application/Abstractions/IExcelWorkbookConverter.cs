using IRM.Application.Features.ImportExport;
using IRM.Application.Models;
using IRM.Data.Models;

namespace IRM.Application.Abstractions;

public interface IExcelWorkbookConverter
{
    ExcelConvertAnalysis Analyze(
        Stream stream,
        IReadOnlyList<ExcelColumnInfo> headers,
        IReadOnlyList<ExcelFieldSelection> selections,
        IReadOnlyList<NationalityEntityDto> nationalities,
        IReadOnlySet<string> activeAdminUnitCodes,
        ImportReadOptions options,
        int sampleLimit = 5);

    ExcelConvertOutput ConvertToCleanWorkbook(
        Stream stream,
        IReadOnlyList<ExcelFieldSelection> selections,
        IReadOnlyList<NationalityEntityDto> nationalities,
        IReadOnlySet<string> activeAdminUnitCodes,
        ImportReadOptions options);
}
