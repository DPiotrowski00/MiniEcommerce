using API.DataModels;
using Dapper;

namespace API.Services
{
    public interface IItemsSqlService
    {
        Task<bool> AddItem(ItemModel item);
        Task<bool> UpdateItem(ItemModel item);
    }

    public class ItemsSqlService (IConfiguration configuration) : IItemsSqlService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Default")!;

        public async Task<List<ItemModel>> GetItems()
        {
            string query = """
                           SELECT * FROM Items
                           """;

            var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                return [.. await connection.QueryAsync<ItemModel>(query)];
            }
            catch
            {
                return [];
            }
        }

        public async Task<bool> AddItem(ItemModel item)
        {
            string query = """
                           INSERT INTO Items (CreatorID, Name, Description, Price, Thumbnail, CreationTime) VALUES (@CreatorID, @Name, @Description, @Price, @Thumbnail, @CreationTime)
                           """;

            var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new { item.CreatorID, item.Name, item.Description, item.Price, item.Thumbnail, CreationTime = DateTime.Now });
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateItem(ItemModel item)
        {
            string query = """
                           UPDATE Items SET Name = @Name, Description = @Description, Price = @Price, Thumbnail = @Thumbnail WHERE ID = @ID
                           """;

            var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new { item.Name, item.Description, item.Price, item.Thumbnail, item.ID });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}