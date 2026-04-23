namespace IRM.Data.Models;

/// <summary>
/// Bảng Careers — Ngành nghề
/// </summary>
public class Career
{
    public int IDCareer { get; set; }
    public string CareerName { get; set; } = "";
    public int IDCG { get; set; }
    public int Delete_flag { get; set; }

    public CareerGroup? CareerGroup { get; set; }
    public bool IsActive => Delete_flag == 0;
}

/// <summary>
/// Bảng CareerGroups — Nhóm ngành nghề
/// </summary>
public class CareerGroup
{
    public int IDCG { get; set; }
    public string CareerGroupName { get; set; } = "";

    public ICollection<Career> Careers { get; set; } = new List<Career>();
}
