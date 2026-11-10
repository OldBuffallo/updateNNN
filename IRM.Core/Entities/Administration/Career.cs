

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
