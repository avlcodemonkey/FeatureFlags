using FeatureFlags.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Domain.Tests;

public class FeatureFlagsDbContextTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture> {
    private readonly DatabaseFixture _Fixture = fixture;

    [Fact]
    public async Task SaveChangesAsync_CreateLanguage_SetsCreatedAndUpdatedDate() {
        // arrange
        var createStartTime = DateTime.UtcNow.AddMinutes(-1);
        var createUserId = _Fixture.UserForCreate.Id;
        var languageForCreate = new Language { Id = -1, Name = "create language", CountryCode = "country1", LanguageCode = "lang1" };

        // act
        Language createdLanguage;
        using (var dbContext = _Fixture.CreateContextForCreate()) {
            dbContext.Languages.Add(languageForCreate);
            await dbContext.SaveChangesAsync();
            createdLanguage = dbContext.Languages.First(x => x.Id == languageForCreate.Id);
        }

        // assert
        Assert.NotNull(createdLanguage);
        Assert.Equal(languageForCreate.Name, createdLanguage.Name);
        Assert.True(createStartTime <= createdLanguage.CreatedDate);
        Assert.True(createStartTime <= createdLanguage.UpdatedDate);
    }

    [Fact]
    public async Task SaveChangesAsync_UpdateLanguage_SetsUpdatedDate() {
        // arrange
        var newLanguageName = "new language name";
        var languageForUpdate = new Language { Id = -2, Name = "update language", CountryCode = "country2", LanguageCode = "lang2" };
        using (var dbContext = _Fixture.CreateContextForCreate()) {
            dbContext.Languages.Add(languageForUpdate);
            await dbContext.SaveChangesAsync();
        }
        await Task.Delay(1100);
        var updateStartTime = DateTime.UtcNow;
        await Task.Delay(1100);

        // act
        Language updatedLanguage;
        using (var dbContext = _Fixture.CreateContextForUpdate()) {
            updatedLanguage = dbContext.Languages.First(x => x.Id == languageForUpdate.Id);
            updatedLanguage.Name = newLanguageName;
            dbContext.Languages.Update(updatedLanguage);
            await dbContext.SaveChangesAsync();
            updatedLanguage = dbContext.Languages.First(x => x.Id == languageForUpdate.Id);
        }

        // assert
        Assert.NotNull(updatedLanguage);
        Assert.Equal(newLanguageName, updatedLanguage.Name);
        Assert.NotEqual(updatedLanguage.CreatedDate, updatedLanguage.UpdatedDate);
        Assert.True(updateStartTime >= updatedLanguage.CreatedDate);
        Assert.True(updateStartTime <= updatedLanguage.UpdatedDate);
    }

    [Fact]
    public async Task SaveChangesAsync_CreateLanguage_CreatesAuditLog() {
        // arrange
        var createStartTime = DateTime.UtcNow;
        var createUserId = _Fixture.UserForCreate.Id;
        var languageForCreateAudit = new Language { Id = -3, Name = "audit create language", CountryCode = "country3", LanguageCode = "lang3" };

        // act
        List<AuditLog> auditLogs;
        using (var dbContext = _Fixture.CreateContextForCreate()) {
            dbContext.Languages.Add(languageForCreateAudit);
            await dbContext.SaveChangesAsync();
            auditLogs = await dbContext.AuditLog.Where(x => x.Entity == nameof(Language) && x.PrimaryKey == languageForCreateAudit.Id).ToListAsync();
        }

        // assert
        Assert.NotNull(auditLogs);
        Assert.Single(auditLogs);

        Assert.All(auditLogs, x => Assert.Equal(createUserId, x.UserId));
        Assert.All(auditLogs, x => Assert.Equal(languageForCreateAudit.Id, x.PrimaryKey));
        Assert.All(auditLogs, x => Assert.Equal(EntityState.Added, x.State));
        Assert.All(auditLogs, x => Assert.True(createStartTime <= x.Date));
        Assert.All(auditLogs, x => Assert.Contains(languageForCreateAudit.Name, x.NewValues));
    }

    [Fact]
    public async Task SaveChangesAsync_UpdateLanguage_CreatesAuditLog() {
        // arrange
        var updateStartTime = DateTime.UtcNow;
        var createUserId = _Fixture.UserForCreate.Id;
        var updateUserId = _Fixture.UserForUpdate.Id;
        var newLanguageName = "new language name";
        var languageForUpdateAudit = new Language { Id = -4, Name = "audit update language", CountryCode = "country4", LanguageCode = "lang4" };
        using (var dbContext = _Fixture.CreateContextForCreate()) {
            dbContext.Languages.Add(languageForUpdateAudit);
            await dbContext.SaveChangesAsync();
        }

        // act
        Language updatedLanguage;
        List<AuditLog> auditLogs;
        using (var dbContext = _Fixture.CreateContextForUpdate()) {
            updatedLanguage = dbContext.Languages.First(x => x.Id == languageForUpdateAudit.Id);
            updatedLanguage.Name = newLanguageName;
            dbContext.Languages.Update(updatedLanguage);
            await dbContext.SaveChangesAsync();
            updatedLanguage = dbContext.Languages.First(x => x.Id == languageForUpdateAudit.Id);
            auditLogs = await dbContext.AuditLog
                .Where(x => x.Entity == nameof(Language) && x.PrimaryKey == languageForUpdateAudit.Id && x.State == EntityState.Modified).ToListAsync();
        }

        // assert
        Assert.NotNull(auditLogs);
        Assert.Single(auditLogs);

        Assert.All(auditLogs, x => Assert.Equal(updateUserId, x.UserId));
        Assert.All(auditLogs, x => Assert.Equal(languageForUpdateAudit.Id, x.PrimaryKey));
        Assert.All(auditLogs, x => Assert.True(updateStartTime <= x.Date));
        Assert.All(auditLogs, x => Assert.Contains(newLanguageName, x.NewValues));
    }

    [Fact]
    public async Task SaveChangesAsync_DeleteLanguage_CreatesAuditLog() {
        // arrange
        var deleteStartTime = DateTime.UtcNow;
        var deleteUserId = _Fixture.UserForDelete.Id;
        var languageForDeleteAudit = new Language { Id = -5, Name = "audit delete language", CountryCode = "country5", LanguageCode = "lang5" };

        // act
        List<AuditLog> auditLogs;
        using (var dbContext = _Fixture.CreateContextForDelete()) {
            dbContext.Languages.Add(languageForDeleteAudit);
            await dbContext.SaveChangesAsync();
            var deletedLanguage = dbContext.Languages.First(x => x.Id == languageForDeleteAudit.Id);
            dbContext.Languages.Remove(deletedLanguage);
            await dbContext.SaveChangesAsync();
            auditLogs = await dbContext.AuditLog
                .Where(x => x.Entity == nameof(Language) && x.PrimaryKey == languageForDeleteAudit.Id && x.State == EntityState.Deleted).ToListAsync();
        }

        // assert
        Assert.NotNull(auditLogs);
        Assert.Single(auditLogs);

        Assert.All(auditLogs, x => Assert.Equal(deleteUserId, x.UserId));
        Assert.All(auditLogs, x => Assert.Equal(languageForDeleteAudit.Id, x.PrimaryKey));
        Assert.All(auditLogs, x => Assert.True(deleteStartTime <= x.Date));
        Assert.All(auditLogs, x => Assert.Contains(languageForDeleteAudit.Name, x.OldValues));
    }


    [Fact]
    public async Task SaveChangesAsync_UpdateLanguageNoOp_DoesNotCreateAuditLog() {
        // arrange
        var languageForNoOpAudit = new Language { Id = -5, Name = "audit noOp language", CountryCode = "country5", LanguageCode = "lang5" };
        using (var dbContext = _Fixture.CreateContextForCreate()) {
            dbContext.Languages.Add(languageForNoOpAudit);
            await dbContext.SaveChangesAsync();
        }

        // act
        Language updatedLanguage;
        List<AuditLog> auditLogs;
        using (var dbContext = _Fixture.CreateContextForUpdate()) {
            updatedLanguage = dbContext.Languages.First(x => x.Id == languageForNoOpAudit.Id);
            dbContext.Languages.Update(updatedLanguage);
            await dbContext.SaveChangesAsync();
            updatedLanguage = dbContext.Languages.First(x => x.Id == languageForNoOpAudit.Id);
            auditLogs = await dbContext.AuditLog
                .Where(x => x.Entity == nameof(Language) && x.PrimaryKey == languageForNoOpAudit.Id && x.State == EntityState.Modified).ToListAsync();
        }

        // assert
        Assert.NotNull(auditLogs);
        Assert.Empty(auditLogs);
    }

    [Fact]
    public async Task SaveChangesAsync_CreateRole_CreatesMultipleAuditLogs() {
        // arrange
        var createStartTime = DateTime.UtcNow;
        var createUserId = _Fixture.UserForCreate.Id;
        var rolePermissionForCreateAudit = new RolePermission { Id = -1, RoleId = -1, PermissionId = 1 };
        var roleForCreateAudit = new Role {
            Id = -1, Name = "audit create role", IsDefault = false,
            RolePermissions = [rolePermissionForCreateAudit]
        };

        // act
        List<AuditLog> roleAuditLogs;
        List<AuditLog> rolePermissionAuditLogs;
        using (var dbContext = _Fixture.CreateContextForCreate()) {
            dbContext.Roles.Add(roleForCreateAudit);
            await dbContext.SaveChangesAsync();
            roleAuditLogs = await dbContext.AuditLog
                .Where(x => x.Entity == nameof(Role) && x.PrimaryKey == roleForCreateAudit.Id).ToListAsync();
            rolePermissionAuditLogs = await dbContext.AuditLog
                .Where(x => x.Entity == nameof(RolePermission) && x.PrimaryKey == rolePermissionForCreateAudit.Id).ToListAsync();
        }

        // assert
        Assert.NotNull(roleAuditLogs);
        Assert.Single(roleAuditLogs);
        Assert.All(roleAuditLogs, x => Assert.Equal(createUserId, x.UserId));
        Assert.All(roleAuditLogs, x => Assert.Equal(roleForCreateAudit.Id, x.PrimaryKey));
        Assert.All(roleAuditLogs, x => Assert.Equal(EntityState.Added, x.State));
        Assert.All(roleAuditLogs, x => Assert.True(createStartTime <= x.Date));
        Assert.All(roleAuditLogs, x => Assert.Contains(roleForCreateAudit.Name, x.NewValues));

        Assert.NotNull(rolePermissionAuditLogs);
        Assert.Single(rolePermissionAuditLogs);
        Assert.All(rolePermissionAuditLogs, x => Assert.Equal(createUserId, x.UserId));
        Assert.All(rolePermissionAuditLogs, x => Assert.Equal(rolePermissionForCreateAudit.Id, x.PrimaryKey));
        Assert.All(rolePermissionAuditLogs, x => Assert.Equal(EntityState.Added, x.State));
        Assert.All(rolePermissionAuditLogs, x => Assert.True(createStartTime <= x.Date));
    }

    [Fact]
    public async Task SaveChangesAsync_RoleDeleteRolePermission_CreatesAuditLog() {
        // arrange
        var deleteStartTime = DateTime.UtcNow;
        var deleteUserId = _Fixture.UserForDelete.Id;
        var rolePermissionForDeleteAudit = new RolePermission { Id = -2, RoleId = -2, PermissionId = 1 };
        var roleForDeleteAudit = new Role {
            Id = -2, Name = "audit delete role permission", IsDefault = false,
            RolePermissions = [rolePermissionForDeleteAudit]
        };

        // act
        List<AuditLog> roleAuditLogs;
        List<AuditLog> rolePermissionAuditLogs;
        using (var dbContext = _Fixture.CreateContextForDelete()) {
            dbContext.Roles.Add(roleForDeleteAudit);
            await dbContext.SaveChangesAsync();
            var createdRole = dbContext.Roles.Include(x => x.RolePermissions).First(x => x.Id == roleForDeleteAudit.Id);
            dbContext.RolePermissions.RemoveRange(createdRole.RolePermissions);
            await dbContext.SaveChangesAsync();

            roleAuditLogs = await dbContext.AuditLog
                .Where(x => x.Entity == nameof(Role) && x.PrimaryKey == roleForDeleteAudit.Id && x.State == EntityState.Deleted).ToListAsync();
            rolePermissionAuditLogs = await dbContext.AuditLog
                .Where(x => x.Entity == nameof(RolePermission) && x.PrimaryKey == rolePermissionForDeleteAudit.Id && x.State == EntityState.Deleted).ToListAsync();
        }

        // assert
        Assert.NotNull(roleAuditLogs);
        Assert.Empty(roleAuditLogs);

        Assert.NotNull(rolePermissionAuditLogs);
        Assert.Single(rolePermissionAuditLogs);
        Assert.All(rolePermissionAuditLogs, x => Assert.Equal(deleteUserId, x.UserId));
        Assert.All(rolePermissionAuditLogs, x => Assert.Equal(rolePermissionForDeleteAudit.Id, x.PrimaryKey));
        Assert.All(rolePermissionAuditLogs, x => Assert.True(deleteStartTime <= x.Date));
    }
}
