using System.Security.Cryptography;
using System.Text;

namespace praktika1.Helpers
{
    public static class Md5Helper
    {
        public static string ComputeMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                    sb.Append(b.ToString("X2"));
                return sb.ToString();
            }
        }
    }
}