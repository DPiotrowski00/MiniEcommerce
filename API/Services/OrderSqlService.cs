using API.DataModels;
using API.DataTransferObjects;
using Dapper;
using static API.Controllers.OrderController;

namespace API.Services
{
    public interface IOrderSqlService
    {
        Task<List<OrderModel>> GetOrders(int UserID);
        Task PlaceOrder(OrderModel order);
        Task DeleteOrder(int OrderID);
    }

    public class OrderSqlService(IConfiguration configuration) : IOrderSqlService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Default")!;

        public async Task PlaceOrder(OrderModel order)
        {
            string query = """
                           INSERT INTO orders (UserID) VALUES (@UserID)
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new { order.UserID });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
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
