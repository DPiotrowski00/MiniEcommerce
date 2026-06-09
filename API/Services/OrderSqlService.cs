using API.DataModels;
using API.DataTransferObjects;
using Dapper;
using System.ComponentModel;
using static API.Controllers.OrderController;

namespace API.Services
{
    public interface IOrderSqlService
    {
        Task<OrderModel> GetOrder(int UserID);
        Task<int> PlaceOrder(OrderModel order);
        Task DeleteOrder(int OrderID);
    }

    public class OrderSqlService(IConfiguration configuration) : IOrderSqlService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Default")!;

        public async Task<int> PlaceOrder(OrderModel order)
        {
            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync("INSERT INTO orders (UserID, StatusID, TimeStamp) VALUES (@UserID, 1, NOW())", new { order.UserID });
                var OrderID = await connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");
                foreach (var pos in order.Positions)
                {
                    await connection.ExecuteAsync("INSERT INTO orderitems (OrderID, ItemID, Quantity) VALUES (@OrderID, @ItemID, @Quantity)", new { OrderID, pos.ItemID, pos.Quantity });
                }

                transaction.Commit();
                return OrderID;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                transaction.Rollback();
                return 0;
            }
        }

        public async Task DeleteOrder(int OrderID)
        {
            string query = """
                           DELETE FROM orders WHERE ID = @OrderID
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new { OrderID });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
        }

        public async Task<OrderModel> GetOrder(int OrderID)
        {
            string query = """
                           SELECT * FROM orders WHERE ID = @OrderID
                           """;

            string positionQuery = """
                                   SELECT * FROM orderitems WHERE OrderID = @OrderID
                                   """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                var order = await connection.QuerySingleAsync<OrderModel>(query, new { OrderID });
                order.Positions = [.. await connection.QueryAsync<OrderPositionDto>(positionQuery, new { OrderID })];

                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new() { Positions = [] };
            }
        }

        public async Task<List<OrderModel>> GetOrders(int UserID)
        {
            string query = """
                           SELECT * FROM orders WHERE UserID = @UserID
                           """;

            string positionQuery = """
                                   SELECT * FROM orderitems WHERE OrderID = @OrderID
                                   """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                List<OrderModel> orders = [.. await connection.QueryAsync<OrderModel>(query, new { UserID })];

                foreach(var order in orders)
                {
                    order.Positions = [.. await connection.QueryAsync<OrderPositionDto>(positionQuery, new { OrderID = order.ID })];
                }

                return orders;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return [];
            }
        }
    }
}
