using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace FeatureFlags.Client.Tests;

public class HttpFeatureFlagClientTests {
    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_ReturnsFeatureDefinitions() {
        // Arrange
        var customFeatures = new List<CustomFeatureDefinition>
        {
            new() { Name = "FeatureA" },
            new() { Name = "FeatureB" }
        };
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().EndsWith("features")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(customFeatures)
            });

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(Constants.HttpClientName)).Returns(httpClient);

        var loggerMock = new Mock<ILogger<HttpFeatureFlagClient>>();
        var client = new HttpFeatureFlagClient(httpClientFactoryMock.Object, loggerMock.Object);

        // Act
        var result = await client.GetAllFeatureDefinitionsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.Name == "FeatureA");
        Assert.Contains(result, f => f.Name == "FeatureB");
    }

    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_HandlesApiError_LogsAndReturnsEmpty() {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(Constants.HttpClientName)).Returns(httpClient);

        var loggerMock = new Mock<ILogger<HttpFeatureFlagClient>>();
        var client = new HttpFeatureFlagClient(httpClientFactoryMock.Object, loggerMock.Object);

        // Act
        var result = await client.GetAllFeatureDefinitionsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error fetching feature definitions")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetFeatureDefinitionByNameAsync_ReturnsFeatureDefinition() {
        // Arrange
        var customFeature = new CustomFeatureDefinition { Name = "FeatureX" };
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().EndsWith("feature/FeatureX")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(customFeature)
            });

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(Constants.HttpClientName)).Returns(httpClient);

        var loggerMock = new Mock<ILogger<HttpFeatureFlagClient>>();
        var client = new HttpFeatureFlagClient(httpClientFactoryMock.Object, loggerMock.Object);

        // Act
        var result = await client.GetFeatureDefinitionByNameAsync("FeatureX");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("FeatureX", result.Name);
    }

    [Fact]
    public async Task GetFeatureDefinitionByNameAsync_HandlesApiError_LogsAndReturnsNull() {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.NotFound
            });

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(Constants.HttpClientName)).Returns(httpClient);

        var loggerMock = new Mock<ILogger<HttpFeatureFlagClient>>();
        var client = new HttpFeatureFlagClient(httpClientFactoryMock.Object, loggerMock.Object);

        // Act
        var result = await client.GetFeatureDefinitionByNameAsync("MissingFeature");

        // Assert
        Assert.Null(result);
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error fetching feature definition")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ClearCache_ThrowsNotImplementedException() {
        // Arrange
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var loggerMock = new Mock<ILogger<HttpFeatureFlagClient>>();
        var client = new HttpFeatureFlagClient(httpClientFactoryMock.Object, loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(() => client.ClearCache());
    }
}
