using IRM.Data.Models;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class AdminAndRbacFeatureTddTests
{
    [Fact]
    public async Task AuthenticateWebAsync_AutoUpgradesLegacyPlaintextPassword()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var authService = new AuthService(db.Context, audit);

        var legacyAccount = new Account
        {
            Username = "manager",
            Name = "Manager User",
            Password = "legacy_plaintext_password",
            Permission = AccountPermission.Admin,
            Delete_flag = 0
        };
        db.Context.Accounts.Add(legacyAccount);
        await db.Context.SaveChangesAsync();

        var principal = await authService.AuthenticateWebAsync("manager", "legacy_plaintext_password", "127.0.0.1");
        Assert.NotNull(principal);
        Assert.True(principal!.IsInRole(IrmRoles.Admin));

        // Verify WebCredentials created with BCrypt/PBKDF2 hash (not plaintext)
        var cred = await db.Context.WebCredentials.SingleAsync(x => x.AccountId == legacyAccount.IDUser);
        Assert.NotEqual("legacy_plaintext_password", cred.PasswordHash);
        Assert.True(cred.PasswordHash.Length > 20);

        // Next login succeeds with same password using hashed credential
        var principal2 = await authService.AuthenticateWebAsync("manager", "legacy_plaintext_password", "127.0.0.1");
        Assert.NotNull(principal2);
    }

    [Fact]
    public async Task AuthenticateWebAsync_LocksAccount_After5FailuresFor15Minutes()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var authService = new AuthService(db.Context, audit);

        var account = new Account
        {
            Username = "victim",
            Name = "Victim",
            Password = "correct_password",
            Permission = (AccountPermission)2,
            Delete_flag = 0
        };
        db.Context.Accounts.Add(account);
        await db.Context.SaveChangesAsync();

        // 4 failed attempts
        for (int i = 0; i < 4; i++)
        {
            var res = await authService.AuthenticateWebAsync("victim", "wrong_password", "127.0.0.1");
            Assert.Null(res);
        }

        var credBefore5 = await db.Context.WebCredentials.SingleOrDefaultAsync(x => x.AccountId == account.IDUser);
        Assert.Null(credBefore5?.LockedUntil);

        // 5th failed attempt -> locks account
        var res5 = await authService.AuthenticateWebAsync("victim", "wrong_password", "127.0.0.1");
        Assert.Null(res5);

        var credAfter5 = await db.Context.WebCredentials.SingleAsync(x => x.AccountId == account.IDUser);
        Assert.NotNull(credAfter5.LockedUntil);
        Assert.True(credAfter5.LockedUntil > DateTime.UtcNow);

        // 6th attempt with CORRECT password -> still blocked due to lock
        var res6 = await authService.AuthenticateWebAsync("victim", "correct_password", "127.0.0.1");
        Assert.Null(res6);
    }

    [Fact]
    public async Task CatalogService_CrudOperations_WithSoftDelete()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var service = new CatalogService(db.Context, TestSecurityContext.CreateAllowAllGuard());

        // Create Field
        var field = new Field { FieldName = "Nông nghiệp công nghệ cao", Description = "Lĩnh vực mới" };
        var createdField = await service.CreateFieldAsync(field);
        Assert.True(createdField.IDField > 0);

        // Update Field
        createdField.Description = "Đã cập nhật";
        var updateSuccess = await service.UpdateFieldAsync(createdField);
        Assert.True(updateSuccess);

        // Delete Field (Soft Delete)
        var deleteSuccess = await service.DeleteFieldAsync(createdField.IDField);
        Assert.True(deleteSuccess);

        var activeFields = await service.GetFieldsAsync();
        Assert.DoesNotContain(activeFields, x => x.IDField == createdField.IDField);

        var dbField = await db.Context.Fields.FindAsync(createdField.IDField);
        Assert.NotNull(dbField);
        Assert.Equal(1, dbField!.Delete_flag);
    }
}
