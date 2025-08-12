using FeatureFlags.Domain.Models;
using FeatureFlags.Extensions.Services;
using FeatureFlags.Utils;

namespace FeatureFlags.Web.Tests.Extensions.Services;

public class AuditLogServiceExtensionsTests {
    [Fact]
    public void SelectSingleAsModel_ReturnsProjectedModel() {
        // arrange
        var user = new User { Name = "name", Email = "a@b.com" };
        var auditLog = new AuditLog {
            Id = 1, BatchId = Guid.NewGuid(), Date = DateTime.MinValue, Entity = "test", State = Microsoft.EntityFrameworkCore.EntityState.Modified,
            PrimaryKey = 100, UserId = 200, OldValues = "old", NewValues = "new", User = user
        };
        var auditLogs = new List<AuditLog> { auditLog }.AsQueryable();

        // act
        var models = auditLogs.SelectAsModel().ToList();

        // assert
        Assert.NotNull(models);
        var singleModel = Assert.Single(models);

        Assert.Equal(auditLog.Id, singleModel.Id);
        Assert.Equal(auditLog.BatchId, singleModel.BatchId);
        Assert.Equal(auditLog.Date, singleModel.Date);
        Assert.Equal(auditLog.Entity, singleModel.Entity);
        Assert.Equal(auditLog.State, singleModel.State);
        Assert.Equal(auditLog.PrimaryKey, singleModel.PrimaryKey);
        Assert.Equal(auditLog.OldValues, singleModel.OldValues);
        Assert.Equal(auditLog.NewValues, singleModel.NewValues);
        Assert.Equal(auditLog.User.Name, singleModel.Name);
        Assert.Equal(auditLog.User.Email, singleModel.Email);
    }

    [Fact]
    public void SelectMultipleAsModel_ReturnsProjectedModels() {
        // arrange
        var auditLog1 = new AuditLog {
            Id = 2, BatchId = Guid.NewGuid(), Date = DateTime.MinValue, Entity = "test2", State = Microsoft.EntityFrameworkCore.EntityState.Added,
            PrimaryKey = 200, UserId = 200, OldValues = "old2", NewValues = "new2"
        };
        var auditLog2 = new AuditLog {
            Id = 3, BatchId = Guid.NewGuid(), Date = DateTime.MinValue, Entity = "test3", State = Microsoft.EntityFrameworkCore.EntityState.Deleted,
            PrimaryKey = 300, UserId = 300, OldValues = "old3", NewValues = "new3"
        };
        var auditLogs = new List<AuditLog> { auditLog1, auditLog2 }.AsQueryable();

        // act
        var models = auditLogs.SelectAsModel().ToList();

        // assert
        Assert.NotNull(models);
        Assert.Equal(2, models.Count);

        Assert.Collection(models,
            x => Assert.Equal(auditLog1.Id, x.Id),
            x => Assert.Equal(auditLog2.Id, x.Id)
        );
        Assert.Collection(models,
            x => Assert.Equal(auditLog1.BatchId, x.BatchId),
            x => Assert.Equal(auditLog2.BatchId, x.BatchId)
        );
        Assert.Collection(models,
            x => Assert.Equal(auditLog1.Date, x.Date),
            x => Assert.Equal(auditLog2.Date, x.Date)
        );
        Assert.Collection(models,
            x => Assert.Equal(auditLog1.Entity, x.Entity),
            x => Assert.Equal(auditLog2.Entity, x.Entity)
        );
        Assert.Collection(models,
            x => Assert.Equal(auditLog1.State, x.State),
            x => Assert.Equal(auditLog2.State, x.State)
        );
        Assert.Collection(models,
            x => Assert.Equal(auditLog1.PrimaryKey, x.PrimaryKey),
            x => Assert.Equal(auditLog2.PrimaryKey, x.PrimaryKey)
        );
        Assert.Collection(models,
            x => Assert.Equal(auditLog1.OldValues, x.OldValues),
            x => Assert.Equal(auditLog2.OldValues, x.OldValues)
        );
        Assert.Collection(models,
            x => Assert.Equal(auditLog1.NewValues, x.NewValues),
            x => Assert.Equal(auditLog2.NewValues, x.NewValues)
        );
        Assert.Collection(models,
            x => Assert.Equal("", x.Name),
            x => Assert.Equal("", x.Name)
        );
    }

    [Fact]
    public void SelectSingleAsSearchResultModel_ReturnsProjectedModel() {
        // arrange
        var user = new User { Name = "name", Email = "a@b.com" };
        var auditLog = new AuditLog {
            Id = 1, BatchId = Guid.NewGuid(), Date = DateTime.MinValue, Entity = "test", State = Microsoft.EntityFrameworkCore.EntityState.Modified,
            PrimaryKey = 100, UserId = 200, OldValues = "old", NewValues = "new", User = user
        };
        var auditLogs = new List<AuditLog> { auditLog }.AsQueryable();

        // act
        var models = auditLogs.SelectAsSearchResultModel().ToList();

        // assert
        Assert.NotNull(models);
        var singleModel = Assert.Single(models);

        Assert.Equal(auditLog.Id, singleModel.Id);
        Assert.Equal(auditLog.BatchId, singleModel.BatchId);
        Assert.Equal(auditLog.Date, singleModel.Date);
        Assert.Equal(auditLog.Entity, singleModel.Entity);
        Assert.Equal(auditLog.State.ToString(), singleModel.State);
        Assert.Equal(NameHelper.DisplayName(user.Name, user.Email), singleModel.Name);
    }

    [Fact]
    public void SelectMultipleAsSearchResultsModel_ReturnsProjectedModels() {
        // arrange
        var user = new User { Name = "name" };
        var auditLog1 = new AuditLog {
            Id = 2, BatchId = Guid.NewGuid(), Date = DateTime.MinValue, Entity = "test2", State = Microsoft.EntityFrameworkCore.EntityState.Added,
            PrimaryKey = 200, UserId = 200, OldValues = "old2", NewValues = "new2", User = user
        };
        var auditLog2 = new AuditLog {
            Id = 3, BatchId = Guid.NewGuid(), Date = DateTime.MinValue, Entity = "test3", State = Microsoft.EntityFrameworkCore.EntityState.Deleted,
            PrimaryKey = 300, UserId = 300, OldValues = "old3", NewValues = "new3", User = user
        };
        var auditLogs = new List<AuditLog> { auditLog1, auditLog2 }.AsQueryable();

        // act
        var models = auditLogs.SelectAsSearchResultModel().ToList();

        // assert
        Assert.NotNull(models);
        Assert.Equal(2, models.Count);

        Assert.Collection(models,
            x => Assert.Equal(auditLog1.Id, x.Id),
            x => Assert.Equal(auditLog2.Id, x.Id)
        );
        Assert.Collection(models,
            x => Assert.Equal(auditLog1.BatchId, x.BatchId),
            x => Assert.Equal(auditLog2.BatchId, x.BatchId)
        );
        Assert.Collection(models,
            x => Assert.Equal(auditLog1.Date, x.Date),
            x => Assert.Equal(auditLog2.Date, x.Date)
        );
        Assert.Collection(models,
            x => Assert.Equal(auditLog1.Entity, x.Entity),
            x => Assert.Equal(auditLog2.Entity, x.Entity)
        );
        Assert.Collection(models,
            x => Assert.Equal(auditLog1.State.ToString(), x.State),
            x => Assert.Equal(auditLog2.State.ToString(), x.State)
        );
        Assert.Collection(models,
            x => Assert.Equal(NameHelper.DisplayName(user.Name, user.Email), x.Name),
            x => Assert.Equal(NameHelper.DisplayName(user.Name, user.Email), x.Name)
        );
    }
}
