using API.Helpers;
using Dapper;
using MySql.Data.MySqlClient;

namespace API.Services
{
    public interface IAccountManagementSqlService
    {
        Task ChangePassword(string Username, string Password);
    }

    public class AccountManagementSqlService (IConfiguration configuration) : IAccountManagementSqlService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Default")!;

        public async Task ChangePassword(string Username, string Password)
        {
            string query = """UPDATE logindata SET Password = @Password WHERE BINARY Username = @Username""";

            MySqlConnection connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new { Password, Username });
            }
            catch
            {
                return;
            }
        }
    }
}
