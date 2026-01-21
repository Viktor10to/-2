using System;
using System.Security.Cryptography;
using System.Text;

namespace Flexi2.Core.Security
{
    public static class PinHasher
    {
        public static string NewSaltHex(int bytes = 16)
        {
            var salt = RandomNumberGenerator.GetBytes(bytes);
            return Convert.ToHexString(salt);
        }

        public static string Hash(string pin, string saltHex)
        {
            pin ??= "";
            saltHex ??= "";

            var saltBytes = Convert.FromHexString(saltHex);
            var pinBytes = Encoding.UTF8.GetBytes(pin);

            var all = new byte[saltBytes.Length + pinBytes.Length];
            Buffer.BlockCopy(saltBytes, 0, all, 0, saltBytes.Length);
            Buffer.BlockCopy(pinBytes, 0, all, saltBytes.Length, pinBytes.Length);

            var hash = SHA256.HashData(all);
            return Convert.ToHexString(hash);
        }

        public static bool Verify(string pin, string saltHex, string expectedHashHex)
        {
            var h = Hash(pin, saltHex);
            return string.Equals(h, expectedHashHex, StringComparison.OrdinalIgnoreCase);
        }
    }
}
