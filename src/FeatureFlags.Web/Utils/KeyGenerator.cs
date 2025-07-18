using System.Security.Cryptography;
using System.Text;

namespace FeatureFlags.Utils;

public static class KeyGenerator {
    internal static readonly char[] _Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    public static string GetUniqueKey(int size) {
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
}
