using System;
using System.Configuration;
using Npgsql;
using praktika1.Models;

namespace praktika1.Helpers
{
    public static class DatabaseHelper
    {
        private static string connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        // Прямой SQL для входа
        public static User LoginSql(string email, string password)
        {
            string hash = Md5Helper.ComputeMD5(password);
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                string sql = @"SELECT id, email, роль_id FROM Пользователь WHERE email = @email AND хеш_пароля = @hash";
                using (var cmd = new NpgsqlCommand(sql, conn))
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

        // Прямой SQL для регистрации
        public static bool RegisterSql(string email, string password)
        {
            string hash = Md5Helper.ComputeMD5(password);
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                string sql = @"INSERT INTO Пользователь (email, хеш_пароля, роль_id) 
                               VALUES (@email, @hash, (SELECT id FROM Роль WHERE название = 'Гость'))";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("email", email);
                    cmd.Parameters.AddWithValue("hash", hash);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}