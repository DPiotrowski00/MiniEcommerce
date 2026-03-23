using API.Filters;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogInController(ILoggingSqlService service) : ControllerBase
    {
        private readonly ILoggingSqlService _sqlService = service;

        public class LogInRequest()
        {
            required public string Login { get; set; }
            required public string Password { get; set; }
        }

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpGet]
        public ActionResult Get()
        {
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult<string>> ValidateLogIn([FromBody] LogInRequest request)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(System.IO.File.ReadAllText("private_key.pem"));

            var privateKey = new RsaSecurityKey(rsa)
            {
                KeyId = "My_key_id"
            };

            int id = await _sqlService.ValidateLogIn(request.Login, request.Password);

            if (id != 0)
            {
                var creds = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                    new Claim(ClaimTypes.Role, "user")
                };

                var jwt = new JwtSecurityToken(
                    issuer: "https://localhost:7153",
                    audience: "https://localhost:7153",
                    signingCredentials: creds,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(10)
                    );

                var csrfToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

                Response.Cookies.Append("JWT_Token", new JwtSecurityTokenHandler().WriteToken(jwt), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(10),
                    Path = "/"
                });

                Response.Cookies.Append("CSRF_Token", csrfToken, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(10),
                    Path = "/"
                });

                return Ok();
            }

            return Unauthorized();
        }

        [HttpPut]
        public async Task<ActionResult> CreateUser([FromBody] LogInRequest request)
        {
            if (request.Login == "" || request.Password == "") return BadRequest();
            await _sqlService.CreateUser(request.Login, request.Password);
            return Ok();
        }
    }
}
