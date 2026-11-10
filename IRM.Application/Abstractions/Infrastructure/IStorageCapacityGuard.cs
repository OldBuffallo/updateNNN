using IRM.Data.Models;

namespace IRM.Services;

public interface IStorageCapacityGuard
{
    void EnsureCanWrite(string path);
}
