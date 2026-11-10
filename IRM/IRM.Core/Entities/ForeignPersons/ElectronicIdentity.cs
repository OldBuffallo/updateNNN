

namespace IRM.Data.Models;

public class ElectronicIdentity
{
    public int Id { get; set; }
    public int ForeignPersonId { get; set; }
    public string IdentityNumber { get; set; } = "";
    public string IdentitySearchKey { get; set; } = "";
    public DateTime? IssuedAt { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string StatusCode { get; set; } = "ACTIVE";

    public ForeignPerson? ForeignPerson { get; set; }
}
