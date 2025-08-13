using FeatureFlags.Utils;

namespace FeatureFlags.Web.Tests.Utils;

public class KeyGeneratorTests {
    internal static readonly char[] _Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    [Fact]
    public void GenerateToken_ReturnsStringOfCorrectLength_AndAllowedChars() {
        // Arrange
        var size = 24;

        // Act
        var token = KeyGenerator.GenerateToken(size);

        // Assert
        Assert.NotNull(token);
        Assert.Equal(size, token.Length);
        Assert.All(token, c => Assert.Contains(c, _Chars));
    }

    [Fact]
    public void GenerateToken_ReturnsDifferentTokens() {
        // Act
        var token1 = KeyGenerator.GenerateToken(16);
        var token2 = KeyGenerator.GenerateToken(16);

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateKey_ReturnsStringWithPrefix_AndCorrectLength() {
        // Act
        var apiKey = KeyGenerator.GenerateKey();

        // Assert
        Assert.NotNull(apiKey);
        Assert.StartsWith("FF-", apiKey);
        Assert.Equal(32, apiKey.Length);
        // Should be base64-url-safe after prefix
        var keyPart = apiKey[3..];
        Assert.Matches("^[A-Za-z0-9_-]+$", keyPart);
    }

    [Fact]
    public void GenerateKey_ReturnsUniqueKeys() {
        // Act
        var key1 = KeyGenerator.GenerateKey();
        var key2 = KeyGenerator.GenerateKey();

        // Assert
        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void GetSha512Hash_ReturnsExpectedHash() {
        // Arrange
        var input = "test";
        // Precomputed SHA-512 hash for "test"
        var expected = "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff";

        // Act
        var hash = KeyGenerator.GetSha512Hash(input);

        // Assert
        Assert.Equal(expected, hash);
        Assert.Equal(128, hash.Length);
        Assert.Matches("^[a-f0-9]+$", hash);
    }

    [Fact]
    public void GetSha512Hash_DifferentInputs_ProduceDifferentHashes() {
        // Act
        var hash1 = KeyGenerator.GetSha512Hash("input1");
        var hash2 = KeyGenerator.GetSha512Hash("input2");

        // Assert
        Assert.NotEqual(hash1, hash2);
    }
}
