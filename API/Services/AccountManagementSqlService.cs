using API.Helpers;
using Dapper;
using MySql.Data.MySqlClient;

namespace API.Services
{
    public interface IAccountManagementSqlService
    {
        Task ChangePassword(string Username, string Password);
        Task UpdateAccountInformation(AccountInformation info);
    }

    public class AccountManagementSqlService (IConfiguration configuration) : IAccountManagementSqlService
    {
        string _connectionString = configuration.GetConnectionString("Default")!;

        private MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public async Task ChangePassword(string Username, string Password)
        {
            string query = """UPDATE logindata SET Password = @Password WHERE BINARY Username = @Username""";

            MySqlConnection connection = CreateConnection();
            try
            {
                await connection.ExecuteAsync(query, new { Password, Username });
            }
            catch
            {
                return;
            }
        }

        public async Task UpdateAccountInformation(AccountInformation info)
        {
            string query = """
                           UPDATE 
                           """;

            MySqlConnection connection = CreateConnection();
            try
            {
                return;
            }
            catch
            {
                return;
            }
        }
    }
}
