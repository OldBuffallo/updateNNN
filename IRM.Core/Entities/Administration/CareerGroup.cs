

namespace IRM.Data.Models;

/// <summary>
/// Bảng CareerGroups — Nhóm ngành nghề
/// </summary>
public class CareerGroup
{
    public int IDCG { get; set; }
    public string CareerGroupName { get; set; } = "";

    public ICollection<Career> Careers { get; set; } = new List<Career>();
}
