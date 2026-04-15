using Npgsql;
using praktika1.Models;
using System.Configuration;

namespace praktika1.Helpers
{
    public static class StoredProcHelper
    {
        private static string connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public static User LoginStoredProc(string email, string password)
        {
            string hash = Md5Helper.ComputeMD5(password);
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM login_user(@email, @hash)", conn))
                {
                    cmd.Parameters.AddWithValue("email", email);
                    cmd.Parameters.AddWithValue("hash", hash);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return new User { Id = reader.GetInt32(0), Email = reader.GetString(1), RoleId = reader.GetInt32(2) };
                    }
                }
            }
            return null;
        }

        public static bool RegisterStoredProc(string email, string password)
        {
            string hash = Md5Helper.ComputeMD5(password);
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT register_user(@email, @hash)", conn))
                {
                    cmd.Parameters.AddWithValue("email", email);
                    cmd.Parameters.AddWithValue("hash", hash);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }
    }
}