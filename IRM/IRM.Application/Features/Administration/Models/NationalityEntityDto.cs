using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for NationalityEntity; never tracked by EF.</summary>
public sealed class NationalityEntityDto
{
    public int IDNationality { get; set; }
    public string NationalityCode { get; set; } = "";
    public string NationalityName { get; set; } = "";
    public int Delete_flag { get; set; }
    public bool IsActive => Delete_flag == 0;
}
