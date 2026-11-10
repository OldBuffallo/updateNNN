using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for Ward; never tracked by EF.</summary>
public sealed class WardDto
{
    public int IDWard { get; set; }
    public string WardName { get; set; } = "";
    public int Delete_flag { get; set; }
    public bool IsActive => Delete_flag == 0;
}
