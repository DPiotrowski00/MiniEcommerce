using System.Security.Cryptography;

namespace API.Helpers
{
    public static class PasswordGenerator
    {
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";
        private const string Special = "!@#$%^&*()-_=+[]{};:,.<>?";

        private static readonly string AllChars =
            Lowercase + Uppercase + Digits + Special;

        public static string GenerateNewPassword(int length = 12)
        {
            if (length < 8)
                throw new ArgumentException("Password length must be at least 8.");

            if (length > 64)
                throw new ArgumentException("Password length must not exceed 64.");

            List<char> chars =
            [
                GetRandomChar(Lowercase),
                GetRandomChar(Uppercase),
                GetRandomChar(Digits),
                GetRandomChar(Special)
            ];

            while (chars.Count < length)
            {
                chars.Add(GetRandomChar(AllChars));
            }

            Shuffle(chars);

            return new string([.. chars]);
        }

        private static char GetRandomChar(string source)
        {
            return source[RandomNumberGenerator.GetInt32(source.Length)];
        }

        private static void Shuffle(List<char> chars)
        {
            for (int i = chars.Count - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);

                (chars[i], chars[j]) = (chars[j], chars[i]);
            }
        }
    }
}