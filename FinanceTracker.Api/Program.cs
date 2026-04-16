using System.Text;
using FinanceTracker.Api.Services;
using FinanceTracker.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

DotNetEnv.Env.Load();

static string? E(string key) => Environment.GetEnvironmentVariable(key);

var builder = WebApplication.CreateBuilder(args);

// --- Database ---
var connectionString =
    $"Host={E("DB_HOST")};Port={E("DB_PORT")};Database={E("DB_NAME")};" +
    $"Username={E("DB_USER")};Password={E("DB_PASSWORD")}";

builder.Services.AddDbContext<FinanceTrackerDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- Services ---
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddSingleton<ITokenService, TokenService>();

// HMAC key for pseudonymous email lookup — never stored in DB
var hmacKey = Convert.FromBase64String(
    E("HMAC_KEY") ?? throw new InvalidOperationException("HMAC_KEY not set."));
builder.Services.AddSingleton(hmacKey);

// --- JWT ---
builder.Configuration["Jwt:Secret"] = E("JWT_SECRET")
    ?? throw new InvalidOperationException("JWT_SECRET not set.");
builder.Configuration["Jwt:Issuer"] = "FinanceTracker";
builder.Configuration["Jwt:Audience"] = "FinanceTrackerClient";
builder.Configuration["Jwt:ExpiryMinutes"] = "60";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "FinanceTracker",
            ValidAudience = "FinanceTrackerClient",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
        };

        // Read the JWT from the httpOnly cookie when no Authorization header is present
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (string.IsNullOrEmpty(ctx.Request.Headers.Authorization) &&
                    ctx.Request.Cookies.TryGetValue("ft_auth", out var token))
                {
                    ctx.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// --- CORS: allow Angular dev server (credentials required for httpOnly cookie) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FinanceTrackerDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors("Angular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
