using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Data;

/// <summary>
/// Seed dữ liệu mẫu vào database khi khởi động lần đầu
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IrmDbContext db)
    {
        // ── Accounts ──
        if (!await db.Accounts.AnyAsync())
        {
            db.Accounts.AddRange(
                new Account { Username = "admin", Name = "Quản trị viên", Password = "123456", Permission = 1 },
                new Account { Username = "nguyenvana", Name = "Nguyễn Văn A", Password = "123456", Permission = 0 },
                new Account { Username = "tranthib", Name = "Trần Thị B", Password = "123456", Permission = 0 }
            );
            await db.SaveChangesAsync();
        }

        // ── Fields ──
        if (!await db.Fields.AnyAsync())
        {
            db.Fields.AddRange(
                new Field { FieldName = "Sản xuất công nghiệp", Description = "Nhà máy, xưởng sản xuất" },
                new Field { FieldName = "Công nghệ thông tin", Description = "Phần mềm, phần cứng, dịch vụ IT" },
                new Field { FieldName = "Xây dựng", Description = "Thi công, xây dựng dân dụng" },
                new Field { FieldName = "Thương mại - Dịch vụ", Description = "Bán hàng, nhà hàng, khách sạn" },
                new Field { FieldName = "Giáo dục - Đào tạo", Description = "Trường học, trung tâm ngoại ngữ" }
            );
            await db.SaveChangesAsync();
        }

        // ── CareerGroups ──
        if (!await db.CareerGroups.AnyAsync())
        {
            db.CareerGroups.AddRange(
                new CareerGroup { CareerGroupName = "Quản lý" },
                new CareerGroup { CareerGroupName = "Kỹ thuật" },
                new CareerGroup { CareerGroupName = "Chuyên gia" },
                new CareerGroup { CareerGroupName = "Lao động phổ thông" }
            );
            await db.SaveChangesAsync();
        }

        // ── Careers ──
        if (!await db.Careers.AnyAsync())
        {
            var groups = await db.CareerGroups.ToListAsync();
            var ql = groups.First(g => g.CareerGroupName == "Quản lý").IDCG;
            var kt = groups.First(g => g.CareerGroupName == "Kỹ thuật").IDCG;
            var cg = groups.First(g => g.CareerGroupName == "Chuyên gia").IDCG;
            var ld = groups.First(g => g.CareerGroupName == "Lao động phổ thông").IDCG;

            db.Careers.AddRange(
                new Career { CareerName = "Giám đốc", IDCG = ql },
                new Career { CareerName = "Phó Giám đốc", IDCG = ql },
                new Career { CareerName = "Quản lý", IDCG = ql },
                new Career { CareerName = "Kỹ sư", IDCG = kt },
                new Career { CareerName = "Kỹ thuật viên", IDCG = kt },
                new Career { CareerName = "Chuyên gia", IDCG = cg },
                new Career { CareerName = "Phiên dịch", IDCG = cg },
                new Career { CareerName = "Lao động", IDCG = ld },
                new Career { CareerName = "Trưởng phòng", IDCG = ql },
                new Career { CareerName = "Tư vấn", IDCG = cg }
            );
            await db.SaveChangesAsync();
        }

        // ── Nationality ──
        if (!await db.Nationality.AnyAsync())
        {
            db.Nationality.AddRange(
                new NationalityEntity { NationalityCode = "CN", NationalityName = "Trung Quốc" },
                new NationalityEntity { NationalityCode = "KR", NationalityName = "Hàn Quốc" },
                new NationalityEntity { NationalityCode = "JP", NationalityName = "Nhật Bản" },
                new NationalityEntity { NationalityCode = "TW", NationalityName = "Đài Loan" },
                new NationalityEntity { NationalityCode = "US", NationalityName = "Hoa Kỳ" },
                new NationalityEntity { NationalityCode = "GB", NationalityName = "Anh Quốc" },
                new NationalityEntity { NationalityCode = "FR", NationalityName = "Pháp" },
                new NationalityEntity { NationalityCode = "DE", NationalityName = "Đức" },
                new NationalityEntity { NationalityCode = "TH", NationalityName = "Thái Lan" },
                new NationalityEntity { NationalityCode = "MY", NationalityName = "Malaysia" },
                new NationalityEntity { NationalityCode = "SG", NationalityName = "Singapore" },
                new NationalityEntity { NationalityCode = "IN", NationalityName = "Ấn Độ" },
                new NationalityEntity { NationalityCode = "PH", NationalityName = "Philippines" },
                new NationalityEntity { NationalityCode = "ID", NationalityName = "Indonesia" },
                new NationalityEntity { NationalityCode = "AU", NationalityName = "Úc" }
            );
            await db.SaveChangesAsync();
        }

        // ── Companies ──
        if (!await db.Companies.AnyAsync())
        {
            var firstField = await db.Fields.FirstAsync();
            var firstUser = await db.Accounts.FirstAsync();

            db.Companies.AddRange(
                new Company { CompanyName = "Công ty TNHH Samsung Electronics Việt Nam", TypeOfBusiniess = "100% vốn nước ngoài", IDField = firstField.IDField, LegalRepresentative = "Choi Joo Ho", Address = "KCN Yên Phong, Bắc Ninh", TotalAmount = 50, TrackerID = firstUser.IDUser },
                new Company { CompanyName = "Công ty CP Hyundai Aluminum Vina", TypeOfBusiniess = "Liên doanh", IDField = firstField.IDField, LegalRepresentative = "Park Sung Jin", Address = "KCN Đại An, Hải Dương", TotalAmount = 30, TrackerID = firstUser.IDUser },
                new Company { CompanyName = "Công ty TNHH Canon Việt Nam", TypeOfBusiniess = "100% vốn nước ngoài", IDField = firstField.IDField, LegalRepresentative = "Tanaka Hiroshi", Address = "KCN Quế Võ, Bắc Ninh", TotalAmount = 25, TrackerID = firstUser.IDUser },
                new Company { CompanyName = "Công ty TNHH LG Display Việt Nam", TypeOfBusiniess = "100% vốn nước ngoài", IDField = firstField.IDField, LegalRepresentative = "Lee Dong Hoon", Address = "KCN Tràng Duệ, Hải Phòng", TotalAmount = 40, TrackerID = firstUser.IDUser },
                new Company { CompanyName = "Công ty TNHH Foxconn Bình Dương", TypeOfBusiniess = "100% vốn nước ngoài", IDField = firstField.IDField, LegalRepresentative = "Chen Wei", Address = "KCN VSIP II, Bình Dương", TotalAmount = 35, TrackerID = firstUser.IDUser }
            );
            await db.SaveChangesAsync();
        }

        // ── Employees mẫu (để test trùng lặp) ──
        if (!await db.Employees.AnyAsync())
        {
            var samsung = await db.Companies.FirstAsync(c => c.CompanyName.Contains("Samsung"));
            var hyundai = await db.Companies.FirstAsync(c => c.CompanyName.Contains("Hyundai"));
            var kysu = await db.Careers.FirstAsync(c => c.CareerName == "Kỹ sư");
            var quanly = await db.Careers.FirstAsync(c => c.CareerName == "Quản lý");

            db.Employees.AddRange(
                new Employee
                {
                    StaffName = "Zhang Wei", Gender = 1, Birthday = new DateTime(1985, 3, 15),
                    Nationality = "CN", Passport = "E12345678", Address = "Bắc Kinh, Trung Quốc",
                    IDCareer = kysu.IDCareer, WorkPermit = 1, TemporaryStay = new DateTime(2027, 6, 30),
                    IDUser = 1, IDCompany = samsung.IDCompany, DateCreated = DateTime.Now
                },
                new Employee
                {
                    StaffName = "Li Ming", Gender = 1, Birthday = new DateTime(1990, 7, 22),
                    Nationality = "CN", Passport = "E87654321", Address = "Thượng Hải, Trung Quốc",
                    IDCareer = kysu.IDCareer, WorkPermit = 1, TemporaryStay = new DateTime(2027, 3, 15),
                    IDUser = 1, IDCompany = samsung.IDCompany, DateCreated = DateTime.Now
                },
                new Employee
                {
                    StaffName = "Park Ji-sung", Gender = 1, Birthday = new DateTime(1988, 11, 5),
                    Nationality = "KR", Passport = "M11223344", Address = "Seoul, Hàn Quốc",
                    IDCareer = quanly.IDCareer, WorkPermit = 1, TemporaryStay = new DateTime(2026, 12, 31),
                    IDUser = 1, IDCompany = hyundai.IDCompany, DateCreated = DateTime.Now
                }
            );
            await db.SaveChangesAsync();
        }

        // ── Districts ──
        if (!await db.Districts.AnyAsync())
        {
            db.Districts.AddRange(
                new District { DisTrictName = "Thành phố Bắc Ninh" },
                new District { DisTrictName = "Huyện Yên Phong" },
                new District { DisTrictName = "Huyện Quế Võ" }
            );
            await db.SaveChangesAsync();
        }

        // ── Wards ──
        if (!await db.Wards.AnyAsync())
        {
            db.Wards.AddRange(
                new Ward { WardName = "Phường Vũ Ninh" },
                new Ward { WardName = "Phường Suối Hoa" }
            );
            await db.SaveChangesAsync();
        }
    }
}
