using MySql.Data.MySqlClient;

namespace API.Services
{
    public class CreateSqlConnection
    {
        public static MySqlConnection CreateConnection(string ConnectionString)
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}
