using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for CareerGroup; never tracked by EF.</summary>
public sealed class CareerGroupDto
{
    public int IDCG { get; set; }
    public string CareerGroupName { get; set; } = "";
    public ICollection<CareerDto> Careers { get; set; } = new List<CareerDto>();
}
