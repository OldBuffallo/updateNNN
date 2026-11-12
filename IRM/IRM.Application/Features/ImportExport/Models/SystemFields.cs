

namespace IRM.Data.Models;

/// <summary>Các trường hệ thống có thể map</summary>
public static class SystemFields
{
    public static readonly Dictionary<string, string> All = new()
    {
        { "StaffName",     "Họ tên" },
        { "Gender",        "Giới tính" },
        { "Birthday",      "Ngày sinh" },
        { "Nationality",   "Quốc tịch" },
        { "Passport",      "Số hộ chiếu" },
        { "Address",       "Địa chỉ" },
        { "Career",        "Nghề nghiệp/Chức danh" },
        { "WorkPermit",    "GPLĐ" },
        { "WorkPermitNumber", "Số GPLĐ" },
        { "VisaNumber",    "Số Visa" },
        { "VisaSymbol",    "Ký hiệu Visa" },
        { "TemporaryStay", "Hạn tạm trú" },
        { "Note",          "Ghi chú" },
        { "SettlementResults", "Kết quả giải quyết" },
        { "CardCreationDate", "Ngày cấp thẻ" },
        // Thăm thân
        { "FamilyVisit",              "Thăm thân (Có/Không)" },
        { "FamilyVisitRelativeName",  "Tên người thân" },
        { "FamilyVisitRelationship",  "Quan hệ người thân" },
        { "FamilyVisitRelativeIdCard","CMND/CCCD người thân" },
        { "FamilyVisitStartDate",     "Ngày bắt đầu thăm thân" },
        { "FamilyVisitEndDate",       "Hạn thăm thân" },
        { "FamilyVisitNote",          "Ghi chú thăm thân" },
        { "StayPurposeCode",          "Diện cư trú (WORK/STUDY/FAMILY_VISIT/TOURISM/OTHER)" },
        { "StayValidFrom",            "Diện cư trú từ ngày" },
        { "StayValidTo",              "Diện cư trú đến ngày" },
        { "ElectronicIdentityNumber", "Số định danh điện tử" },
        { "DocumentTypeCode",         "Loại giấy tờ (phân cách ; nếu nhiều)" },
        { "DocumentNumber",           "Số thị thực/gia hạn/thẻ (phân cách ;)" },
        { "DocumentValidFrom",        "Giấy tờ hiệu lực từ" },
        { "DocumentValidTo",          "Giấy tờ hiệu lực đến (phân cách ;)" },
        { "AdministrativeUnitCode",   "Mã xã/phường/đặc khu" },
        { "ResidenceAddress",         "Địa chỉ lưu trú" },
        { "ResidenceValidFrom",       "Lưu trú từ ngày" },
        { "ResidenceValidTo",         "Lưu trú đến ngày" },
        { "CompanyName",    "Công ty (CQ bảo lãnh)" },
        { "skip",          "— Bỏ qua —" }
    };

    /// <summary>Keywords tự động nhận dạng cột Excel → trường hệ thống</summary>
    public static readonly Dictionary<string, string[]> AutoMapKeywords = new()
    {
        { "StaffName",     new[] { "họ tên", "ho ten", "full name", "name", "tên", "ten", "họ và tên" } },
        { "Gender",        new[] { "giới tính", "gioi tinh", "gender", "phái", "gt" } },
        { "Birthday",      new[] { "ngày sinh", "ngay sinh", "birthday", "dob", "sinh" } },
        { "Nationality",   new[] { "quốc tịch", "quoc tich", "nationality", "nước", "nuoc" } },
        { "Passport",      new[] { "hộ chiếu", "ho chieu", "passport", "hc", "số hộ chiếu", "so ho chieu" } },
        { "Address",       new[] { "địa chỉ", "dia chi", "address", "đ/c" } },
        { "Career",        new[] { "nghề nghiệp", "nghe nghiep", "career", "chức danh", "chuc danh", "vị trí", "vi tri", "job" } },
        { "WorkPermit",    new[] { "gplđ", "gpld", "work permit", "giấy phép" } },
        { "WorkPermitNumber", new[] { "số gplđ", "so gpld", "wp number" } },
        { "VisaNumber",    new[] { "visa", "số visa", "so visa", "visa number" } },
        { "VisaSymbol",    new[] { "ký hiệu visa", "ky hieu visa", "visa symbol", "visa code", "ký hiệu", "ky hieu" } },
        { "TemporaryStay", new[] { "tạm trú", "tam tru", "temporary stay", "hạn tạm trú", "han tam tru" } },
        { "Note",          new[] { "ghi chú", "ghi chu", "note", "chú thích" } },
        // Thăm thân
        { "FamilyVisit",              new[] { "thăm thân", "tham than", "family visit", "diện thăm thân", "dien tham than" } },
        { "FamilyVisitRelativeName",  new[] { "tên người thân", "ten nguoi than", "người bảo lãnh", "nguoi bao lanh", "relative name" } },
        { "FamilyVisitRelationship",  new[] { "quan hệ", "quan he", "mối quan hệ", "relationship", "moi quan he" } },
        { "FamilyVisitRelativeIdCard",new[] { "cmnd người thân", "cccd người thân", "cmnd nguoi than", "id card", "căn cước người thân" } },
        { "FamilyVisitStartDate",     new[] { "ngày thăm thân", "ngay tham than", "bắt đầu thăm thân", "bat dau tham than" } },
        { "FamilyVisitEndDate",       new[] { "hạn thăm thân", "han tham than", "hết hạn thăm thân", "het han tham than", "family visit expiry" } },
        { "StayPurposeCode",          new[] { "diện cư trú", "dien cu tru", "stay purpose", "purpose code" } },
        { "StayValidFrom",            new[] { "diện từ ngày", "dien tu ngay", "stay valid from" } },
        { "StayValidTo",              new[] { "diện đến ngày", "dien den ngay", "stay valid to" } },
        { "ElectronicIdentityNumber", new[] { "số định danh điện tử", "so dinh danh dien tu", "electronic identity", "eid" } },
        { "DocumentTypeCode",         new[] { "loại giấy tờ", "loai giay to", "document type" } },
        { "DocumentNumber",           new[] { "số giấy tờ", "so giay to", "document number", "số thẻ", "so the" } },
        { "DocumentValidFrom",        new[] { "giấy tờ từ ngày", "giay to tu ngay", "document valid from" } },
        { "DocumentValidTo",          new[] { "giấy tờ đến ngày", "giay to den ngay", "document valid to" } },
        { "AdministrativeUnitCode",   new[] { "mã địa bàn", "ma dia ban", "administrative unit code" } },
        { "ResidenceAddress",         new[] { "địa chỉ lưu trú", "dia chi luu tru", "residence address" } },
        { "ResidenceValidFrom",       new[] { "lưu trú từ ngày", "luu tru tu ngay", "residence valid from" } },
        { "ResidenceValidTo",         new[] { "lưu trú đến ngày", "luu tru den ngay", "residence valid to" } },
        { "CompanyName",               new[] { "công ty", "cong ty", "company", "cq bảo lãnh", "cq bao lanh", "đơn vị", "don vi", "doanh nghiệp", "doanh nghiep", "tên công ty", "ten cong ty" } },
    };
}
