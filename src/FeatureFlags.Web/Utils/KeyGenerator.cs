using System.Security.Cryptography;
using System.Text;

namespace FeatureFlags.Utils;

/// <summary>
/// Provides methods for generating unique keys.
/// </summary>
public static class KeyGenerator {
    internal static readonly char[] _Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    private const string _Prefix = "FF-";
    private const int _NumberOfSecureBytesToGenerate = 32;
    private const int _LengthOfKey = 32;

    /// <summary>
    /// Generates a unique token of the specified size using cryptographic random numbers.
    /// </summary>
    /// <remarks>The method uses a cryptographic random number generator to ensure the uniqueness and
    /// unpredictability of the key. The generated key consists of characters from a predefined set.</remarks>
    /// <param name="size">The length of the key to generate. Must be a positive integer.</param>
    /// <returns>A string representing the unique key composed of randomly selected characters.</returns>
    public static string GetUniqueToken(int size) {
        var data = new byte[4 * size];
        using (var crypto = RandomNumberGenerator.Create()) {
            crypto.GetBytes(data);
        }
        var result = new StringBuilder(size);
        for (var i = 0; i < size; i++) {
            var rnd = BitConverter.ToUInt32(data, i * 4);
            var idx = rnd % _Chars.Length;

            result.Append(_Chars[idx]);
        }

        return result.ToString();
    }

    /// <summary>
    /// Generates an API key using cryptographic random numbers.
    /// </summary>
    /// <remarks>The method uses a cryptographic random number generator to ensure the uniqueness and
    /// unpredictability of the key. The generated key consists of characters from a predefined set.</remarks>
    /// <returns>A string representing the unique key composed of randomly selected characters.</returns>
    public static string GetApiKey() {
        var bytes = RandomNumberGenerator.GetBytes(_NumberOfSecureBytesToGenerate);
        var base64String = Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_");
        var keyLength = _LengthOfKey - _Prefix.Length;
        return _Prefix + base64String[..keyLength];
    }

    /// <summary>
    /// Computes the SHA-512 hash of the specified input string.
    /// </summary>
    /// <param name="input">The input string to compute the hash for. Cannot be <see langword="null"/>.</param>
    /// <returns>A string representation of the SHA-512 hash in hexadecimal format.</returns>
    public static string GetSha512Hash(string input) {
        var bytes = SHA512.HashData(Encoding.UTF8.GetBytes(input));
        var builder = new StringBuilder();
        for (var i = 0; i < bytes.Length; i++) {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }
}
