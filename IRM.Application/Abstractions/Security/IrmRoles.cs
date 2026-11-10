using IRM.Data.Models;

namespace IRM.Services;

public static class IrmRoles
{
    public const string Admin = "ADMIN";
    public const string DataEditor = "DATA_EDITOR";
    public const string Inspector = "INSPECTOR";
    public const string Reporter = "REPORTER";
    public const string Viewer = "VIEWER";
    public const string AnyUser = Admin + "," + DataEditor + "," + Inspector + "," + Reporter + "," + Viewer;
}
