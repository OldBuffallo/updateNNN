

namespace IRM.Data.Models;

public class CompanyRepresentative
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string FullName { get; set; } = "";
    public string? Title { get; set; }
    public string? IdNumber { get; set; }
    public string? Phone { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public Company? Company { get; set; }
}
