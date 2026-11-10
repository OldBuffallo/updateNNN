using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for WebRoleAssignment; never tracked by EF.</summary>
public sealed class WebRoleAssignmentDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string RoleCode { get; set; } = "VIEWER";
    public int? AdministrativeUnitId { get; set; }
    public AccountDto? Account { get; set; }
    public AdministrativeUnitDto? AdministrativeUnit { get; set; }
}
