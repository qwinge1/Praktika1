using System;
using Npgsql;
using programma_praktiki.Models;
using Microsoft.Extensions.Configuration;

namespace programma_praktiki.Helpers
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // Публичное свойство для проверки подключения
        public string ConnectionString => _connectionString;

        public bool TestConnection()
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public User? LoginSql(string email, string password)
        {
            string hash = Md5Helper.ComputeMD5(password);
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            string sql = @"SELECT id, email, роль_id FROM ""Пользователь"" WHERE email = @email AND хеш_пароля = @hash";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("email", email);
            cmd.Parameters.AddWithValue("hash", hash);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    RoleId = reader.GetInt32(2)
                };
            }
            return null;
        }

        public bool RegisterSql(string email, string password)
        {
            string hash = Md5Helper.ComputeMD5(password);
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            string sql = @"
                INSERT INTO ""Пользователь"" (email, хеш_пароля, роль_id) 
                VALUES (@email, @hash, (SELECT id FROM ""Роль"" WHERE название = 'Гость'))";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("email", email);
            cmd.Parameters.AddWithValue("hash", hash);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}