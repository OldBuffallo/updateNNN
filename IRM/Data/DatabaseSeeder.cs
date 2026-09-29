using IRM.Data.Models;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Data;

/// <summary>
/// Khởi tạo dữ liệu danh mục và tài khoản quản trị cho database mới.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Nạp danh mục đơn vị hành chính cho các database thử nghiệm cũ.
    /// Production chỉ gọi SeedCatalogsIfEmptyAsync và không nạp hồ sơ nghiệp vụ mẫu.
    /// </summary>
    public static async Task SeedV010Async(IrmDbContext db)
    {
        if (!await db.AdministrativeUnits.AnyAsync(x => x.Code == "QN-C-001"))
        {
            var validFrom = new DateTime(2025, 7, 1);
            var communes = new[]
            {
                "Quảng La", "Thống Nhất", "Hải Hòa", "Tiên Yên", "Điền Xá", "Đông Ngũ",
                "Hải Lạng", "Lương Minh", "Kỳ Thượng", "Ba Chẽ", "Quảng Tân", "Đầm Hà",
                "Quảng Hà", "Đường Hoa", "Quảng Đức", "Hoành Mô", "Lục Hồn", "Bình Liêu",
                "Hải Sơn", "Hải Ninh", "Vĩnh Thực", "Cái Chiên"
            };
            var wards = new[]
            {
                "An Sinh", "Đông Triều", "Bình Khê", "Mạo Khê", "Hoàng Quế", "Yên Tử",
                "Vàng Danh", "Uông Bí", "Đông Mai", "Hiệp Hòa", "Quảng Yên", "Hà An",
                "Phong Cốc", "Liên Hòa", "Tuần Châu", "Việt Hưng", "Bãi Cháy", "Hà Tu",
                "Hà Lầm", "Cao Xanh", "Hồng Gai", "Hạ Long", "Hoành Bồ", "Mông Dương",
                "Quang Hanh", "Cẩm Phả", "Cửa Ông", "Móng Cái 1", "Móng Cái 2", "Móng Cái 3"
            };

            db.AdministrativeUnits.AddRange(communes.Select((name, i) => new AdministrativeUnit
            {
                Code = $"QN-C-{i + 1:000}", Name = name,
                TypeCode = AdministrativeUnitTypeCodes.Commune, ValidFrom = validFrom
            }));
            db.AdministrativeUnits.AddRange(wards.Select((name, i) => new AdministrativeUnit
            {
                Code = $"QN-W-{i + 1:000}", Name = name,
                TypeCode = AdministrativeUnitTypeCodes.Ward, ValidFrom = validFrom
            }));
            db.AdministrativeUnits.AddRange(
                new AdministrativeUnit { Code = "QN-S-001", Name = "Vân Đồn", TypeCode = AdministrativeUnitTypeCodes.SpecialZone, ValidFrom = validFrom },
                new AdministrativeUnit { Code = "QN-S-002", Name = "Cô Tô", TypeCode = AdministrativeUnitTypeCodes.SpecialZone, ValidFrom = validFrom });
        }

        if (!await db.SchemaVersions.AnyAsync(x => x.Version == "0.1.0"))
        {
            db.SchemaVersions.Add(new SchemaVersion
            {
                Version = "0.1.0",
                Description = "IRM legacy compatibility schema used by automated tests",
                Checksum = "LOCAL-ENSURECREATED",
                AppliedBy = "DatabaseSeeder"
            });
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Seed danh mục cần thiết cho production SQL Server deployment.
    /// Chỉ seed catalog data (Fields, CareerGroups, Careers, Nationalities, Districts, Wards)
    /// và tài khoản admin do người cài đặt cung cấp nếu database trống.
    /// </summary>
    public static async Task SeedCatalogsIfEmptyAsync(IrmDbContext db, string? adminUsername = null, string? adminPassword = null)
    {
        if (!await db.Accounts.AnyAsync())
        {
            if (string.IsNullOrWhiteSpace(adminUsername) || string.IsNullOrWhiteSpace(adminPassword))
                throw new InvalidOperationException("Database chưa có tài khoản quản trị. Hãy chạy IRM --bootstrap với Bootstrap:AdminUsername và Bootstrap:AdminPassword.");
            if (adminPassword.Length < 12)
                throw new InvalidOperationException("Mật khẩu quản trị ban đầu phải có ít nhất 12 ký tự.");
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Account>();
            var admin = new Account
            {
                Username = adminUsername.Trim(),
                Name = "Quản trị viên",
                Password = "***",
                Permission = AccountPermission.Admin
            };
            db.Accounts.Add(admin);
            await db.SaveChangesAsync();

            db.WebCredentials.Add(new WebCredential
            {
                AccountId = admin.IDUser,
                PasswordHash = hasher.HashPassword(admin, adminPassword),
                UpdatedAt = DateTime.UtcNow
            });
            db.WebRoleAssignments.Add(new WebRoleAssignment
            {
                AccountId = admin.IDUser,
                RoleCode = IrmRoles.Admin
            });
            await db.SaveChangesAsync();
        }

        if (!await db.Fields.AnyAsync())
        {
            db.Fields.AddRange(
                new Field { FieldName = "Sản xuất công nghiệp", Description = "Nhà máy, xưởng sản xuất" },
                new Field { FieldName = "Điện tử", Description = "Linh kiện điện tử, bán dẫn" },
                new Field { FieldName = "Thép", Description = "Luyện thép, gia công kim loại" },
                new Field { FieldName = "Hóa chất", Description = "Hóa chất, vật liệu mới" },
                new Field { FieldName = "Ô tô", Description = "Sản xuất, lắp ráp ô tô" },
                new Field { FieldName = "Bán dẫn", Description = "Chip, bán dẫn, IC" },
                new Field { FieldName = "Dệt may", Description = "Dệt, may mặc" },
                new Field { FieldName = "Thực phẩm", Description = "Chế biến thực phẩm" }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.CareerGroups.AnyAsync())
        {
            db.CareerGroups.AddRange(
                new CareerGroup { CareerGroupName = "Quản lý" },
                new CareerGroup { CareerGroupName = "Kỹ thuật" },
                new CareerGroup { CareerGroupName = "Chuyên gia" },
                new CareerGroup { CareerGroupName = "Lao động phổ thông" }
            );
            await db.SaveChangesAsync();

            var groups = await db.CareerGroups.ToListAsync();
            var ql = groups.First(g => g.CareerGroupName == "Quản lý").IDCG;
            var kt = groups.First(g => g.CareerGroupName == "Kỹ thuật").IDCG;
            var cg = groups.First(g => g.CareerGroupName == "Chuyên gia").IDCG;
            var ld = groups.First(g => g.CareerGroupName == "Lao động phổ thông").IDCG;
            db.Careers.AddRange(
                new Career { CareerName = "Giám đốc", IDCG = ql },
                new Career { CareerName = "Kỹ sư", IDCG = kt },
                new Career { CareerName = "Chuyên gia tư vấn", IDCG = cg },
                new Career { CareerName = "Công nhân", IDCG = ld }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Nationality.AnyAsync())
        {
            db.Nationality.AddRange(
                new NationalityEntity { NationalityCode = "CN", NationalityName = "Trung Quốc" },
                new NationalityEntity { NationalityCode = "KR", NationalityName = "Hàn Quốc" },
                new NationalityEntity { NationalityCode = "JP", NationalityName = "Nhật Bản" },
                new NationalityEntity { NationalityCode = "TW", NationalityName = "Đài Loan" },
                new NationalityEntity { NationalityCode = "IN", NationalityName = "Ấn Độ" },
                new NationalityEntity { NationalityCode = "MY", NationalityName = "Malaysia" },
                new NationalityEntity { NationalityCode = "PH", NationalityName = "Philippines" },
                new NationalityEntity { NationalityCode = "TH", NationalityName = "Thái Lan" },
                new NationalityEntity { NationalityCode = "ID", NationalityName = "Indonesia" },
                new NationalityEntity { NationalityCode = "US", NationalityName = "Hoa Kỳ" },
                new NationalityEntity { NationalityCode = "GB", NationalityName = "Anh Quốc" },
                new NationalityEntity { NationalityCode = "AU", NationalityName = "Úc" },
                new NationalityEntity { NationalityCode = "LA", NationalityName = "Lào" },
                new NationalityEntity { NationalityCode = "KH", NationalityName = "Campuchia" });
            await db.SaveChangesAsync();
        }

        if (!await db.Districts.AnyAsync())
        {
            db.Districts.AddRange(
                new District { DisTrictName = "Thành phố Hạ Long" },
                new District { DisTrictName = "Thành phố Cẩm Phả" },
                new District { DisTrictName = "Thành phố Móng Cái" });
            await db.SaveChangesAsync();
        }

        if (!await db.Wards.AnyAsync())
        {
            db.Wards.AddRange(
                new Ward { WardName = "Phường Hồng Gai" },
                new Ward { WardName = "Phường Bãi Cháy" },
                new Ward { WardName = "Phường Cẩm Trung" });
            await db.SaveChangesAsync();
        }

        if (!await db.AdministrativeUnits.AnyAsync(x => x.Code == "QN-C-001"))
        {
            var validFrom = new DateTime(2025, 7, 1);
            var communes = new[]
            {
                "Quảng La", "Thống Nhất", "Hải Hòa", "Tiên Yên", "Điền Xá", "Đông Ngũ",
                "Hải Lạng", "Lương Minh", "Kỳ Thượng", "Ba Chẽ", "Quảng Tân", "Đầm Hà",
                "Quảng Hà", "Đường Hoa", "Quảng Đức", "Hoành Mô", "Lục Hồn", "Bình Liêu",
                "Hải Sơn", "Hải Ninh", "Vĩnh Thực", "Cái Chiên"
            };
            var wards = new[]
            {
                "An Sinh", "Đông Triều", "Bình Khê", "Mạo Khê", "Hoàng Quế", "Yên Tử",
                "Vàng Danh", "Uông Bí", "Đông Mai", "Hiệp Hòa", "Quảng Yên", "Hà An",
                "Phong Cốc", "Liên Hòa", "Tuần Châu", "Việt Hưng", "Bãi Cháy", "Hà Tu",
                "Hà Lầm", "Cao Xanh", "Hồng Gai", "Hạ Long", "Hoành Bồ", "Mông Dương",
                "Quang Hanh", "Cẩm Phả", "Cửa Ông", "Móng Cái 1", "Móng Cái 2", "Móng Cái 3"
            };
            db.AdministrativeUnits.AddRange(communes.Select((name, i) => new AdministrativeUnit
            {
                Code = $"QN-C-{i + 1:000}", Name = name,
                TypeCode = AdministrativeUnitTypeCodes.Commune, ValidFrom = validFrom
            }));
            db.AdministrativeUnits.AddRange(wards.Select((name, i) => new AdministrativeUnit
            {
                Code = $"QN-W-{i + 1:000}", Name = name,
                TypeCode = AdministrativeUnitTypeCodes.Ward, ValidFrom = validFrom
            }));
            db.AdministrativeUnits.AddRange(
                new AdministrativeUnit { Code = "QN-S-001", Name = "Vân Đồn", TypeCode = AdministrativeUnitTypeCodes.SpecialZone, ValidFrom = validFrom },
                new AdministrativeUnit { Code = "QN-S-002", Name = "Cô Tô", TypeCode = AdministrativeUnitTypeCodes.SpecialZone, ValidFrom = validFrom });
            await db.SaveChangesAsync();
        }

        if (!await db.SchemaVersions.AnyAsync(x => x.Version == "1.0.1"))
        {
            db.SchemaVersions.Add(new SchemaVersion
            {
                Version = "1.0.1",
                Description = "IRM SQL Server-only production schema",
                Checksum = SchemaVersionService.RequiredChecksum,
                AppliedBy = "DatabaseSeeder"
            });
            await db.SaveChangesAsync();
        }
    }
}
