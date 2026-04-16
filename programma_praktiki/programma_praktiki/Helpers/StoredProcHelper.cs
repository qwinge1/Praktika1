using Npgsql;
using programma_praktiki.Models;
using Microsoft.Extensions.Configuration;

namespace programma_praktiki.Helpers
{
    public class StoredProcHelper
    {
        private readonly string _connectionString;

        public StoredProcHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public User? LoginStoredProc(string email, string password)
        {
            string hash = Md5Helper.ComputeMD5(password);
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT * FROM login_user(@email, @hash)", conn);
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

        public bool RegisterStoredProc(string email, string password)
        {
            string hash = Md5Helper.ComputeMD5(password);
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT register_user(@email, @hash)", conn);
            cmd.Parameters.AddWithValue("email", email);
            cmd.Parameters.AddWithValue("hash", hash);
            cmd.ExecuteNonQuery();
            return true;
        }
    }
}