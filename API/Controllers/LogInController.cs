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

        private string CreateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
         
        //Endpoint do testowania zabezpieczeń
        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpGet]
        public ActionResult Test()
        {
            return Ok();
        }

        //Endpoint do odświeżania tokenów
        [ServiceFilter(typeof(CsrfFilter))]
        [HttpPost]
        [Route("/refresh")]
        public async Task<ActionResult<string>> RefreshToken()
        {
            if (!Request.Cookies.TryGetValue("Refresh-Token", out var refreshTokenFromCookie))
                return Unauthorized();

            //Pobranie RSA z pliku tekstowego
            var rsa = RSA.Create();
            rsa.ImportFromPem(System.IO.File.ReadAllText("private_key.pem"));

            //Odczytanie klucza prywatnego z RSA
            var privateKey = new RsaSecurityKey(rsa)
            {
                KeyId = "RSA_KEY_ID"
            };

            int id = await _sqlService.GetIdFromRefreshToken(refreshTokenFromCookie);

            if (await _sqlService.ValidateRefreshToken(Convert.ToInt32(id), refreshTokenFromCookie))
            {
                //Generowanie tokena CSRF
                var csrfToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

                //Tworzenie Refresh Tokena i zapisanie go w bazie
                var refreshToken = CreateRefreshToken();
                await _sqlService.UpdateRefreshToken(id, refreshToken);

                //Utworzenie podpisu dla tokena JWT
                var creds = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);

                //W claims zaszyte są dane użytkownika
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

                //Dodanie tokena JWT do cookies odpowiedzi
                Response.Cookies.Append("Refresh-Token", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(30),
                    Path = "/"
                });

                //Dodanie tokena SCRF do cookies odpowiedzi
                Response.Cookies.Append("CSRF-Token", csrfToken, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(30),
                    Path = "/"
                });

                return Ok(new JwtSecurityTokenHandler().WriteToken(jwt));
            }
            return Unauthorized();
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

                //Tworzenie Refresh Tokena i zapisanie go w bazie
                var refreshToken = CreateRefreshToken();
                await _sqlService.UpdateRefreshToken(id, refreshToken);

                //Dodanie tokena JWT do cookies odpowiedzi
                Response.Cookies.Append("Refresh-Token", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(30),
                    Path = "/"
                });

                //Dodanie tokena SCRF do cookies odpowiedzi
                Response.Cookies.Append("CSRF-Token", csrfToken, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(30),
                    Path = "/"
                });

                //Zwracaj Ok jeśli poprawnie zalogowano
                return Ok(new JwtSecurityTokenHandler().WriteToken(jwt));
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
