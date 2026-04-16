using API.DataModels;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController(IOrderSqlService orderSqlService) : ControllerBase
    {
        private readonly IOrderSqlService _orderSqlService = orderSqlService;

        

        

        [HttpPost]
        public async Task<ActionResult> CreateOrder([FromBody] OrderModel order)
        {
            if (order == null) return BadRequest("Zamówienie jest puste.");
            if (order.Positions == null) return BadRequest("Lista artykułów jest pusta.");
            if (order.Positions.Count == 0) return BadRequest("Lista artykułów jest pusta.");
            if (order.Address == null) return BadRequest("Adres jest pusty");

            try
            {

                await _orderSqlService.PlaceOrder(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest();
            }

            return Ok();
        }
    }
}
