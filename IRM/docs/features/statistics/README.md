# Thống kê Địa bàn (Statistics)

## 1. Tổng quan

Module thống kê theo địa bàn hành chính: bao nhiêu người nước ngoài ở từng xã/phường, phân theo diện cư trú (lao động/thăm thân/du học/du lịch), doanh nghiệp, CSLT, KCN/CCN, giấy tờ/định danh. Hỗ trợ as-of date (xem dữ liệu tại thời điểm bất kỳ).

**Route:** `/statistics`

## 2. Kiến trúc kỹ thuật

### Files liên quan

| Layer | File | Vai trò |
|---|---|---|
| Page | `Components/Pages/Statistics.razor` | UI thống kê (bảng + biểu đồ) |
| Service | `Services/StatisticsService.cs` | Query thống kê phức tạp |
| Service | `Services/StatisticsExportService.cs` | Export thống kê ra Excel |
| DTO | `Services/V010Contracts.cs` | TemporalReportFilter, TerritoryStatistics, TerritoryOverview, ZoneStatistics, DocumentIdentityStatistics |

### DI Registration

```csharp
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IStatisticsExportService, StatisticsExportService>();
```

## 3. Service Interface

### `IStatisticsService`

| Method | Return | Mô tả |
|---|---|---|
| `GetTerritoryAsync(filter)` | `Task<TerritoryStatistics>` | Thống kê chi tiết 1 địa bàn |
| `GetTerritoryOverviewAsync(filter)` | `Task<IReadOnlyList<TerritoryOverview>>` | Tổng quan tất cả địa bàn |
| `GetZoneStatisticsAsync(filter)` | `Task<IReadOnlyList<ZoneStatistics>>` | Thống kê theo KCN/CCN |
| `GetDocumentIdentityStatisticsAsync(filter)` | `Task<DocumentIdentityStatistics>` | Thống kê giấy tờ + định danh |

### `TemporalReportFilter`

| Property | Type | Mô tả |
|---|---|---|
| `AsOfDate` | `DateTime?` | Xem dữ liệu tại ngày cụ thể |
| `From/To` | `DateTime?` | Khoảng thời gian (không dùng chung AsOfDate) |
| `AdministrativeUnitId` | `int?` | Lọc theo địa bàn |
| `StayPurposeCode` | `string?` | Lọc theo diện cư trú |
| `EconomicZoneId` | `int?` | Lọc theo KCN |
| `HasElectronicIdentity` | `bool?` | Lọc theo có/không định danh |

### Output DTOs

**TerritoryStatistics** (1 địa bàn):
- `TotalForeigners`, `PeopleByPurpose` (dict), `CompanyCount`, `AccommodationCount`
- `Companies` (list CompanySummary: Id, Name, OwnershipType, WorkerCount)
- `Accommodations` (list AccommodationSummary: Id, Name, Type, ResidentCount, Companies)

**TerritoryOverview** (tất cả địa bàn):
- Mỗi AdministrativeUnit: TotalForeigners + PeopleByPurpose

**ZoneStatistics**: Mỗi EconomicZone: CompanyCount + ForeignPersonCount

**DocumentIdentityStatistics**: DocumentsByType (dict), WithElectronicIdentity, WithoutElectronicIdentity

## 4. Ràng buộc Nghiệp vụ

- **Temporal queries:** Tất cả query sử dụng `as-of date` để filter StayCases, ResidencePeriods, CompanySites, Agreements theo `ValidFrom <= at && (!ValidTo || ValidTo >= at)`
- **Distinct counting:** Đếm distinct ForeignPersonId (1 người có nhiều StayCase vẫn chỉ đếm 1)
- **Primary cases only:** Chỉ đếm StayCase có `IsPrimary = true` và `StatusCode = "ACTIVE"`
- **Validation:** AsOfDate và From/To không được dùng đồng thời; From <= To
- **Authorization:** Tất cả roles đều xem được thống kê

## 5. Hướng dẫn Test

- GetTerritoryOverview: seed dữ liệu nhiều địa bàn → verify tổng đúng
- GetTerritory cho 1 địa bàn: verify breakdown by purpose, company, accommodation
- As-of date: seed residence ending yesterday → as-of today → không đếm
- Zone statistics: seed company trong KCN → verify count

## 6. Hướng dẫn Mở rộng

- **Thêm dimension thống kê:** Thêm method vào `IStatisticsService`, query group-by tương ứng
- **Export:** Tạo Excel sheet mới trong `StatisticsExportService`
- **Chart types:** Thêm MudChart components trong Statistics.razor
