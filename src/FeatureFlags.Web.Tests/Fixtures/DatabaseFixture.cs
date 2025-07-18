using System.Security.Principal;
using FeatureFlags.Domain;
using FeatureFlags.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FeatureFlags.Web.Tests.Fixtures;

/// <summary>
/// See https://learn.microsoft.com/en-us/ef/core/testing/testing-with-the-database
/// for more details about using a database fixture for testing with xUnit.
/// </summary>
public class DatabaseFixture : IDisposable {
    private const string _ConnectionString = "Data Source=:memory:";
    private readonly SqliteConnection _Connection;

    private readonly Mock<IConfiguration> _MockConfiguration;
    private static readonly Lock _Lock = new();
    private static bool _DatabaseInitialized;

    private static IHttpContextAccessor CreateHttpContextAccessor(User? user = null) {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        if (user != null) {
            mockHttpContextAccessor.Setup(x => x.HttpContext!.User.Identity).Returns(new GenericIdentity(user.Email, "test"));
        }
        return mockHttpContextAccessor.Object;
    }

    public DatabaseFixture() {
        _MockConfiguration = new Mock<IConfiguration>();

        // creates the SQLite in-memory database, which will persist until the connection is closed at the end of the test (see Dispose below).
        _Connection = new SqliteConnection(_ConnectionString);
        _Connection.Open();

        // lock allows us to use this fixture safely with multiple classes of tests if needed
        lock (_Lock) {
            if (!_DatabaseInitialized) {
                // create the schema and data we will need for each test
                using (var dbContext = CreateContext()) {
                    dbContext.Database.Migrate();

                    dbContext.Users.Add(User);
                    dbContext.Permissions.Add(TestPermission);
                    dbContext.Roles.Add(TestRole);
                    dbContext.Users.Add(TestUser);

                    TestAuditLog.User = User;
                    TestAuditLog2.User = TestUser;
                    dbContext.AuditLog.Add(TestAuditLog);
                    dbContext.AuditLog.Add(TestAuditLog2);

                    dbContext.FeatureFlags.Add(TestFeatureFlag);

                    dbContext.SaveChanges();
                }

                _DatabaseInitialized = true;
            }
        }
    }

    public FeatureFlagsDbContext CreateContext(User? user)
        => new(new DbContextOptionsBuilder<FeatureFlagsDbContext>().UseSqlite(_Connection).Options, _MockConfiguration.Object, CreateHttpContextAccessor(user));

    public FeatureFlagsDbContext CreateContext() => CreateContext(User);

    public User User { get; } = new() { Id = -1, Email = "user@fake.com", LanguageId = -1, Name = "User", Status = true };

    public Role TestRole { get; } = new() {
        Id = -1, Name = "Test", IsDefault = false,
        RolePermissions = new List<RolePermission> { new() { Id = -2, RoleId = -1, PermissionId = -3 } }
    };

    public Permission TestPermission { get; } = new() { Id = -3, ControllerName = "controller", ActionName = "action" };

    public User TestUser { get; } = new() {
        Id = -2, Name = "name", Email = "email@domain.com", Status = true,
        LanguageId = 1, UserRoles = new List<UserRole> { new() { Id = -2, UserId = -2, RoleId = -1 } }
    };

    public AuditLog TestAuditLog { get; } = new() {
        Id = 1, BatchId = Guid.NewGuid(), Date = DateTime.MinValue, Entity = "test1", State = EntityState.Modified,
        PrimaryKey = 10, UserId = 100, OldValues = "old", NewValues = "new"
    };

    public AuditLog TestAuditLog2 { get; } = new() {
        Id = 2, BatchId = Guid.NewGuid(), Date = DateTime.MinValue, Entity = "test2", State = EntityState.Deleted,
        PrimaryKey = 20, UserId = 200, OldValues = "old", NewValues = "new"
    };

    public FeatureFlag TestFeatureFlag { get; } = new() {
        Id = -1, Name = "name", IsEnabled = true
    };

    public void Dispose() {
        _Connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
