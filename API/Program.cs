using API.Filters;
using API.Middleware;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.Security.Cryptography;
using System.Threading.RateLimiting;

//Builder
var builder = WebApplication.CreateBuilder(args);

//Odczytanie RSA z pliku tekstowego
var rsa = RSA.Create();
rsa.ImportFromPem(builder.Configuration["JWT_PRIVATE_KEY"]);

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
builder.Services.AddScoped<IOrderSqlService, OrderSqlService>();
builder.Services.AddScoped<IAccountSqlService, AccountSqlService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<CsrfFilter>();

builder.Services.AddSingleton<RSA>(_ => rsa);
builder.Services.AddSingleton(privateKey);

string? issuer = builder.Configuration["Issuer"];
string? audience = builder.Configuration["Audience"];

builder.Services.AddSingleton(issuer!);
builder.Services.AddSingleton(audience!);

//Autentykacja JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(jwtOptions =>
{
    jwtOptions.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("LogInPolicy", httpContext =>
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

app.UseMiddleware<LoggingMiddleware>();

app.UseCors("AllowFrontend");

//Uruchomienie zabezpieczeń
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

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

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

//Mapowanie kontrolerów
app.MapControllers();

//Uruchomienie aplikacji
app.Run();