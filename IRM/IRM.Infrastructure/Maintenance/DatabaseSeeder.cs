using IRM.Data.Models;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Data;

/// <summary>
/// Khởi tạo dữ liệu danh mục và tài khoản quản trị cho database mới.
/// </summary>
public static class DatabaseSeeder
{
    public const string InitialCompanyName = "CÔNG TY MẪU - HÃY ĐỔI TÊN";

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
    /// và đồng bộ tài khoản admin do người cài đặt cung cấp.
    /// </summary>
    public static async Task SeedCatalogsIfEmptyAsync(IrmDbContext db, string? adminUsername = null, string? adminPassword = null)
    {
        var hasAccounts = await db.Accounts.AnyAsync();
        var hasAdminUsername = !string.IsNullOrWhiteSpace(adminUsername);
        var hasAdminPassword = !string.IsNullOrWhiteSpace(adminPassword);
        if (!hasAccounts && (!hasAdminUsername || !hasAdminPassword))
            throw new InvalidOperationException("Database chưa có tài khoản quản trị. Hãy chạy IRM --bootstrap với Bootstrap:AdminUsername và Bootstrap:AdminPassword.");
        if (hasAdminUsername != hasAdminPassword)
            throw new InvalidOperationException("Bootstrap phải cung cấp đủ cả AdminUsername và AdminPassword.");

        // Installer luôn truyền cặp thông tin này. Phải upsert credential ngay cả khi
        // database đã tồn tại; nếu chỉ seed khi Accounts rỗng thì lần cài lại sẽ
        // bỏ qua mật khẩu vừa nhập và self-test chắc chắn đăng nhập thất bại.
        if (hasAdminUsername && hasAdminPassword)
        {
            if (adminPassword!.Length < 12)
                throw new InvalidOperationException("Mật khẩu quản trị phải có ít nhất 12 ký tự.");

            var normalizedUsername = adminUsername!.Trim();
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Account>();
            var admin = await db.Accounts.FirstOrDefaultAsync(account => account.Username == normalizedUsername);
            if (admin is null)
            {
                admin = new Account
                {
                    Username = normalizedUsername,
                    Name = "Quản trị viên",
                    Password = "***",
                    Permission = AccountPermission.Admin,
                    Delete_flag = 0
                };
                db.Accounts.Add(admin);
                await db.SaveChangesAsync();
            }
            else
            {
                admin.Name = string.IsNullOrWhiteSpace(admin.Name) ? "Quản trị viên" : admin.Name;
                admin.Password = "***";
                admin.Permission = AccountPermission.Admin;
                admin.Delete_flag = 0;
            }

            var credential = await db.WebCredentials.SingleOrDefaultAsync(item => item.AccountId == admin.IDUser);
            if (credential is null)
            {
                credential = new WebCredential { AccountId = admin.IDUser };
                db.WebCredentials.Add(credential);
            }
            credential.PasswordHash = hasher.HashPassword(admin, adminPassword);
            credential.MustChangePassword = false;
            credential.FailedLoginCount = 0;
            credential.LockedUntil = null;
            credential.UpdatedAt = DateTime.UtcNow;

            if (!await db.WebRoleAssignments.AnyAsync(role =>
                    role.AccountId == admin.IDUser && role.RoleCode == IrmRoles.Admin))
            {
                db.WebRoleAssignments.Add(new WebRoleAssignment
                {
                    AccountId = admin.IDUser,
                    RoleCode = IrmRoles.Admin
                });
            }
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

        // Một database mới phải sử dụng được ngay. Employee có khóa ngoại bắt buộc
        // tới Company, vì vậy tạo một công ty khởi đầu rõ ràng để người dùng đổi tên
        // thay vì để thao tác import đầu tiên thất bại với IDCompany = 0.
        if (!await db.Companies.AnyAsync(company => company.Delete_flag == 0))
        {
            var trackerId = await db.Accounts
                .Where(account => account.Delete_flag == 0)
                .OrderBy(account => account.IDUser)
                .Select(account => account.IDUser)
                .FirstOrDefaultAsync();
            var fieldId = await db.Fields
                .Where(field => field.Delete_flag == 0)
                .OrderBy(field => field.IDField)
                .Select(field => field.IDField)
                .FirstOrDefaultAsync();

            if (trackerId <= 0 || fieldId <= 0)
                throw new InvalidOperationException("Không thể tạo công ty mẫu vì thiếu tài khoản quản trị hoặc lĩnh vực hoạt động.");

            db.Companies.Add(new Company
            {
                CompanyName = InitialCompanyName,
                TypeOfBusiniess = "Chưa cập nhật",
                IDField = fieldId,
                TrackerID = trackerId,
                Note = "Dữ liệu khởi tạo của hệ thống. Hãy sửa tên và thông tin công ty trước khi sử dụng chính thức.",
                UpdateDay = DateTime.Now,
                Delete_flag = 0
            });
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
                // Đông Á & Đông Nam Á (phổ biến nhất với lao động nước ngoài tại VN)
                new NationalityEntity { NationalityCode = "CN", NationalityName = "Trung Quốc" },
                new NationalityEntity { NationalityCode = "KR", NationalityName = "Hàn Quốc" },
                new NationalityEntity { NationalityCode = "JP", NationalityName = "Nhật Bản" },
                new NationalityEntity { NationalityCode = "TW", NationalityName = "Đài Loan" },
                new NationalityEntity { NationalityCode = "IN", NationalityName = "Ấn Độ" },
                new NationalityEntity { NationalityCode = "MY", NationalityName = "Malaysia" },
                new NationalityEntity { NationalityCode = "PH", NationalityName = "Philippines" },
                new NationalityEntity { NationalityCode = "TH", NationalityName = "Thái Lan" },
                new NationalityEntity { NationalityCode = "ID", NationalityName = "Indonesia" },
                new NationalityEntity { NationalityCode = "SG", NationalityName = "Singapore" },
                new NationalityEntity { NationalityCode = "MM", NationalityName = "Myanmar" },
                new NationalityEntity { NationalityCode = "LA", NationalityName = "Lào" },
                new NationalityEntity { NationalityCode = "KH", NationalityName = "Campuchia" },
                new NationalityEntity { NationalityCode = "BN", NationalityName = "Brunei" },
                // Châu Âu & Bắc Mỹ
                new NationalityEntity { NationalityCode = "US", NationalityName = "Hoa Kỳ" },
                new NationalityEntity { NationalityCode = "GB", NationalityName = "Anh Quốc" },
                new NationalityEntity { NationalityCode = "FR", NationalityName = "Pháp" },
                new NationalityEntity { NationalityCode = "DE", NationalityName = "Đức" },
                new NationalityEntity { NationalityCode = "RU", NationalityName = "Nga" },
                new NationalityEntity { NationalityCode = "IT", NationalityName = "Ý" },
                new NationalityEntity { NationalityCode = "ES", NationalityName = "Tây Ban Nha" },
                new NationalityEntity { NationalityCode = "CA", NationalityName = "Canada" },
                new NationalityEntity { NationalityCode = "NL", NationalityName = "Hà Lan" },
                new NationalityEntity { NationalityCode = "SE", NationalityName = "Thụy Điển" },
                new NationalityEntity { NationalityCode = "DK", NationalityName = "Đan Mạch" },
                new NationalityEntity { NationalityCode = "AU", NationalityName = "Úc" },
                new NationalityEntity { NationalityCode = "NZ", NationalityName = "New Zealand" },
                // Nam Á & Trung Đông
                new NationalityEntity { NationalityCode = "BD", NationalityName = "Bangladesh" },
                new NationalityEntity { NationalityCode = "PK", NationalityName = "Pakistan" },
                new NationalityEntity { NationalityCode = "LK", NationalityName = "Sri Lanka" },
                new NationalityEntity { NationalityCode = "NP", NationalityName = "Nepal" },
                // Châu Phi
                new NationalityEntity { NationalityCode = "NG", NationalityName = "Nigeria" },
                new NationalityEntity { NationalityCode = "ZA", NationalityName = "Nam Phi" },
                // Châu Mỹ Latinh
                new NationalityEntity { NationalityCode = "BR", NationalityName = "Brazil" });
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
