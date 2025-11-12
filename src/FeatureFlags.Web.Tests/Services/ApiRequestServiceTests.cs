using FeatureFlags.Domain.Models;
using FeatureFlags.Services;
using FeatureFlags.Web.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Web.Tests.Services;

[Collection(nameof(DatabaseCollection))]
public class ApiRequestServiceTests {
    private readonly DatabaseFixture _fixture;
    private readonly ApiRequestService _apiRequestService;

    public ApiRequestServiceTests(DatabaseFixture fixture) {
        _fixture = fixture;
        _apiRequestService = CreateService();
    }

    private ApiRequestService CreateService() => new(_fixture.CreateContext());

    [Fact]
    public async Task GetApiRequestsAsync_ReturnsRequests_WhenCriteriaMatch() {
        // Arrange
        var context = _fixture.CreateContext();
        var apiKey = _fixture.TestApiKey;
        var userId = apiKey.UserId;
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow.AddDays(7);
        context.ApiRequests.Add(new ApiRequest { ApiKeyId = apiKey.Id, IpAddress = "127.0.0.1", RequestedDate = DateTime.UtcNow });
        context.ApiRequests.Add(new ApiRequest { ApiKeyId = apiKey.Id, IpAddress = "192.168.1.1", RequestedDate = DateTime.UtcNow });
        await context.SaveChangesAsync();

        // Act
        var result = await _apiRequestService.GetApiRequestsAsync(userId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count() >= 2);
        Assert.All(result, r => Assert.NotNull(r.IpAddress));
    }

    [Fact]
    public async Task SaveApiRequestAsync_ReturnsTrue_WhenRequestIsSaved() {
        // Arrange
        var apiKeyId = _fixture.TestApiKey.Id;
        var ipAddress = "127.0.0.1";

        // Act
        var result = await _apiRequestService.SaveApiRequestAsync(apiKeyId, ipAddress);

        // Assert
        Assert.True(result);

        // Confirm in DB
        using var context = _fixture.CreateContext();
        var savedRequest = await context.ApiRequests.FirstOrDefaultAsync(r => r.IpAddress == ipAddress);
        Assert.NotNull(savedRequest);
        Assert.Equal(apiKeyId, savedRequest.ApiKeyId);
    }

    [Fact]
    public async Task SaveApiRequestAsync_ReturnsFalse_WhenSaveFails() {
        // Arrange
        var apiKeyId = -9999; // Invalid API key ID
        var ipAddress = "127.0.0.1";

        // Act + Assert
        await Assert.ThrowsAsync<DbUpdateException>(async () => await _apiRequestService.SaveApiRequestAsync(apiKeyId, ipAddress));
    }

    [Fact]
    public async Task GetApiRequestsAsync_ReturnsEmpty_WhenNoRequestsMatch() {
        // Arrange
        var userId = _fixture.TestApiKey.UserId;
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow.AddDays(-7); // No overlap with request dates

        // Act
        var result = await _apiRequestService.GetApiRequestsAsync(userId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetApiRequestsAsync_AllowsNullUserId() {
        // Arrange
        var context = _fixture.CreateContext();
        await context.SaveChangesAsync();

        // Act
        var result = await _apiRequestService.GetApiRequestsAsync(null, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(7));

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetApiRequestsAsync_AllowsNullStartDate() {
        // Arrange
        var context = _fixture.CreateContext();
        var apiKey = _fixture.TestApiKey;
        context.ApiRequests.Add(new ApiRequest { ApiKeyId = apiKey.Id, IpAddress = "127.0.0.1", RequestedDate = DateTime.UtcNow });
        await context.SaveChangesAsync();

        // Act
        var result = await _apiRequestService.GetApiRequestsAsync(apiKey.UserId, null, DateTime.UtcNow.AddDays(7));

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetApiRequestsAsync_AllowsNullEndDate() {
        // Arrange
        var context = _fixture.CreateContext();
        var apiKey = _fixture.TestApiKey;
        context.ApiRequests.Add(new ApiRequest { ApiKeyId = apiKey.Id, IpAddress = "127.0.0.1", RequestedDate = DateTime.UtcNow });
        await context.SaveChangesAsync();

        // Act
        var result = await _apiRequestService.GetApiRequestsAsync(apiKey.UserId, DateTime.UtcNow.AddDays(-7), null);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetApiRequestsAsync_AllowsAllNullParameters() {
        // Arrange
        var context = _fixture.CreateContext();
        var apiKey = _fixture.TestApiKey;
        context.ApiRequests.Add(new ApiRequest { ApiKeyId = apiKey.Id, IpAddress = "127.0.0.1", RequestedDate = DateTime.UtcNow });
        await context.SaveChangesAsync();

        // Act
        var result = await _apiRequestService.GetApiRequestsAsync(null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}
