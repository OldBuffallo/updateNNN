using IRM.Data.Models;

namespace IRM.Application.Abstractions;

public interface IPasswordHashService
{
    string Hash(Account account, string password);
    bool Verify(Account account, string hash, string password);
}
