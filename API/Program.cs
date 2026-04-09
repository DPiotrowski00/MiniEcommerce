using API.Filters;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Threading.RateLimiting;

//Odczytanie RSA z pliku tekstowego
var rsa = RSA.Create();
rsa.ImportFromPem(File.ReadAllText("private_key.pem"));

//Wyeksportowanie klucza publicznego z RSA
var publicKey = new RsaSecurityKey(rsa.ExportParameters(false))
{
    KeyId = "RSA_KEY_ID"
};

//Odczytanie klucza prywatnego z RSA
var privateKey = new RsaSecurityKey(rsa)
{
    KeyId = "RSA_KEY_ID"
};

//Builder
var builder = WebApplication.CreateBuilder(args);

//Konfiguracja maksymalnego rozmiaru Requesta
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1_000_000;
});

//Rejestracja serwisów
builder.Services.AddControllers();
builder.Services.AddScoped<ILoggingSqlService, LoggingSqlService>();
builder.Services.AddScoped<IItemsSqlService, ItemsSqlService>();
builder.Services.AddScoped<ISessionSqlService, SessionSqlService>();
builder.Services.AddScoped<CsrfFilter>();

builder.Services.AddSingleton<RSA>(_ => rsa);
builder.Services.AddSingleton(privateKey);

//Autentykacja JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(jwtOptions =>
{
    jwtOptions.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = "https://localhost:7153",
        ValidAudience = "https://localhost:7153",
        IssuerSigningKey = publicKey,
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true
    };

    jwtOptions.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            return Task.CompletedTask;
        },
        //OnMessageReceived = context =>
        //{
        //    //Wyciągamy token JWT z cookies do kontekstu, tak aby JWT authenticator go widział
        //    context.Token = context.Request.Cookies["JWT_Token"];
        //    return Task.CompletedTask;
        //}
    };
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("LoginPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("RefreshPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("RegisterPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

//Autoryzacja JWT
builder.Services.AddAuthorization();

var app = builder.Build();

//Uruchomienie zabezpieczeń
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

//Middleware odpowiedzialny za zabezpieczenie przed atakami XSS
app.Use(async (context, next) =>
{
    context.Response.Headers.ContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self'; " +
        "img-src 'self' data:; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "frame-ancestors 'none';" +
        "form-action 'self';";

    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "DENY";

    await next();
});

//
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

//Mapowanie kontrolerów
app.MapControllers();

//Uruchomienie aplikacji
app.Run();