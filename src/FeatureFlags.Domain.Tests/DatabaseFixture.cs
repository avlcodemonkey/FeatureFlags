using System.Security.Principal;
using FeatureFlags.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FeatureFlags.Domain.Tests;

/// <summary>
/// See https://learn.microsoft.com/en-us/ef/core/testing/testing-with-the-database
/// for more details about using a database fixture for testing with xUnit.
/// </summary>
public class DatabaseFixture : IDisposable {
    private const string _ConnectionString = "Data Source=:memory:";
    private readonly SqliteConnection _Connection;

    private readonly Mock<IConfiguration> _MockConfiguration;
    private static readonly object _Lock = new();
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
                // create the schema and user data we will need for each test
                using (var dbContext = CreateContext()) {
                    dbContext.Database.Migrate();

                    dbContext.Users.Add(UserForCreate);
                    dbContext.Users.Add(UserForUpdate);
                    dbContext.Users.Add(UserForDelete);
                    dbContext.SaveChanges();
                }

                _DatabaseInitialized = true;
            }
        }
    }

    public FeatureFlagsDbContext CreateContext(User? user = null)
        => new(new DbContextOptionsBuilder<FeatureFlagsDbContext>().UseSqlite(_Connection).Options, _MockConfiguration.Object, CreateHttpContextAccessor(user));

    public FeatureFlagsDbContext CreateContextForCreate() => CreateContext(UserForCreate);

    public FeatureFlagsDbContext CreateContextForUpdate() => CreateContext(UserForUpdate);

    public FeatureFlagsDbContext CreateContextForDelete() => CreateContext(UserForDelete);

    public User UserForCreate { get; } = new() { Id = -1, Email = "create_user@fake.com", LanguageId = -1, Name = "Create" };
    public User UserForUpdate { get; } = new() { Id = -2, Email = "update_user@fake.com", LanguageId = -1, Name = "Update" };
    public User UserForDelete { get; } = new() { Id = -3, Email = "delete_user@fake.com", LanguageId = -1, Name = "Delete" };

    public void Dispose() {
        _Connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
