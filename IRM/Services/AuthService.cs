using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

/// <summary>
/// Service Xác thực — Đăng nhập (sử dụng plain text password theo yêu cầu)
/// </summary>
public class AuthService
{
    private readonly IrmDbContext _db;
    public AuthService(IrmDbContext db) => _db = db;

    public async Task<Account?> LoginAsync(string username, string password)
    {
        return await _db.Accounts.FirstOrDefaultAsync(a =>
            a.Delete_flag == 0 &&
            a.Username == username &&
            a.Password == password);
    }

    public async Task<List<Account>> GetAllAccountsAsync()
    {
        return await _db.Accounts
            .Where(a => a.Delete_flag == 0)
            .OrderBy(a => a.Username)
            .ToListAsync();
    }

    public async Task CreateAccountAsync(Account account)
    {
        account.Delete_flag = 0;
        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAccountAsync(Account account)
    {
        _db.Accounts.Update(account);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(int id)
    {
        var acc = await _db.Accounts.FindAsync(id);
        if (acc != null)
        {
            acc.Delete_flag = 1;
            await _db.SaveChangesAsync();
        }
    }
}
