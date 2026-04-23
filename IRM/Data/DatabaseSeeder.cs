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
                new Account { Username = "tranthib", Name = "Trần Thị B", Password = "123456", Permission = 0 },
                new Account { Username = "levanc", Name = "Lê Văn C", Password = "123456", Permission = 0 }
            );
            await db.SaveChangesAsync();
        }

        // ── Fields ──
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
                new NationalityEntity { NationalityCode = "IN", NationalityName = "Ấn Độ" },
                new NationalityEntity { NationalityCode = "MY", NationalityName = "Malaysia" },
                new NationalityEntity { NationalityCode = "PH", NationalityName = "Philippines" },
                new NationalityEntity { NationalityCode = "TH", NationalityName = "Thái Lan" },
                new NationalityEntity { NationalityCode = "ID", NationalityName = "Indonesia" },
                new NationalityEntity { NationalityCode = "US", NationalityName = "Hoa Kỳ" },
                new NationalityEntity { NationalityCode = "GB", NationalityName = "Anh Quốc" },
                new NationalityEntity { NationalityCode = "AU", NationalityName = "Úc" }
            );
            await db.SaveChangesAsync();
        }

        // ── Companies (15 công ty) ──
        if (!await db.Companies.AnyAsync())
        {
            var fields = await db.Fields.ToListAsync();
            var users = await db.Accounts.ToListAsync();
            var f1 = fields[0].IDField; // Sản xuất
            var f2 = fields[1].IDField; // Điện tử
            var f3 = fields[2].IDField; // Thép
            var f4 = fields[3].IDField; // Hóa chất
            var f5 = fields[4].IDField; // Ô tô
            var f6 = fields[5].IDField; // Bán dẫn
            var u1 = users[0].IDUser;
            var u2 = users[1].IDUser;
            var u3 = users[2].IDUser;

            db.Companies.AddRange(
                new Company { CompanyName = "Cty TNHH Samsung Electronics VN", TypeOfBusiniess = "TNHH", IDField = f1, LegalRepresentative = "Choi Joo Ho", Address = "KCN Yên Phong, Bắc Ninh", TotalAmount = 125, QuantityAvailable = 98, QuantityNotYet = 15, AmountOfExemption = 12, TrackerID = u2 },
                new Company { CompanyName = "Cty CP Hyundai Aluminum Vina", TypeOfBusiniess = "Cổ phần", IDField = f1, LegalRepresentative = "Park Sung Jin", Address = "KCN Đại An, Hải Dương", TotalAmount = 89, QuantityAvailable = 72, QuantityNotYet = 10, AmountOfExemption = 7, TrackerID = u3 },
                new Company { CompanyName = "Cty TNHH Canon Vietnam", TypeOfBusiniess = "TNHH", IDField = f1, LegalRepresentative = "Tanaka Hiroshi", Address = "KCN Quế Võ, Bắc Ninh", TotalAmount = 67, QuantityAvailable = 55, QuantityNotYet = 5, AmountOfExemption = 7, TrackerID = u2 },
                new Company { CompanyName = "Cty TNHH LG Display Vietnam", TypeOfBusiniess = "TNHH", IDField = f2, LegalRepresentative = "Lee Dong Hoon", Address = "KCN Tràng Duệ, Hải Phòng", TotalAmount = 56, QuantityAvailable = 48, QuantityNotYet = 3, AmountOfExemption = 5, TrackerID = u3 },
                new Company { CompanyName = "Cty CP Posco Vietnam", TypeOfBusiniess = "Cổ phần", IDField = f3, LegalRepresentative = "Kim Dong Woo", Address = "KCN Phú Mỹ, Bà Rịa", TotalAmount = 45, QuantityAvailable = 38, QuantityNotYet = 4, AmountOfExemption = 3, TrackerID = u3 },
                new Company { CompanyName = "Cty TNHH Foxconn Bình Dương", TypeOfBusiniess = "TNHH", IDField = f2, LegalRepresentative = "Chen Wei", Address = "KCN VSIP II, Bình Dương", TotalAmount = 78, QuantityAvailable = 65, QuantityNotYet = 8, AmountOfExemption = 5, TrackerID = u2 },
                new Company { CompanyName = "Cty TNHH Panasonic VN", TypeOfBusiniess = "TNHH", IDField = f1, LegalRepresentative = "Sato Kenji", Address = "KCN Bắc Thăng Long, Hà Nội", TotalAmount = 34, QuantityAvailable = 28, QuantityNotYet = 3, AmountOfExemption = 3, TrackerID = u3 },
                new Company { CompanyName = "Cty CP Formosa VN", TypeOfBusiniess = "Cổ phần", IDField = f4, LegalRepresentative = "Tsai Wen Lin", Address = "KKT Vũng Áng, Hà Tĩnh", TotalAmount = 92, QuantityAvailable = 76, QuantityNotYet = 9, AmountOfExemption = 7, TrackerID = u3 },
                new Company { CompanyName = "Cty TNHH Toyota Motor VN", TypeOfBusiniess = "TNHH", IDField = f5, LegalRepresentative = "Yamada Akira", Address = "KCN Phúc Yên, Vĩnh Phúc", TotalAmount = 41, QuantityAvailable = 35, QuantityNotYet = 3, AmountOfExemption = 3, TrackerID = u2 },
                new Company { CompanyName = "Cty TNHH Intel Products VN", TypeOfBusiniess = "TNHH", IDField = f6, LegalRepresentative = "John Smith", Address = "SHTP, TP.HCM", TotalAmount = 112, QuantityAvailable = 95, QuantityNotYet = 10, AmountOfExemption = 7, TrackerID = u3 }
            );
            await db.SaveChangesAsync();
        }

        // ── Employees (30+ nhân viên) ──
        if (!await db.Employees.AnyAsync())
        {
            var companies = await db.Companies.ToListAsync();
            var careers = await db.Careers.ToListAsync();
            var samsung = companies.First(c => c.CompanyName.Contains("Samsung"));
            var hyundai = companies.First(c => c.CompanyName.Contains("Hyundai"));
            var canon = companies.First(c => c.CompanyName.Contains("Canon"));
            var lg = companies.First(c => c.CompanyName.Contains("LG"));
            var foxconn = companies.First(c => c.CompanyName.Contains("Foxconn"));
            var posco = companies.First(c => c.CompanyName.Contains("Posco"));
            var formosa = companies.First(c => c.CompanyName.Contains("Formosa"));
            var toyota = companies.First(c => c.CompanyName.Contains("Toyota"));
            var intel = companies.First(c => c.CompanyName.Contains("Intel"));

            var kysu = careers.First(c => c.CareerName == "Kỹ sư").IDCareer;
            var quanly = careers.First(c => c.CareerName == "Quản lý").IDCareer;
            var giamdoc = careers.First(c => c.CareerName == "Giám đốc").IDCareer;
            var kythuatvien = careers.First(c => c.CareerName == "Kỹ thuật viên").IDCareer;
            var phiendich = careers.First(c => c.CareerName == "Phiên dịch").IDCareer;
            var chuyengia = careers.First(c => c.CareerName == "Chuyên gia").IDCareer;
            var laodong = careers.First(c => c.CareerName == "Lao động").IDCareer;
            var tuvan = careers.First(c => c.CareerName == "Tư vấn").IDCareer;

            var now = DateTime.Now;
            db.Employees.AddRange(
                // Samsung - Trung Quốc
                Emp("Zhang Wei", 1, 1985,3,15, "CN", "E12345678", kysu, 1, samsung, now.AddDays(3)),
                Emp("Wang Li", 1, 1987,8,20, "CN", "E23456789", kythuatvien, 1, samsung, now.AddDays(25)),
                Emp("Liu Chen", 1, 1990,1,10, "CN", "E34567890", chuyengia, 2, samsung, now.AddMonths(8)),
                Emp("Zhao Peng", 1, 1992,5,18, "CN", "E45678901", laodong, 1, samsung, now.AddMonths(4)),
                Emp("Sun Mei", 2, 1994,11,3, "CN", "E56789012", phiendich, 1, samsung, now.AddMonths(6)),

                // Samsung - Hàn Quốc
                Emp("Kim Min-jun", 1, 1983,7,12, "KR", "M55667788", quanly, 1, samsung, now.AddDays(5)),
                Emp("Lee Soo-jin", 2, 1991,2,28, "KR", "M66778899", kysu, 1, samsung, now.AddMonths(10)),

                // Hyundai
                Emp("Park Ji-hoon", 1, 1986,4,9, "KR", "M77889900", giamdoc, 1, hyundai, now.AddDays(18)),
                Emp("Choi Yong", 1, 1989,9,14, "KR", "M88990011", kysu, 1, hyundai, now.AddMonths(3)),
                Emp("Chen Hao", 1, 1993,6,7, "CN", "E67890123", kythuatvien, 1, hyundai, now.AddMonths(5)),

                // Canon - Nhật Bản
                Emp("Tanaka Yuki", 2, 1988,11,10, "JP", "TK4567890", phiendich, 2, canon, now.AddDays(12)),
                Emp("Suzuki Takeshi", 1, 1985,3,22, "JP", "TK5678901", giamdoc, 1, canon, now.AddMonths(14)),
                Emp("Yamamoto Kenji", 1, 1991,8,5, "JP", "TK6789012", kysu, 1, canon, now.AddMonths(7)),

                // LG Display - Hàn Quốc
                Emp("Jung Seung-ho", 1, 1987,12,1, "KR", "M99001122", quanly, 1, lg, now.AddMonths(9)),
                Emp("Hwang Min-ji", 2, 1993,4,15, "KR", "M00112233", kysu, 1, lg, now.AddDays(45)),

                // Foxconn - Đài Loan + Trung Quốc
                Emp("Tsai Ming", 1, 1984,6,30, "TW", "T11223344", giamdoc, 1, foxconn, now.AddMonths(11)),
                Emp("Lin Jia-wei", 1, 1990,10,8, "TW", "T22334455", quanly, 1, foxconn, now.AddMonths(6)),
                Emp("Wu Xiao", 1, 1995,2,14, "CN", "E78901234", laodong, 0, foxconn, now.AddMonths(2)),

                // Posco - Hàn Quốc
                Emp("Han Dong-hee", 1, 1986,8,19, "KR", "M11223300", kysu, 1, posco, now.AddMonths(8)),
                Emp("Yoon Seo-yeon", 2, 1992,1,25, "KR", "M22334411", phiendich, 1, posco, now.AddDays(28)),

                // Formosa - Đài Loan
                Emp("Chen Wei-ling", 2, 1988,5,3, "TW", "T33445566", quanly, 1, formosa, now.AddMonths(4)),
                Emp("Huang Yu-ting", 1, 1985,9,17, "TW", "T44556677", chuyengia, 1, formosa, now.AddMonths(12)),
                Emp("Chang Li", 1, 1993,7,21, "TW", "T55667788", kythuatvien, 1, formosa, now.AddDays(35)),

                // Toyota - Nhật Bản
                Emp("Sato Haruki", 1, 1987,11,8, "JP", "TK7890123", giamdoc, 1, toyota, now.AddMonths(18)),
                Emp("Nakamura Aoi", 2, 1991,3,30, "JP", "TK8901234", kysu, 1, toyota, now.AddMonths(6)),

                // Intel - Đa quốc tịch
                Emp("James Wilson", 1, 1983,4,12, "US", "US1234567", chuyengia, 1, intel, now.AddMonths(15)),
                Emp("Priya Sharma", 2, 1990,8,25, "IN", "IN2345678", kysu, 1, intel, now.AddMonths(7)),
                Emp("David Lee", 1, 1988,12,3, "MY", "MY3456789", tuvan, 1, intel, now.AddMonths(9)),
                Emp("Maria Santos", 2, 1992,6,18, "PH", "PH4567890", kythuatvien, 1, intel, now.AddDays(22)),
                Emp("Budi Santoso", 1, 1989,2,7, "ID", "ID5678901", laodong, 0, intel, now.AddMonths(3))
            );
            await db.SaveChangesAsync();

            // ── Cập nhật thăm thân cho một số nhân viên ──
            var allEmps = await db.Employees.ToListAsync();
            var zhangWei = allEmps.FirstOrDefault(e => e.Passport == "E12345678");
            if (zhangWei != null)
            {
                zhangWei.FamilyVisit = 1;
                zhangWei.FamilyVisitRelativeName = "Nguyễn Thị Hoa";
                zhangWei.FamilyVisitRelationship = "Vợ";
                zhangWei.FamilyVisitRelativeIdCard = "001099012345";
                zhangWei.FamilyVisitStartDate = now.AddMonths(-6);
                zhangWei.FamilyVisitEndDate = now.AddDays(15);
                zhangWei.FamilyVisitNote = "Thăm vợ tại Bắc Ninh";
            }
            var sunMei = allEmps.FirstOrDefault(e => e.Passport == "E56789012");
            if (sunMei != null)
            {
                sunMei.FamilyVisit = 1;
                sunMei.FamilyVisitRelativeName = "Trần Văn Đức";
                sunMei.FamilyVisitRelationship = "Chồng";
                sunMei.FamilyVisitRelativeIdCard = "001099056789";
                sunMei.FamilyVisitStartDate = now.AddMonths(-3);
                sunMei.FamilyVisitEndDate = now.AddMonths(9);
                sunMei.FamilyVisitNote = "Thăm chồng tại Hà Nội";
            }
            var parkJi = allEmps.FirstOrDefault(e => e.Passport == "M77889900");
            if (parkJi != null)
            {
                parkJi.FamilyVisit = 1;
                parkJi.FamilyVisitRelativeName = "Lê Thị Mai";
                parkJi.FamilyVisitRelationship = "Vợ";
                parkJi.FamilyVisitRelativeIdCard = "030199034567";
                parkJi.FamilyVisitStartDate = now.AddMonths(-2);
                parkJi.FamilyVisitEndDate = now.AddDays(45);
            }
            var tanakaYuki = allEmps.FirstOrDefault(e => e.Passport == "TK4567890");
            if (tanakaYuki != null)
            {
                tanakaYuki.FamilyVisit = 1;
                tanakaYuki.FamilyVisitRelativeName = "Phạm Minh Tuấn";
                tanakaYuki.FamilyVisitRelationship = "Con";
                tanakaYuki.FamilyVisitRelativeIdCard = "027099078901";
                tanakaYuki.FamilyVisitStartDate = now.AddMonths(-1);
                tanakaYuki.FamilyVisitEndDate = now.AddMonths(11);
            }
            var chenWeiLing = allEmps.FirstOrDefault(e => e.Passport == "T33445566");
            if (chenWeiLing != null)
            {
                chenWeiLing.FamilyVisit = 1;
                chenWeiLing.FamilyVisitRelativeName = "Võ Văn Hùng";
                chenWeiLing.FamilyVisitRelationship = "Chồng";
                chenWeiLing.FamilyVisitRelativeIdCard = "038099023456";
                chenWeiLing.FamilyVisitStartDate = now.AddMonths(-4);
                chenWeiLing.FamilyVisitEndDate = now.AddDays(22);
                chenWeiLing.FamilyVisitNote = "Thăm chồng tại Hà Tĩnh";
            }
            var priya = allEmps.FirstOrDefault(e => e.Passport == "IN2345678");
            if (priya != null)
            {
                priya.FamilyVisit = 1;
                priya.FamilyVisitRelativeName = "Nguyễn Anh Khoa";
                priya.FamilyVisitRelationship = "Chồng";
                priya.FamilyVisitRelativeIdCard = "079099034567";
                priya.FamilyVisitStartDate = now.AddMonths(-5);
                priya.FamilyVisitEndDate = now.AddMonths(7);
            }
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

    private static Employee Emp(string name, int gender, int y, int m, int d,
        string nat, string passport, int careerId, int workPermit,
        Company company, DateTime tempStay)
    {
        return new Employee
        {
            StaffName = name, Gender = gender, Birthday = new DateTime(y, m, d),
            Nationality = nat, Passport = passport, IDCareer = careerId,
            WorkPermit = workPermit, TemporaryStay = tempStay,
            IDUser = 1, IDCompany = company.IDCompany, DateCreated = DateTime.Now
        };
    }
}
