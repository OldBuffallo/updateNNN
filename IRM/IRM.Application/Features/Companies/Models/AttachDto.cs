using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for Attach; never tracked by EF.</summary>
public sealed class AttachDto
{
    public int IDAttach { get; set; }
    public int IDCompany { get; set; }
    public AttachType Type { get; set; }
    public string? Name { get; set; }
    public string? Folder { get; set; }
    public DateTime? DateCreated { get; set; }
    public DateTime? DateModified { get; set; }
    public int Delete_flag { get; set; }
    public CompanyDto? Company { get; set; }
    public string TypeString => Type.ToDisplay();
    public bool IsActive => Delete_flag == 0;
}
