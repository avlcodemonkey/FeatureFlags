using FeatureFlags.Models;
using FeatureFlags.Services;
using FeatureFlags.Web.Tests.Fixtures;

namespace FeatureFlags.Web.Tests.Services;

[Collection(nameof(DatabaseCollection))]
public class AuditLogServiceTests(DatabaseFixture fixture) {
    private readonly DatabaseFixture _Fixture = fixture;
    private readonly AuditLogService _AuditLogService = new(fixture.CreateContext());

    [Fact]
    public async Task GetLogByIdAsync_ReturnsFakeAuditLogModel() {
        // arrange
        var fakeLog = await _Fixture.CreateContext().AuditLog.FindAsync(1L)!;

        // act
        var log = await _AuditLogService.GetLogByIdAsync(1);

        // assert
        Assert.NotNull(fakeLog);
        Assert.NotNull(log);
        Assert.IsType<AuditLogModel>(log);
        Assert.Equal(fakeLog.BatchId, log.BatchId);
        Assert.Equal(fakeLog.Date, log.Date);
        Assert.Equal(fakeLog.Entity, log.Entity);
        Assert.Equal(fakeLog.State, log.State);
        Assert.Equal(fakeLog.PrimaryKey, log.PrimaryKey);
        Assert.Equal(fakeLog.OldValues, log.OldValues);
        Assert.Equal(fakeLog.NewValues, log.NewValues);
    }

    [Fact]
    public async Task GetLogByIdAsync_WithInvalidId_ReturnsNull() {
        // arrange
        var logIdToGet = -200;

        // act
        var log = await _AuditLogService.GetLogByIdAsync(logIdToGet);

        // assert
        Assert.Null(log);
    }

    [Fact]
    public async Task SearchLogsAsync_WithNoCriteria_ReturnsTwoLogModels() {
        // arrange
        using var context = _Fixture.CreateContext();
        var fakeLog1 = await context.AuditLog.FindAsync(1L);
        var fakeLog2 = await context.AuditLog.FindAsync(2L);
        var search = new AuditLogSearchModel { StartDate = DateOnly.FromDateTime(DateTime.MinValue), EndDate = DateOnly.FromDateTime(DateTime.MinValue) };

        // act
        var logs = await _AuditLogService.SearchLogsAsync(search);

        // assert
        Assert.NotNull(fakeLog1);
        Assert.NotNull(fakeLog2);
        Assert.NotEmpty(logs);
        Assert.IsType<IEnumerable<AuditLogSearchResultModel>>(logs, exactMatch: false);
        Assert.Equal(2, logs.Count());
        Assert.Collection(logs,
            x => Assert.Equal(fakeLog1.Id, x.Id),
            x => Assert.Equal(fakeLog2.Id, x.Id)
        );
    }

    [Fact]
    public async Task SearchLogsAsync_WithFutureStartDate_ReturnsNoModels() {
        // arrange
        // fixture creates two auditlogs with date=DateTime.MinValue, so searching for records in the future should find none
        var search = new AuditLogSearchModel { StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)), EndDate = DateOnly.FromDateTime(DateTime.MaxValue) };

        // act
        var logs = await _AuditLogService.SearchLogsAsync(search);

        // assert
        Assert.Empty(logs);
    }

    [Fact]
    public async Task SearchLogsAsync_WithPastStartDate_ReturnsNoModels() {
        // arrange
        // fixture creates two auditlogs with date=DateTime.MinValue, so searching for records after that should find none
        var search = new AuditLogSearchModel { StartDate = DateOnly.FromDateTime(DateTime.MinValue.AddDays(1)), EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)) };

        // act
        var logs = await _AuditLogService.SearchLogsAsync(search);

        // assert
        Assert.Empty(logs);
    }

    [Fact]
    public async Task SearchLogsAsync_WithValidStartDateNoEndDate_ReturnsTwoLogModels() {
        // arrange
        using var context = _Fixture.CreateContext();
        var fakeLog1 = await context.AuditLog.FindAsync(1L);
        var fakeLog2 = await context.AuditLog.FindAsync(2L);

        // fixture creates two auditlogs with date=DateTime.MinValue
        // this test is iffy - because other tests use the same database fixture there could be other audit log records. se can't really use endDate=null
        var search = new AuditLogSearchModel { StartDate = DateOnly.FromDateTime(DateTime.MinValue), EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)) };

        // act
        var logs = await _AuditLogService.SearchLogsAsync(search);

        // assert
        Assert.NotNull(fakeLog1);
        Assert.NotNull(fakeLog2);
        Assert.NotEmpty(logs);
        Assert.IsType<IEnumerable<AuditLogSearchResultModel>>(logs, exactMatch: false);
        Assert.Equal(2, logs.Count());
        Assert.Collection(logs,
            x => Assert.Equal(fakeLog1.Id, x.Id),
            x => Assert.Equal(fakeLog2.Id, x.Id)
        );
    }

    [Fact]
    public async Task SearchLogsAsync_WithValidEndDateNoStartDate_ReturnsTwoLogModels() {
        // arrange
        using var context = _Fixture.CreateContext();
        var fakeLog1 = await context.AuditLog.FindAsync(1L);
        var fakeLog2 = await context.AuditLog.FindAsync(2L);
        // fixture creates two auditlogs with date=DateTime.Now
        var search = new AuditLogSearchModel { StartDate = null, EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)) };

        // act
        var logs = await _AuditLogService.SearchLogsAsync(search);

        // assert
        Assert.NotNull(fakeLog1);
        Assert.NotNull(fakeLog2);
        Assert.NotEmpty(logs);
        Assert.IsType<IEnumerable<AuditLogSearchResultModel>>(logs, exactMatch: false);
        Assert.Equal(2, logs.Count());
        Assert.Collection(logs,
            x => Assert.Equal(fakeLog1.Id, x.Id),
            x => Assert.Equal(fakeLog2.Id, x.Id)
        );
    }

    [Fact]
    public async Task SearchLogsAsync_WithValidBatch_ReturnsOneLogModel() {
        // arrange
        var fakeLog1 = await _Fixture.CreateContext().AuditLog.FindAsync(1L);
        var search = new AuditLogSearchModel { StartDate = null, EndDate = null, BatchId = fakeLog1!.BatchId };

        // act
        var logs = await _AuditLogService.SearchLogsAsync(search);

        // assert
        Assert.NotNull(fakeLog1);
        Assert.NotEmpty(logs);
        Assert.IsType<IEnumerable<AuditLogSearchResultModel>>(logs, exactMatch: false);
        var singleResult = Assert.Single(logs);
        Assert.Equal(fakeLog1.Id, singleResult.Id);
    }

    [Fact]
    public async Task SearchLogsAsync_WithNoValidBatch_ReturnsNoModel() {
        // arrange
        var search = new AuditLogSearchModel { StartDate = null, EndDate = null, BatchId = Guid.NewGuid() };

        // act
        var logs = await _AuditLogService.SearchLogsAsync(search);

        // assert
        Assert.Empty(logs);
    }

    [Fact]
    public async Task SearchLogsAsync_WithValidEntity_ReturnsOneLogModel() {
        // arrange
        var fakeLog1 = await _Fixture.CreateContext().AuditLog.FindAsync(1L);
        var search = new AuditLogSearchModel { StartDate = null, EndDate = null, Entity = fakeLog1!.Entity };

        // act
        var logs = await _AuditLogService.SearchLogsAsync(search);

        // assert
        Assert.NotNull(fakeLog1);
        Assert.NotEmpty(logs);
        Assert.IsType<IEnumerable<AuditLogSearchResultModel>>(logs, exactMatch: false);
        var singleResult = Assert.Single(logs);
        Assert.Equal(fakeLog1.Id, singleResult.Id);
    }

    [Fact]
    public async Task SearchLogsAsync_WithNoValidEntity_ReturnsNoModel() {
        // arrange
        var search = new AuditLogSearchModel { StartDate = null, EndDate = null, Entity = "gibberish" };

        // act
        var logs = await _AuditLogService.SearchLogsAsync(search);

        // assert
        Assert.Empty(logs);
    }

    [Fact]
    public async Task SearchLogsAsync_WithValidPrimaryKey_ReturnsOneLogModel() {
        // arrange
        var fakeLog1 = await _Fixture.CreateContext().AuditLog.FindAsync(1L);
        var search = new AuditLogSearchModel { StartDate = null, EndDate = null, PrimaryKey = fakeLog1!.PrimaryKey };

        // act
        var logs = await _AuditLogService.SearchLogsAsync(search);

        // assert
        Assert.NotNull(fakeLog1);
        Assert.NotEmpty(logs);
        Assert.IsType<IEnumerable<AuditLogSearchResultModel>>(logs, exactMatch: false);
        Assert.NotEmpty(logs);
    }

    [Fact]
    public async Task SearchLogsAsync_WithNoValidPrimaryKey_ReturnsNoModel() {
        // arrange
        var search = new AuditLogSearchModel { StartDate = null, EndDate = null, PrimaryKey = 999 };

        // act
        var logs = await _AuditLogService.SearchLogsAsync(search);

        // assert
        Assert.Empty(logs);
    }

    [Fact]
    public async Task SearchLogsAsync_WithModifiedState_ReturnsOneLogModel() {
        // arrange
        var fakeLog1 = await _Fixture.CreateContext().AuditLog.FindAsync(1L);
        var search = new AuditLogSearchModel { StartDate = null, EndDate = DateOnly.FromDateTime(DateTime.MinValue), State = fakeLog1!.State };

        // act
        var logs = await _AuditLogService.SearchLogsAsync(search);

        // assert
        Assert.NotNull(fakeLog1);
        Assert.NotEmpty(logs);
        Assert.IsType<IEnumerable<AuditLogSearchResultModel>>(logs, exactMatch: false);
        var singleResult = Assert.Single(logs);
        Assert.Equal(fakeLog1.Id, singleResult.Id);
    }

    [Fact]
    public async Task SearchLogsAsync_WithDeletedState_ReturnsOneLogModel() {
        // arrange
        var fakeLog2 = await _Fixture.CreateContext().AuditLog.FindAsync(2L);
        var search = new AuditLogSearchModel { StartDate = null, EndDate = DateOnly.FromDateTime(DateTime.MinValue), State = fakeLog2!.State };

        // act
        var logs = await _AuditLogService.SearchLogsAsync(search);

        // assert
        Assert.NotNull(fakeLog2);
        Assert.NotEmpty(logs);
        Assert.IsType<IEnumerable<AuditLogSearchResultModel>>(logs, exactMatch: false);
        var singleResult = Assert.Single(logs);
        Assert.Equal(fakeLog2.Id, singleResult.Id);
    }

    [Fact]
    public async Task SearchLogsAsync_WithNoValidState_ReturnsNoModel() {
        // arrange
        var search = new AuditLogSearchModel { StartDate = null, EndDate = null, State = Microsoft.EntityFrameworkCore.EntityState.Unchanged };

        // act
        var logs = await _AuditLogService.SearchLogsAsync(search);

        // assert
        Assert.Empty(logs);
    }

    [Fact]
    public async Task SearchLogsAsync_WithValidUser_ReturnsOneLogModel() {
        // arrange
        var user1 = _Fixture.User;
        var fakeLog1 = await _Fixture.CreateContext().AuditLog.FindAsync(1L);
        var search = new AuditLogSearchModel { StartDate = null, EndDate = DateOnly.FromDateTime(DateTime.MinValue), UserId = user1.Id };

        // act
        var logs = await _AuditLogService.SearchLogsAsync(search);

        // assert
        Assert.NotNull(fakeLog1);
        Assert.NotEmpty(logs);
        Assert.IsType<IEnumerable<AuditLogSearchResultModel>>(logs, exactMatch: false);
        var singleResult = Assert.Single(logs);
        Assert.Equal(fakeLog1.Id, singleResult.Id);
    }

    [Fact]
    public async Task SearchLogsAsync_WithNoValidUser_ReturnsNoModel() {
        // arrange
        var search = new AuditLogSearchModel { StartDate = null, EndDate = null, UserId = 999 };

        // act
        var logs = await _AuditLogService.SearchLogsAsync(search);

        // assert
        Assert.Empty(logs);
    }
}
