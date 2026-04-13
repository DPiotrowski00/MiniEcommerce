using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("controller")]
    public class OrderController(IOrderSqlService orderSqlService) : ControllerBase
    {
        private readonly IOrderSqlService _orderSqlService = orderSqlService;

        public class Address
        {
            public string? Country { get; set; }
            public string? PostalCode { get; set; }
            public string? City { get; set; }
            public string? Street { get; set; }
            public string? BuildingNumber { get; set; }
            public string? ApartmentNumber { get; set; }
        }

        public class Position
        {
            public int ItemID { get; set; }
            public int Quantity { get; set; }
        }

        public class OrderRequest
        {
            public int UserID { get; set; }
            public List<Position>? Positions { get; set; }
            public Address? Address { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrder([FromBody] OrderRequest order)
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
