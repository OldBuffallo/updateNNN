using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for Career; never tracked by EF.</summary>
public sealed class CareerDto
{
    public int IDCareer { get; set; }
    public string CareerName { get; set; } = "";
    public int IDCG { get; set; }
    public int Delete_flag { get; set; }
    public CareerGroupDto? CareerGroup { get; set; }
    public bool IsActive => Delete_flag == 0;
}
