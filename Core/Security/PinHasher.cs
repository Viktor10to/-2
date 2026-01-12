using System;
using System.Security.Cryptography;
using System.Text;

namespace Flexi2.Core.Security
{
    public static class PinHasher
    {
        public static string NewSalt()
        {
            var bytes = new byte[16];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToHexString(bytes);
        }

        public static string Hash(string pin, string saltHex)
        {
            var input = $"{saltHex}:{pin}";
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }

        public static bool Verify(string pin, string saltHex, string expectedHashHex)
            => string.Equals(Hash(pin, saltHex), expectedHashHex, StringComparison.OrdinalIgnoreCase);
    }
}
