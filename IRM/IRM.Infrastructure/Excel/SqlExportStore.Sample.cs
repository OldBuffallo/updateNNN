using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Infrastructure.Excel;

public partial class SqlExportStore
{
    public async Task<byte[]> GenerateSampleExcelBytesAsync()
    {
        if (_guard is not null) await _guard.GetRequiredAccountIdAsync();
        await using var db = await _dbFactory.CreateDbContextAsync();
        var catalogs = new List<ImportSampleCatalogRow>();
        catalogs.AddRange(await db.Nationality.AsNoTracking().Where(n => n.Delete_flag == 0).OrderBy(n => n.NationalityName)
            .Select(n => new ImportSampleCatalogRow("Quốc tịch", n.NationalityCode, n.NationalityName, "Dùng đúng mã hoặc tên tại hệ thống đích.")).ToListAsync());
        catalogs.AddRange(await db.Careers.AsNoTracking().Where(c => c.Delete_flag == 0).OrderBy(c => c.CareerName)
            .Select(c => new ImportSampleCatalogRow("Nghề nghiệp", c.IDCareer.ToString(), c.CareerName,
                "Nhóm " + c.IDCG + " · " + c.CareerGroup!.CareerGroupName)).ToListAsync());
        catalogs.AddRange(await db.CareerGroups.AsNoTracking().OrderBy(g => g.CareerGroupName)
            .Select(g => new ImportSampleCatalogRow("Nhóm nghề", g.IDCG.ToString(), g.CareerGroupName, "Chọn nhóm tương ứng khi nạp file.")).ToListAsync());
        catalogs.AddRange(await db.Wards.AsNoTracking().Where(w => w.Delete_flag == 0).OrderBy(w => w.WardName)
            .Select(w => new ImportSampleCatalogRow("Phường/xã cho địa chỉ lõi", w.IDWard.ToString(), w.WardName, "Cột Địa chỉ phải kết thúc bằng tên này.")).ToListAsync());
        catalogs.AddRange(await db.AdministrativeUnits.AsNoTracking().Where(u => !u.IsDeleted).OrderBy(u => u.Code)
            .Select(u => new ImportSampleCatalogRow("Mã địa bàn cư trú", u.Code, u.Name, "Cột Mã xã/phường/đặc khu; dùng cùng Địa chỉ lưu trú.")).ToListAsync());
        catalogs.AddRange(await db.Companies.AsNoTracking().Where(c => c.Delete_flag == 0).OrderBy(c => c.CompanyName)
            .Select(c => new ImportSampleCatalogRow("Công ty", c.IDCompany.ToString(), c.CompanyName, "Cột Công ty dùng tên đầy đủ; mã chỉ để tham chiếu.")).ToListAsync());
        catalogs.AddRange(StayPurposeCodes.All.Select(code => new ImportSampleCatalogRow("Diện cư trú", code, code, "Chỉ ghi nhận diện cư trú được cung cấp.")));
        string[] documentTypes = [ImmigrationDocumentTypeCodes.Visa, ImmigrationDocumentTypeCodes.VisaExtension,
            ImmigrationDocumentTypeCodes.TemporaryResidenceCard, ImmigrationDocumentTypeCodes.VisaExemption, ImmigrationDocumentTypeCodes.Other];
        catalogs.AddRange(documentTypes.Select(code => new ImportSampleCatalogRow("Loại giấy tờ", code, code, "Nếu nhiều giấy tờ, phân cách bằng dấu ; theo đúng thứ tự số và hạn.")));
        return ImportSampleWorkbook.Create(catalogs, DateTime.Now);
    }
}
