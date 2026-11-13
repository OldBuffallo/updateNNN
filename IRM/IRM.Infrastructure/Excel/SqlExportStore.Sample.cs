namespace IRM.Infrastructure.Excel;

public partial class SqlExportStore
{
    public async Task<byte[]> GenerateSampleExcelBytesAsync()
    {
        if (_guard is not null) await _guard.GetRequiredAccountIdAsync();
        return ImportSampleWorkbook.Create(DateTime.Now);
    }
}
