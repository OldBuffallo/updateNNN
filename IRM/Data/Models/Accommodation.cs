namespace IRM.Data.Models;

/// <summary>Một địa điểm lưu trú vật lý. Quan hệ thuê và người ở được lưu theo thời gian.</summary>
public class Accommodation
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string TypeCode { get; set; } = "RENTED";
    public string AddressLine { get; set; } = "";
    public int? AdministrativeUnitId { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public int? Capacity { get; set; }
    public string? Note { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public AdministrativeUnit? AdministrativeUnit { get; set; }
    public ICollection<CompanyAccommodationAgreement> CompanyAgreements { get; set; } = new List<CompanyAccommodationAgreement>();
    public ICollection<ResidencePeriod> ResidencePeriods { get; set; } = new List<ResidencePeriod>();
}
