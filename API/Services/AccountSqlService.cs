using API.DataModels;
using Dapper;
using System.Configuration.Internal;

namespace API.Services
{
    public interface IAccountSqlService
    {
        Task<AddressModel> GetAddress(int UserID);
        Task UpdateAddress(int UserID, AddressModel Address);
        Task DeleteAddress(int UserID);
    }
    public class AccountSqlService(IConfiguration configuration) : IAccountSqlService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Default")!;

        public async Task<AddressModel> GetAddress(int UserID)
        {
            string query = """
                           SELECT * FROM addresses WHERE UserID = @UserID
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                return await connection.QuerySingleAsync<AddressModel>(query, new { UserID });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new();
            }
        }

        public async Task DeleteAddress(int UserID)
        {
            string query = """
                           DELETE FROM addresses WHERE UserID = @UserID
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new { UserID });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
        }

        public async Task UpdateAddress(int UserID, AddressModel Address)
        {
            string query = """
                           INSERT INTO addresses (
                               Country, PostalCode, City, Street, BuildingNumber, ApartmentNumber, UserID
                           )
                           VALUES (
                               @Country, @PostalCode, @City, @Street, @BuildingNumber, @ApartmentNumber, @UserID
                           )
                           ON DUPLICATE KEY UPDATE
                               Country = VALUES(Country),
                               PostalCode = VALUES(PostalCode),
                               City = VALUES(City),
                               Street = VALUES(Street),
                               BuildingNumber = VALUES(BuildingNumber),
                               ApartmentNumber = VALUES(ApartmentNumber);
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new { UserID, Address.Country, Address.PostalCode, Address.City, Address.Street, Address.BuildingNumber, Address.ApartmentNumber });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
        }
    }
}
