using System.Security.Cryptography;
using System.Text;

namespace API.Helpers
{
    public static class HashHelper
    {
        public static string ComputeSha256(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = SHA256.HashData(bytes);

            return Convert.ToHexString(hash);
        }
    }
}
