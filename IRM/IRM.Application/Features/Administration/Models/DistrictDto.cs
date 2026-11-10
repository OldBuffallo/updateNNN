using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for District; never tracked by EF.</summary>
public sealed class DistrictDto
{
    public int IDDistrict { get; set; }
    public string DisTrictName { get; set; } = "";
    public int Delete_flag { get; set; }
    public bool IsActive => Delete_flag == 0;
}
