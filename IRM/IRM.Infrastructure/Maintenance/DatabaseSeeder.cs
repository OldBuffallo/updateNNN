using IRM.Data.Models;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Data;

/// <summary>
/// Khởi tạo dữ liệu danh mục và tài khoản quản trị cho database mới.
/// </summary>
public static class DatabaseSeeder
{
    private static readonly (string Iso2, string Iso3, string Name)[] DefaultNationalities =
    [
        // Đông Á & Đông Nam Á (phổ biến nhất với lao động nước ngoài tại VN)
        ("CN", "CHN", "Trung Quốc"),
        ("KR", "KOR", "Hàn Quốc"),
        ("JP", "JPN", "Nhật Bản"),
        ("TW", "TWN", "Đài Loan"),
        ("IN", "IND", "Ấn Độ"),
        ("MY", "MYS", "Malaysia"),
        ("PH", "PHL", "Philippines"),
        ("TH", "THA", "Thái Lan"),
        ("ID", "IDN", "Indonesia"),
        ("SG", "SGP", "Singapore"),
        ("MM", "MMR", "Myanmar"),
        ("LA", "LAO", "Lào"),
        ("KH", "KHM", "Campuchia"),
        ("BN", "BRN", "Brunei"),
        // Châu Âu & Bắc Mỹ
        ("US", "USA", "Hoa Kỳ"),
        ("GB", "GBR", "Anh Quốc"),
        ("FR", "FRA", "Pháp"),
        ("DE", "DEU", "Đức"),
        ("RU", "RUS", "Nga"),
        ("IT", "ITA", "Ý"),
        ("ES", "ESP", "Tây Ban Nha"),
        ("CA", "CAN", "Canada"),
        ("NL", "NLD", "Hà Lan"),
        ("SE", "SWE", "Thụy Điển"),
        ("DK", "DNK", "Đan Mạch"),
        ("AU", "AUS", "Úc"),
        ("NZ", "NZL", "New Zealand"),
        // Nam Á & Trung Đông
        ("BD", "BGD", "Bangladesh"),
        ("PK", "PAK", "Pakistan"),
        ("LK", "LKA", "Sri Lanka"),
        ("NP", "NPL", "Nepal"),
        // Châu Phi
        ("NG", "NGA", "Nigeria"),
        ("ZA", "ZAF", "Nam Phi"),
        // Châu Mỹ Latinh
        ("BR", "BRA", "Brazil")
    ];

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
                TypeOfBusiniess = "Doanh nghiệp FDI (Dữ liệu mẫu hướng dẫn)",
                IDField = fieldId,
                TrackerID = trackerId,
                LegalRepresentative = "Nguyễn Văn Mẫu (Hướng dẫn)",
                Address = "Địa chỉ trụ sở mẫu (Hãy cập nhật theo đơn vị)",
                TotalAmount = 1,
                QuantityAvailable = 1,
                Note = "🏢 Dữ liệu mẫu hướng dẫn khi cài đặt lần đầu. Bấm biểu tượng Xem (👁️) để xem mẫu Hồ sơ 6 Tab, dùng để thử nghiệm Import Excel, hoặc đổi tên / xóa khi sử dụng chính thức.",
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
                new CareerGroup { CareerGroupName = "Lao động phổ thông" },
                new CareerGroup { CareerGroupName = "Khác" }
            );
            await db.SaveChangesAsync();

            var groups = await db.CareerGroups.ToListAsync();
            var ql = groups.First(g => g.CareerGroupName == "Quản lý").IDCG;
            var kt = groups.First(g => g.CareerGroupName == "Kỹ thuật").IDCG;
            var cg = groups.First(g => g.CareerGroupName == "Chuyên gia").IDCG;
            var ld = groups.First(g => g.CareerGroupName == "Lao động phổ thông").IDCG;
            var kh = groups.First(g => g.CareerGroupName == "Khác").IDCG;
            db.Careers.AddRange(
                new Career { CareerName = "Giám đốc", IDCG = ql },
                new Career { CareerName = "Kỹ sư", IDCG = kt },
                new Career { CareerName = "Chuyên gia tư vấn", IDCG = cg },
                new Career { CareerName = "Công nhân", IDCG = ld },
                new Career { CareerName = "Thân nhân", IDCG = kh }
            );
            await db.SaveChangesAsync();
        }
        else
        {
            await EnsureRelativeCareerGroupAsync(db);
        }

        if (!await db.Nationality.AnyAsync())
        {
            db.Nationality.AddRange(DefaultNationalities.Select(item =>
                new NationalityEntity { NationalityCode = item.Iso3, NationalityName = item.Name }));
            await db.SaveChangesAsync();
        }
        else
        {
            await UpgradeLegacySeededNationalitiesAsync(db);
        }

        if (!await db.Districts.AnyAsync())
        {
            db.Districts.Add(new District { DisTrictName = "Quận/Huyện mẫu (Admin tự cấu hình)" });
            await db.SaveChangesAsync();
        }

        if (!await db.Wards.AnyAsync())
        {
            db.Wards.Add(new Ward { WardName = "Phường/Xã mẫu (Admin tự cấu hình)" });
            await db.SaveChangesAsync();
        }

        if (!await db.AdministrativeUnits.AnyAsync())
        {
            db.AdministrativeUnits.Add(new AdministrativeUnit
            {
                Code = "DB-001",
                Name = "Địa bàn mẫu (Admin tự cấu hình)",
                TypeCode = AdministrativeUnitTypeCodes.Ward,
                ValidFrom = DateTime.Today
            });
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

    /// <summary>
    /// Tạo bộ dữ liệu mẫu hướng dẫn sử dụng khi cài đặt lần đầu (1 Doanh nghiệp mẫu đủ 6 Tab,
    /// 1 Cơ sở lưu trú mẫu, và 1 Hồ sơ Người nước ngoài mẫu đủ 5 Tab).
    /// Chỉ chạy khi trong hệ thống chưa từng có bất kỳ bản ghi Nhân viên, Du học sinh hay Hồ sơ NNN nào.
    /// </summary>
    public static async Task SeedOnboardingGuideIfEmptyAsync(IrmDbContext db)
    {
        if (await db.Employees.AnyAsync() || await db.Students.AnyAsync() || await db.ForeignPersons.AnyAsync())
            return;

        var company = await db.Companies
            .Where(c => c.Delete_flag == 0)
            .OrderBy(c => c.IDCompany)
            .FirstOrDefaultAsync();
        if (company is null)
            return;

        var adminUnit = await db.AdministrativeUnits
            .Where(u => !u.ValidTo.HasValue)
            .OrderBy(u => u.Id)
            .FirstOrDefaultAsync();
        var careerId = await db.Careers
            .Where(c => c.Delete_flag == 0)
            .OrderBy(c => c.IDCareer)
            .Select(c => (int?)c.IDCareer)
            .FirstOrDefaultAsync();
        var trackerId = await db.Accounts
            .Where(a => a.Delete_flag == 0)
            .OrderBy(a => a.IDUser)
            .Select(a => a.IDUser)
            .FirstOrDefaultAsync();
        var nationalityCode = await db.Nationality
            .Where(n => n.Delete_flag == 0 && (n.NationalityCode == "KOR" || n.NationalityCode == "USA"))
            .Select(n => n.NationalityCode)
            .FirstOrDefaultAsync()
            ?? await db.Nationality.Where(n => n.Delete_flag == 0).Select(n => n.NationalityCode).FirstOrDefaultAsync();

        // 1. Hồ sơ mở rộng 6 Tab cho Công ty mẫu
        if (!await db.CompanyProfiles.AnyAsync(p => p.CompanyId == company.IDCompany))
        {
            db.CompanyProfiles.Add(new CompanyProfile
            {
                CompanyId = company.IDCompany,
                OwnershipTypeCode = "FDI",
                TaxCode = "5700000001-MAU",
                BusinessRegistrationNumber = "ĐKKD-MAU-001",
                ValidFrom = DateTime.Today
            });
        }

        if (adminUnit is not null && !await db.CompanySites.AnyAsync(s => s.CompanyId == company.IDCompany))
        {
            db.CompanySites.Add(new CompanySite
            {
                CompanyId = company.IDCompany,
                Name = "Địa điểm hoạt động mẫu (Dữ liệu hướng dẫn)",
                SiteTypeCode = "FACTORY",
                AddressLine = "Địa chỉ cơ sở mẫu (Hãy cập nhật)",
                AdministrativeUnitId = adminUnit.Id,
                ValidFrom = DateTime.Today
            });
        }

        if (!await db.CompanyRepresentatives.AnyAsync(r => r.CompanyId == company.IDCompany))
        {
            db.CompanyRepresentatives.Add(new CompanyRepresentative
            {
                CompanyId = company.IDCompany,
                FullName = "Nguyễn Văn Mẫu (Đại diện pháp luật mẫu)",
                Title = "Tổng Giám đốc",
                IdNumber = "022000000001",
                Phone = "0900000001",
                ValidFrom = DateTime.Today
            });
        }

        // 2. Cơ sở lưu trú mẫu hướng dẫn
        Accommodation? sampleAcc = await db.Accommodations.FirstOrDefaultAsync(a => !a.IsDeleted);
        if (sampleAcc is null)
        {
            sampleAcc = new Accommodation
            {
                Name = "CƠ SỞ LƯU TRÚ MẪU - HƯỚNG DẪN SỬ DỤNG",
                TypeCode = "RENTED",
                AddressLine = "Địa chỉ cơ sở lưu trú mẫu (Dữ liệu hướng dẫn)",
                AdministrativeUnitId = adminUnit?.Id,
                ContactName = "Trần Văn Mẫu (Quản lý CSLT)",
                ContactPhone = "0900000002",
                Capacity = 50,
                Note = "🏠 Cơ sở lưu trú mẫu được tạo tự động khi cài đặt lần đầu để hướng dẫn sử dụng. Bạn có thể sửa hoặc xóa.",
                IsDeleted = false
            };
            db.Accommodations.Add(sampleAcc);
            await db.SaveChangesAsync();

            db.CompanyAccommodationAgreements.Add(new CompanyAccommodationAgreement
            {
                CompanyId = company.IDCompany,
                AccommodationId = sampleAcc.Id,
                ScopeCode = "PARTIAL",
                UnitLabel = "Tầng 2 - Khu chuyên gia mẫu",
                ValidFrom = DateTime.Today,
                ValidTo = DateTime.Today.AddYears(1)
            });
        }

        // 3. Hồ sơ Người nước ngoài mẫu hướng dẫn (Đầy đủ thông tin 5 Tab)
        if (!string.IsNullOrWhiteSpace(nationalityCode))
        {
            var sampleEmp = new Employee
            {
                StaffName = "KIM MIN-SU (HỒ SƠ MẪU HƯỚNG DẪN - HÃY SỬA HOẶC XÓA)",
                Gender = Gender.Male,
                Birthday = new DateTime(1988, 6, 15),
                Nationality = nationalityCode,
                Passport = "M00000001",
                Address = "Địa chỉ tạm trú mẫu (Hãy cập nhật)",
                IDCompany = company.IDCompany,
                IDCareer = careerId,
                WorkPermit = WorkPermitType.WorkerHasPermit,
                WorkPermitNumber = "GPLĐ-MAU-001",
                VisaNumber = "VISA-MAU-001",
                VisaSymbol = "LĐ2",
                TemporaryStay = DateTime.Today.AddMonths(6),
                IDUser = trackerId > 0 ? trackerId : 1,
                DateCreated = DateTime.Now,
                WorkingStatus = 0,
                Hidden_flag = 0,
                Note = "🪪 Hồ sơ mẫu hướng dẫn khi cài đặt lần đầu. Bấm nút Sửa (✏️) để xem mẫu nhập đầy đủ 5 Tab hoặc bấm Xóa (🗑️) khi bắt đầu sử dụng."
            };
            db.Employees.Add(sampleEmp);
            await db.SaveChangesAsync();

            var samplePerson = new ForeignPerson
            {
                FullName = sampleEmp.StaffName,
                Gender = (int)sampleEmp.Gender,
                Birthday = sampleEmp.Birthday,
                PassportNumber = sampleEmp.Passport,
                PassportSearchKey = "M00000001",
                NationalityCode = nationalityCode,
                IsDataIncomplete = false,
                IsDeleted = false
            };
            db.ForeignPersons.Add(samplePerson);
            await db.SaveChangesAsync();

            db.ForeignPersonSourceLinks.Add(new ForeignPersonSourceLink
            {
                ForeignPersonId = samplePerson.Id,
                SourceType = "EMPLOYEE",
                SourceId = sampleEmp.IDEmployee.ToString()
            });

            var stayCase = new StayCase
            {
                ForeignPersonId = samplePerson.Id,
                PurposeCode = StayPurposeCodes.Work,
                SponsorCompanyId = company.IDCompany,
                ValidFrom = DateTime.Today,
                ValidTo = sampleEmp.TemporaryStay,
                StatusCode = "ACTIVE",
                Note = sampleEmp.Note
            };
            db.StayCases.Add(stayCase);

            if (adminUnit is not null)
            {
                db.ResidencePeriods.Add(new ResidencePeriod
                {
                    ForeignPersonId = samplePerson.Id,
                    AccommodationId = sampleAcc.Id,
                    AdministrativeUnitId = adminUnit.Id,
                    AddressLine = sampleEmp.Address ?? "",
                    ValidFrom = DateTime.Today,
                    ValidTo = sampleEmp.TemporaryStay
                });
            }

            db.ImmigrationDocuments.Add(new ImmigrationDocument
            {
                ForeignPersonId = samplePerson.Id,
                TypeCode = ImmigrationDocumentTypeCodes.Visa,
                Number = "VISA-MAU-001",
                ValidFrom = DateTime.Today,
                ValidTo = sampleEmp.TemporaryStay,
                StatusCode = "ACTIVE"
            });

            db.ElectronicIdentities.Add(new ElectronicIdentity
            {
                ForeignPersonId = samplePerson.Id,
                IdentityNumber = "EID-MAU-0001",
                IdentitySearchKey = "EID-MAU-0001",
                ValidFrom = DateTime.Today,
                ValidTo = DateTime.Today.AddYears(2)
            });

            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureRelativeCareerGroupAsync(IrmDbContext db)
    {
        var groups = await db.CareerGroups.ToListAsync();
        var otherGroup = groups.FirstOrDefault(g =>
            IRM.Application.Features.ImportExport.LegacyImportRules.RemoveDiacritics(g.CareerGroupName)
                .Equals("KHAC", StringComparison.OrdinalIgnoreCase));
        if (otherGroup is null)
        {
            otherGroup = new CareerGroup { CareerGroupName = "Khác" };
            db.CareerGroups.Add(otherGroup);
            await db.SaveChangesAsync();
        }

        var hasRelative = await db.Careers.AnyAsync(c =>
            c.Delete_flag == 0 && c.IDCG == otherGroup.IDCG && c.CareerName == "Thân nhân");
        if (!hasRelative)
        {
            db.Careers.Add(new Career { CareerName = "Thân nhân", IDCG = otherGroup.IDCG });
            await db.SaveChangesAsync();
        }
    }

    private static async Task UpgradeLegacySeededNationalitiesAsync(IrmDbContext db)
    {
        var existing = await db.Nationality.ToListAsync();
        var existingByCode = existing.ToDictionary(n => n.NationalityCode.Trim(), StringComparer.OrdinalIgnoreCase);

        foreach (var (iso2, iso3, name) in DefaultNationalities)
        {
            if (!existingByCode.TryGetValue(iso2, out var legacyRow))
                continue;

            if (!existingByCode.ContainsKey(iso3))
            {
                var upgraded = new NationalityEntity
                {
                    NationalityCode = iso3,
                    NationalityName = string.IsNullOrWhiteSpace(legacyRow.NationalityName) ? name : legacyRow.NationalityName,
                    Delete_flag = legacyRow.Delete_flag
                };
                db.Nationality.Add(upgraded);
                await db.SaveChangesAsync();
                existingByCode[iso3] = upgraded;
            }

            foreach (var item in await db.Employees.Where(e => e.Nationality == iso2).ToListAsync())
                item.Nationality = iso3;
            foreach (var item in await db.Students.Where(s => s.Nationality == iso2).ToListAsync())
                item.Nationality = iso3;
            foreach (var item in await db.ForeignPersons.Where(p => p.NationalityCode == iso2).ToListAsync())
                item.NationalityCode = iso3;
            foreach (var item in await db.Investments.Where(i => i.Nationality == iso2).ToListAsync())
                item.Nationality = iso3;
            foreach (var item in await db.ArchivedEmployees.Where(a => a.Nationality == iso2).ToListAsync())
                item.Nationality = iso3;
            foreach (var item in await db.FamilyVisitDetails.Where(f => f.RelativeNationalityCode == iso2).ToListAsync())
                item.RelativeNationalityCode = iso3;
            await db.SaveChangesAsync();

            db.Nationality.Remove(legacyRow);
            await db.SaveChangesAsync();
            existingByCode.Remove(iso2);
        }
    }
}
