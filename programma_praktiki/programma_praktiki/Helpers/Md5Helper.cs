using System.Security.Cryptography;
using System.Text;

namespace programma_praktiki.Helpers
{
    public static class Md5Helper
    {
        public static string ComputeMD5(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = MD5.HashData(inputBytes);
            return Convert.ToHexString(hashBytes);
        }
    }
}