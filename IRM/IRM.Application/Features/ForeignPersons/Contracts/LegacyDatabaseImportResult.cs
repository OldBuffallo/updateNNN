namespace IRM.Services;

/// <summary>
/// Kết quả nhập & đồng bộ dữ liệu từ database cũ (ReportManagerDB) sang database IRM.
/// </summary>
public sealed record LegacyDatabaseImportResult(
    int NationalitiesAdded,
    int FieldsAdded,
    int CareersAdded,
    int DistrictsAdded,
    int WardsAdded,
    int CompaniesAdded,
    int EmployeesAdded,
    int ProfilesLinked);
