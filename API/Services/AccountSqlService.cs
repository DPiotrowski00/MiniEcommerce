using System.Configuration.Internal;

namespace API.Services
{
    public interface IAccountSqlService
    {

    }
    public class AccountSqlService(IConfiguration configuration) : IAccountSqlService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Default")!;


    }
}
