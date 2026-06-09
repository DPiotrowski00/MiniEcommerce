using API.DataModels;
using API.DataTransferObjects;
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
    public class OrderController(IOrderSqlService orderSqlService, IEmailService emailService) : ControllerBase
    {
        private readonly IOrderSqlService _orderSqlService = orderSqlService;
        private readonly IEmailService _emailService = emailService;

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpGet("{OrderID}")]
        public async Task<ActionResult> GetOrder(int OrderID)
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
                var orders = await _orderSqlService.GetOrder(OrderID);
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
        public async Task<ActionResult> CreateOrder([FromBody] PlaceOrderRequest request)
        {
            if (request == null) return BadRequest("Order is null.");
            if (request.Items == null) return BadRequest("Positions are null.");
            if (request.Items.Count == 0) return BadRequest("Order has no positions.");

            var authHeader = Request.Headers.Authorization.ToString();
            var token = authHeader.Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var claims = jwt.Claims;
            if (claims == null) return BadRequest("Claims are null.");

            int id = Convert.ToInt32(claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value);
            if (id == 0) return BadRequest("Id is null.");

            OrderModel order = new()
            {
                UserID = id,
                Positions = request.Items
            };

            try
            {
                order.ID = await _orderSqlService.PlaceOrder(order);

                await _emailService.SendOrderConfirmation(order);
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
            var authHeader = Request.Headers.Authorization.ToString();
            var token = authHeader.Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var claims = jwt.Claims;
            if (claims == null) return BadRequest("Claims are null.");

            int id = Convert.ToInt32(claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value);
            if (id == 0) return BadRequest("Id is null.");

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
