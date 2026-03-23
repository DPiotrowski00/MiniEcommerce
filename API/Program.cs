using API.Filters;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

var rsa = RSA.Create();
rsa.ImportFromPem(File.ReadAllText("private_key.pem"));

var publicKey = new RsaSecurityKey(rsa.ExportParameters(false))
{
    KeyId = "My_key_id"
};

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1_000_000;
});

builder.Services.AddControllers();
builder.Services.AddScoped<ILoggingSqlService, LoggingSqlService>();
builder.Services.AddScoped<CsrfFilter>();

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
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["JWT_Token"];
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

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

app.MapControllers();

app.Run();