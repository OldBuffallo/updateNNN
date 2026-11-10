

namespace IRM.Data.Models;

public class WebRoleAssignment
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string RoleCode { get; set; } = "VIEWER";
    public int? AdministrativeUnitId { get; set; }
    public Account? Account { get; set; }
    public AdministrativeUnit? AdministrativeUnit { get; set; }
}
