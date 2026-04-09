using API.Filters;
using API.Helpers;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    public class LogInController(ILoggingSqlService loginService, ISessionSqlService sessionService, RsaSecurityKey privateKey, string issuer, string audience) : ControllerBase
    {
        private readonly ILoggingSqlService _loginSqlService = loginService;
        private readonly ISessionSqlService _sessionSqlService = sessionService;

        private readonly RsaSecurityKey _privateKey = privateKey;

        private readonly string _issuer = issuer;
        private readonly string _audience = audience;

        //Szablony filtrujące login i hasło przesłane przez użytkownika
        private readonly string loginRegex = "^[0-9a-zA-Z]{3,8}$";
        private readonly string passwordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,64}$";

        public class RefreshTokenRequest()
        {
            public string? DeviceID { get; set; }
        }

        private static string CreateRefreshToken()
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

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpPost]
        [Route("/logout")]
        public async Task<ActionResult> LogOut()
        {
            if (!Request.Cookies.TryGetValue("Refresh-Token", out var refreshTokenFromCookie))
                return Unauthorized();

            var session = await _sessionSqlService.GetSessionByToken(refreshTokenFromCookie);
            await _sessionSqlService.RevokeSession(session!.ID);

            Response.Cookies.Delete("Refresh-Token", new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

            Response.Cookies.Delete("CSRF-Token", new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

            return Ok();
        }

        //Endpoint do odświeżania tokenów
        [EnableRateLimiting("RefreshPolicy")]
        [ServiceFilter(typeof(CsrfFilter))]
        [HttpPost]
        [Route("/login/refresh")]
        public async Task<ActionResult<string>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!Request.Cookies.TryGetValue("Refresh-Token", out var refreshTokenFromCookie))
                return Unauthorized();

            int RevokeSessionID = await _sessionSqlService.CheckForTokenReuse(refreshTokenFromCookie);

            if (RevokeSessionID != 0)
            {
                await _sessionSqlService.RevokeSession(RevokeSessionID);
                return Unauthorized();
            }

            var session = await _sessionSqlService.GetSessionByToken(refreshTokenFromCookie);

            if (session == null || session.ExpiresAt <= DateTime.UtcNow || session.IsRevoked)
                return Unauthorized();

            if (session.DeviceID == request.DeviceID)
            {
                //Generowanie tokena CSRF
                var csrfToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

                //Tworzenie Refresh Tokena i zapisanie go w bazie
                var refreshToken = CreateRefreshToken();

                session.ExpiresAt = DateTime.UtcNow.AddDays(30);
                session.RefreshTokenHash = HashHelper.ComputeSha256(refreshToken);

                await _sessionSqlService.RotateRefreshToken(session, refreshTokenFromCookie);

                //Utworzenie podpisu dla tokena JWT
                var creds = new SigningCredentials(_privateKey, SecurityAlgorithms.RsaSha256);

                //W claims zaszyte są dane użytkownika
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, session.UserID.ToString()),
                    new Claim(ClaimTypes.Role, "user"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("sid", session.ID.ToString())
                };

                var jwt = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
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

                Response.Headers.CacheControl = "no-store";
                Response.Headers.Pragma = "no-cache";

                return Ok(new JwtSecurityTokenHandler().WriteToken(jwt));
            }
            return Unauthorized();
        }

        //Endpoint do walidacji logowania. W odpowiedzi zwraca [JWS http-only secure cookie] i [CSRF secure cookie]
        [EnableRateLimiting("LogInPolicy")]
        [HttpPost]
        public async Task<ActionResult<string>> ValidateLogIn([FromBody] LogInData request)
        {
            //Walidacja wartości wprowadzonych przez użytkownika
            if (request.Login == null || request.Password == null) return BadRequest();

            //Sprawdzenie czy wartości są zgodne z szablonem
            var matchLogin = Regex.Match(request.Login, loginRegex);
            var matchPassword = Regex.Match(request.Password, passwordRegex);
            if (!matchLogin.Success || !matchPassword.Success || request.DeviceID == null)
            {
                //Zwracaj BadRequest gdy login lub hasło nie jest poprawne
                return BadRequest();
            }

            int id = await _loginSqlService.ValidateLogIn(request.Login, request.Password);

            if (id != 0)
            {
                //Generowanie tokena CSRF
                var csrfToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

                //Tworzenie Refresh Tokena i zapisanie go w bazie
                var refreshToken = CreateRefreshToken();
                var session = await _sessionSqlService.GetSessionByDeviceId(id, request.DeviceID);

                if (session == null || session.ExpiresAt < DateTime.UtcNow || session.IsRevoked)
                {
                    session = new()
                    {
                        UserID = id,
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(30),
                        RefreshTokenHash = HashHelper.ComputeSha256(refreshToken),
                        DeviceID = request.DeviceID,
                    };
                    session.ID = await _sessionSqlService.CreateSession(session);
                }
                else
                {
                    var oldTokenHash = session.RefreshTokenHash;
                    session.RefreshTokenHash = HashHelper.ComputeSha256(refreshToken);
                    session.ExpiresAt = DateTime.UtcNow.AddDays(30);

                    await _sessionSqlService.RotateRefreshToken(session, oldTokenHash!);
                }

                //Utworzenie podpisu do podpiania tokena JWT
                var creds = new SigningCredentials(_privateKey, SecurityAlgorithms.RsaSha256);

                //W claims zaszyte są dane użytkownika
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                    new Claim(ClaimTypes.Role, "user"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("sid", session.ID.ToString())
                };

                //Generowanie tokena JWT
                var jwt = new JwtSecurityToken(
                    issuer: _issuer,
                    audience: _audience,
                    signingCredentials: creds,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(10)
                    );

                //Dodanie refresh tokena do cookies odpowiedzi
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

                Response.Headers.CacheControl = "no-store";
                Response.Headers.Pragma = "no-cache";

                //Zwracaj Ok jeśli poprawnie zalogowano
                return Ok(new JwtSecurityTokenHandler().WriteToken(jwt));
            }

            //Zwracaj Unauthorized jeśli login lub hasło się nie zgadza
            return Unauthorized();
        }

        //Endpoint odpowiedzialny za tworzenie nowych użytkowników
        [EnableRateLimiting("RegisterPolicy")]
        [HttpPut]
        public async Task<ActionResult> CreateUser([FromBody] LogInData request)
        {
            //Walidacja wartości wprowadzonych przez użytkownika
            if (request.Login == null || request.Password == null) return BadRequest();

            //Sprawdzenie czy wartości są zgodne z szablonem
            var matchLogin = Regex.Match(request.Login, loginRegex);
            var matchPassword = Regex.Match(request.Password, passwordRegex);
            if (!matchLogin.Success || !matchPassword.Success)
            {
                //Zwracaj BadRequest gdy login lub hasło nie jest poprawne
                return BadRequest();
            }

            //Finalnie spróbuj utworzyć użytkownika
            if(await _loginSqlService.CreateUser(request.Login, request.Password))
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
