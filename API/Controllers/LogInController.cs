using API.Filters;
using API.Helpers;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace API.Controllers
{
    //Kontroler logowania oraz rejestracji
    [ApiController]
    [Route("[controller]")]
    public class LogInController(ILoggingSqlService service) : ControllerBase
    {
        private readonly ILoggingSqlService _sqlService = service;

        //Endpoint do testowania zabezpieczeń
        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpGet]
        public ActionResult Test()
        {
            return Ok();
        }

        //Endpoint do walidacji logowania. W odpowiedzi zwraca [JWS http-only secure cookie] i [CSRF secure cookie]
        [HttpPost]
        public async Task<ActionResult<string>> ValidateLogIn([FromBody] LogInData request)
        {
            //Walidacja wartości wprowadzonych przez użytkownika
            if (request.Login == null || request.Password == null) return BadRequest();
            //Szablon filtrujący login i hasło przesłane przez użytkownika, dozwolone znaki to litery a-z, A-Z oraz cyfry 0-9
            string regex = "[0-9a-zA-Z]{3,8}";

            //Sprawdzenie czy wartości są zgodne z szablonem
            var matchLogin = Regex.Match(request.Login, regex);
            var matchPassword = Regex.Match(request.Password, regex);
            if (!matchLogin.Success || !matchPassword.Success)
            {
                //Zwracaj BadRequest gdy login lub hasło nie jest poprawne
                return BadRequest();
            }

            //Pobranie RSA z pliku tekstowego
            var rsa = RSA.Create();
            rsa.ImportFromPem(System.IO.File.ReadAllText("private_key.pem"));

            //Odczytanie klucza prywatnego z RSA
            var privateKey = new RsaSecurityKey(rsa)
            {
                KeyId = "RSA_KEY_ID"
            };

            int id = await _sqlService.ValidateLogIn(request.Login, request.Password);

            if (id != 0)
            {
                //Utworzenie podpisu dla tokena JWT
                var creds = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);

                //W claims zaszyte są dane użytkownika
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                    new Claim(ClaimTypes.Role, "user")
                };

                //Generowanie tokena JWT
                var jwt = new JwtSecurityToken(
                    issuer: "https://localhost:7153",
                    audience: "https://localhost:7153",
                    signingCredentials: creds,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(10)
                    );

                //Generowanie tokena CSRF
                var csrfToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

                //Dodanie tokena JWT do cookies odpowiedzi
                Response.Cookies.Append("JWT_Token", new JwtSecurityTokenHandler().WriteToken(jwt), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(10),
                    Path = "/"
                });

                //Dodanie tokena SCRF do cookies odpowiedzi
                Response.Cookies.Append("CSRF_Token", csrfToken, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(10),
                    Path = "/"
                });

                //Zwracaj Ok jeśli poprawnie zalogowano
                return Ok();
            }

            //Zwracaj Unauthorized jeśli login lub hasło się nie zgadza
            return Unauthorized();
        }

        //Endpoint odpowiedzialny za tworzenie nowych użytkowników
        [HttpPut]
        public async Task<ActionResult> CreateUser([FromBody] LogInData request)
        {
            //Walidacja wartości wprowadzonych przez użytkownika
            if (request.Login == null || request.Password == null) return BadRequest();
            //Szablon filtrujący login i hasło przesłane przez użytkownika, dozwolone znaki to litery a-z, A-Z oraz cyfry 0-9
            string regex = "[0-9a-zA-Z]{3,8}";

            //Sprawdzenie czy wartości są zgodne z szablonem
            var matchLogin = Regex.Match(request.Login, regex);
            var matchPassword = Regex.Match(request.Password, regex);
            if (!matchLogin.Success || !matchPassword.Success)
            {
                //Zwracaj BadRequest gdy login lub hasło nie jest poprawne
                return BadRequest();
            }

            //Finalnie spróbuj utworzyć użytkownika
            if(await _sqlService.CreateUser(request.Login, request.Password))
            {
                //Jeśli tworzenie użytkownika się powiodło odpowiadaj Ok
                return Ok();
            }
            else
            {
                //Jeśli nie udało się utworzyć użytkownika odpowiadaj BadRequest
                return BadRequest();
            }
            
        }
    }
}
