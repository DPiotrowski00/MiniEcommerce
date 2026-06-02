using API.DataModels;
using API.Filters;
using API.Helpers;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace API.Controllers
{
    //Kontroler logowania oraz rejestracji
    [ApiController]
    [Route("[controller]")]
    public class LogInController(ILoggingSqlService loginService, ISessionSqlService sessionService, IEmailService emailService, RsaSecurityKey privateKey, string issuer, string audience) : ControllerBase
    {
        private readonly ILoggingSqlService _loginSqlService = loginService;
        private readonly ISessionSqlService _sessionSqlService = sessionService;
        private readonly IEmailService _emailService = emailService;

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

        public record PassChangeInfo
        {
            public string? OldPass { get; set; }
            public string? NewPass { get; set; }
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
            var authHeader = Request.Headers.Authorization.ToString();
            var token = authHeader.Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var claims = jwt.Claims;
            if (claims == null) return BadRequest("Claims are null.");

            int id = Convert.ToInt32(claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value);
            if (id == 0) return BadRequest("Id is null.");

            if (!Request.Cookies.TryGetValue("Refresh-Token", out var refreshTokenFromCookie))
                return Unauthorized();

            var session = await _sessionSqlService.GetSessionByToken(refreshTokenFromCookie);
            await _sessionSqlService.RevokeSession(session!.ID);

            Response.Cookies.Delete("Refresh-Token", new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            Response.Cookies.Delete("CSRF-Token", new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            return Ok();
        }

        //Endpoint do odświeżania tokenów
        [EnableRateLimiting("RefreshPolicy")]
        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
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

            if (session == null || session.IsRevoked)
                return Unauthorized();

            if (session.ExpiresAt <= DateTime.UtcNow)
            {
                await _sessionSqlService.RevokeSession(session.ID);
            }

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

                //Dodanie refresh tokena do cookies odpowiedzi
                Response.Cookies.Append("Refresh-Token", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(30),
                    Path = "/"
                });

                //Dodanie tokena SCRF do cookies odpowiedzi
                Response.Cookies.Append("CSRF-Token", csrfToken, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(30),
                    Path = "/"
                });

                Response.Headers.CacheControl = "no-store";
                Response.Headers.Pragma = "no-cache";

                return Ok(new JwtSecurityTokenHandler().WriteToken(jwt));
            }
            return Unauthorized();
        }

        //Endpoint do walidacji logowania. W odpowiedzi zwraca [JWS w body], [CSRF jako secure cookie] oraz [RefreshToken jako secure http-only cookie]
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

                //Utworzenie podpisu do podpisania tokena JWT
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
                    expires: DateTime.UtcNow.AddMinutes(60)
                    );

                //Dodanie refresh tokena do cookies odpowiedzi
                Response.Cookies.Append("Refresh-Token", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(30),
                    Path = "/"
                });

                //Dodanie tokena SCRF do cookies odpowiedzi
                Response.Cookies.Append("CSRF-Token", csrfToken, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.None,
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
            if (request.Login == null || request.Password == null || request.Email == null) return BadRequest();

            //Sprawdzenie czy wartości są zgodne z szablonem
            var matchLogin = Regex.Match(request.Login, loginRegex);
            var matchPassword = Regex.Match(request.Password, passwordRegex);
            if (!matchLogin.Success || !matchPassword.Success)
            {
                //Zwracaj BadRequest gdy login lub hasło nie jest poprawne
                return BadRequest();
            }

            var VerificationToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
            request.VerificationToken = VerificationToken;

            //Finalnie spróbuj utworzyć użytkownika
            if (await _loginSqlService.CreateUser(request))
            {
                //Jeśli tworzenie użytkownika się powiodło odpowiadaj Ok
                if (await _emailService.SendEmail(request.Email, VerificationToken))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Email verification failed");
                }
            }
            else
            {
                //Jeśli nie udało się utworzyć użytkownika odpowiadaj BadRequest
                return BadRequest();
            }
        }

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [Route("/password")]
        [HttpPost]
        public async Task<ActionResult> SetNewPassword([FromBody] PassChangeInfo request)
        {
            var authHeader = Request.Headers.Authorization.ToString();
            var token = authHeader.Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var claims = jwt.Claims;
            if (claims == null) return BadRequest("Claims are null.");

            int id = Convert.ToInt32(claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value);
            if (id == 0) return BadRequest("Id is null.");

            if (request.NewPass == null || request.OldPass == null)
            {
                return BadRequest("Password cannot be null.");
            }

            var matchPassword = Regex.Match(request.NewPass, passwordRegex);
            if (!matchPassword.Success)
            {
                return BadRequest("Password doesn't match regex.");
            }
            else
            {
                if (await _loginSqlService.ChangePassword(id, request.OldPass, request.NewPass))
                {
                    return Ok("Password change successful.");
                }
                else
                {
                    return BadRequest("Password change was not successful.");
                }
            }
        }

        [HttpGet("verify-email")]
        public async Task<ActionResult> VerifyEmail(string token)
        {
            if (token == null) return BadRequest("Token cannot be null");

            if (await _loginSqlService.VerifyEmail(token))
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
