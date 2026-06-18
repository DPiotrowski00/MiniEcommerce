using API.DataModels;
using Dapper;
using System.Runtime.CompilerServices;

namespace API.Services
{
    public interface IItemsSqlService
    {
        Task<List<ItemModel>> GetItems();
        Task<ItemModel> GetItemById(int ID);
        Task<int> AddItem(ItemModel item);
        Task<bool> UpdateItem(ItemModel item);
        Task<bool> AddImages(ItemModel item, List<string> images);
        Task SwitchPrimaryImage(ItemModel item, string image);
        Task DeleteImages(List<string> images);
    }

    public class ItemsSqlService (IConfiguration configuration) : IItemsSqlService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Default")!;

        public async Task<List<ItemModel>> GetItems()
        {
            string query = """
                           SELECT i.ID, i.CreatorID, l.DisplayName as CreatorName, i.Name, i.Description, i.Price, im.GUID as Thumbnail, i.CreationTime FROM items i JOIN images im ON i.ID = im.ItemID JOIN logindata l ON i.CreatorID = l.ID WHERE im.IsPrimary = 1
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                return [.. await connection.QueryAsync<ItemModel>(query)];
            }
            catch
            {
                return [];
            }
        }

        public async Task<ItemModel> GetItemById(int ID)
        {
            string query = """
                           SELECT i.ID, i.CreatorID, l.DisplayName as CreatorName, i.Name, i.Description, i.Price, im.GUID as Thumbnail, i.CreationTime FROM items i JOIN images im ON i.ID = im.ItemID JOIN logindata l ON i.CreatorID = l.ID WHERE im.IsPrimary = 1 AND i.ID = @ID
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                return await connection.QuerySingleAsync<ItemModel>(query, new { ID });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new();
            }
        }

        public async Task<int> AddItem(ItemModel item)
        {
            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync("INSERT INTO items (CreatorID, Name, Description, Price, CreationTime) VALUES (@CreatorID, @Name, @Description, @Price, @CreationTime)", new { item.CreatorID, item.Name, item.Description, item.Price, CreationTime = DateTime.UtcNow });
                await connection.ExecuteAsync("INSERT INTO images (GUID, ItemID, IsPrimary) VALUES (@Thumbnail, LAST_INSERT_ID(), 1)", new { item.Thumbnail });
                int ItemID = await connection.QuerySingleAsync("SELECT LAST_INSERT_ID()");

                transaction.Commit();
                return ItemID;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                transaction.Rollback();
                return 0;
            }
        }

        public async Task<bool> UpdateItem(ItemModel item)
        {
            string query = """
                           UPDATE items SET Name = @Name, Description = @Description, Price = @Price WHERE ID = @ID;
                           UPDATE images SET GUID = @Thumbnail WHERE ItemID = @ID AND IsPrimary = 1;
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
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

        public async Task<bool> AddImages(ItemModel item, List<string> images)
        {
            string query = """
                           INSERT INTO images (GUID, ItemID, IsPrimary) VALUES (@image, @ID, 0);
                           """;
            
            var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                foreach (var image in images)
                {
                    await connection.ExecuteAsync(query, new { image, item.ID });
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task SwitchPrimaryImage(ItemModel item, string image)
        {
            string query = """
                           UPDATE images SET IsPrimary = 0 WHERE ItemID = @ID;
                           UPDATE images SET IsPrimary = 1 WHERE ItemID = @ID AND GUID = @image;
                           """;

            var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new { item.ID, image });
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
        }

        public async Task DeleteImages(List<string> images)
        {
            string query = """
                           DELETE FROM images WHERE GUID = @image AND IsPrimary = 0
                           """;

            var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                foreach(var image in images)
                {
                    await connection.ExecuteAsync(query, new { image });
                }
                
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
        }
    }
}