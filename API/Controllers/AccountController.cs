using API.DataModels;
using API.Filters;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController(IAccountSqlService userSqlService) : ControllerBase
    {
        private readonly IAccountSqlService _userSqlService = userSqlService;

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        //[HttpGet]
        //public ActionResult<AccountInformation> GetAccountInformation()
        //{

        //}

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpGet]
        [Route("/account/address")]
        public async Task<ActionResult<AddressModel>> GetAddress()
        {
            var authHeader = Request.Headers.Authorization.ToString();
            var token = authHeader.Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var claims = jwt.Claims;
            if (claims == null) return BadRequest("Claims są null");

            int id = Convert.ToInt32(claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value);
            if (id == 0) return BadRequest("Id jest null");

            var address = await _userSqlService.GetAddress(id);
            return Ok(address);
        }

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpPut]
        [Route("/account/address")]
        public async Task<ActionResult> UpdateAddress(AddressModel address)
        {
            var authHeader = Request.Headers.Authorization.ToString();
            var token = authHeader.Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var claims = jwt.Claims;
            if (claims == null) return BadRequest("Claims are null.");

            int id = Convert.ToInt32(claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value);
            if (id == 0) return BadRequest("Id is null.");

            await _userSqlService.UpdateAddress(id, address);
            return Ok();
        }

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpDelete]
        [Route("/account/address")]
        public async Task<ActionResult> DeleteAddress()
        {
            var authHeader = Request.Headers.Authorization.ToString();
            var token = authHeader.Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var claims = jwt.Claims;
            if (claims == null) return BadRequest("Claims are null.");

            int id = Convert.ToInt32(claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value);
            if (id == 0) return BadRequest("Id is null.");

            await _userSqlService.DeleteAddress(id);
            return Ok();
        }
    }
}
