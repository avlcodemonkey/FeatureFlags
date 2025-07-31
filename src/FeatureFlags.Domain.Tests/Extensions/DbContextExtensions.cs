using System.ComponentModel.DataAnnotations;
using FeatureFlags.Domain.Attributes;
using FeatureFlags.Domain.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Domain.Tests.Extensions;

/// <summary>
/// Provides unit tests for extension methods on <see cref="DbContext"/> entries.
/// </summary>
public class DbContextExtensionsTests {
    private class TestEntity {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = "";

        [NoAudit]
        public string Secret { get; set; } = "";
    }

    private class TestDbContext : DbContext {
        public DbSet<TestEntity> Entities { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    }

    [Fact]
    public void GetPrimaryKey_ReturnsPrimaryKeyValue() {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("GetPrimaryKeyTest")
            .Options;

        using var context = new TestDbContext(options);
        var entity = new TestEntity { Id = 42, Name = "Test" };
        context.Entities.Add(entity);
        context.SaveChanges();

        var entry = context.Entry(entity);
        var pk = entry.GetPrimaryKey();

        Assert.Equal(42, pk);
    }

    [Fact]
    public void ToAuditJson_ExcludesNoAuditProperties() {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("ToAuditJsonTest")
            .Options;

        using var context = new TestDbContext(options);
        var entity = new TestEntity { Id = 1, Name = "TestName", Secret = "Hidden" };
        context.Entities.Add(entity);
        context.SaveChanges();

        var entry = context.Entry(entity);
        var json = entry.Properties.ToAuditJson();

        Assert.Contains("\"Name\":\"TestName\"", json);
        Assert.DoesNotContain("Secret", json);
    }

    [Fact]
    public void ToAuditJson_UsesOriginalValues_WhenSpecified() {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("ToAuditJsonOriginalTest")
            .Options;

        using var context = new TestDbContext(options);
        var entity = new TestEntity { Id = 2, Name = "Original", Secret = "Hidden" };
        context.Entities.Add(entity);
        context.SaveChanges();

        entity.Name = "Changed";
        var entry = context.Entry(entity);

        var json = entry.Properties.ToAuditJson(currentValues: false);

        Assert.Contains("\"Name\":\"Original\"", json);
        Assert.DoesNotContain("Secret", json);
    }
}
