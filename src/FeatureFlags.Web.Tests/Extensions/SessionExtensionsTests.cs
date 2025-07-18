using System.Text;
using FeatureFlags.Extensions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FeatureFlags.Web.Tests.Extensions;

public class SessionExtensionsTests {
    private readonly Mock<ISession> _MockSession = new();

    [Theory]
    [InlineData("key", "", false)]
    [InlineData("key", "false", false)]
    [InlineData("key", "False", false)]
    [InlineData("key", "true", true)]
    [InlineData("key", "True", true)]
    public void IsEnabled_ReturnsExpectedResult(string key, string value, bool expected) {
        // arrange
        var val = Encoding.UTF8.GetBytes(value);
        _MockSession.Setup(x => x.TryGetValue(key, out val)).Returns(true);

        // act
        var result = _MockSession.Object.IsEnabled(key);

        // assert
        Assert.Equal(expected, result);
    }
}
