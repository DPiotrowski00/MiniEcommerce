using API.DataModels;
using static API.Controllers.OrderController;

namespace API.Services
{
    public interface IOrderSqlService
    {
        Task PlaceOrder(OrderModel order);
    }

    public class OrderSqlService(IConfiguration configuration) : IOrderSqlService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Default")!;

        public async Task PlaceOrder(OrderModel order)
        {
            //string query = """
            //               INSERT INTO orders (ClientID, )
            //               """;
        }
    }
}
