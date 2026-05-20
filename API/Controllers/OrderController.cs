using API.DataModels;
using API.Filters;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController(IOrderSqlService orderSqlService) : ControllerBase
    {
        private readonly IOrderSqlService _orderSqlService = orderSqlService;

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetOrders()
        {
            var authHeader = Request.Headers.Authorization.ToString();
            var token = authHeader.Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var claims = jwt.Claims;
            if (claims == null) return BadRequest("Claims are null.");

            int id = Convert.ToInt32(claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value);
            if (id == 0) return BadRequest("Id is null.");

            try
            {
                var orders = await _orderSqlService.GetOrders(id);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest();
            }
        }

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpPut]
        public async Task<ActionResult> CreateOrder([FromBody] OrderModel order)
        {
            if (order == null) return BadRequest("Order is null.");
            if (order.Positions == null) return BadRequest("Positions are null.");
            if (order.Positions.Count == 0) return BadRequest("Order has no positions.");
            if (order.Address == null) return BadRequest("Address is null.");

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

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpDelete]
        public async Task<ActionResult> DeleteOrder([FromBody] int orderID)
        {
            if (orderID == 0) return BadRequest("Order id not provided.");
            try
            {
                await _orderSqlService.DeleteOrder(orderID);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest();
            }
        }
    }
}
