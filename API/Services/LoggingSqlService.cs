using System.Data;
using System.Configuration;
using System.Data.Common;
using MySql.Data.MySqlClient;
using Dapper;
using Org.BouncyCastle.Crypto.Generators;
using BCrypt.Net;
using Org.BouncyCastle.Asn1.Mozilla;

namespace API.Services
{
    public class LogInInfo()
    {
        public int ID { get; set; }
        public string? Password { get; set; }
    }
    
    public interface ILoggingSqlService
    {
        Task<int> ValidateLogIn(string Username, string Password);
        Task CreateUser(string Username, string Password);

    }

    public class LoggingSqlService(IConfiguration configuration) : ILoggingSqlService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Default")!;

        private MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public async Task<int> ValidateLogIn(string Username, string Password)
        {
            string query = """
                           SELECT ID, Password FROM logindata WHERE BINARY Username = @Username
                           """;

            var connection = CreateConnection();
            try
            {
                var data = await connection.QuerySingleAsync<LogInInfo>(query, new { Username });
                if(BCrypt.Net.BCrypt.Verify(Password, data.Password))
                {
                    return data.ID;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public async Task CreateUser(string Username, string Password)
        {
            string query = """
                           INSERT INTO logindata (Username, Password) VALUES (@Username, @HashedPassword)
                           """;

            var connection = CreateConnection();
            try
            {
                var HashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);
                await connection.ExecuteAsync(query, new { Username, HashedPassword });
            }
            catch
            {
                return;
            }
        }
    }
}
