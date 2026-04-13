using FinanceTracker.Blazor.Components;
using FinanceTracker.Data;
using FinanceTracker.Data.Encryption;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

Env.Load();

var dbHost       = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort       = Environment.GetEnvironmentVariable("DB_PORT");
var dbName       = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser       = Environment.GetEnvironmentVariable("DB_USER");
var dbPassword   = Environment.GetEnvironmentVariable("DB_PASSWORD");

var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

builder.Services.AddDbContext<FinanceTrackerDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- Encryption services ---
// Singleton: stateless, safe to share across all requests.
builder.Services.AddSingleton<IEncryptionService, AesGcmEncryptionService>();

// Scoped: one instance per Blazor Server circuit.
// The user's in-memory encryption key lives here — never written to disk.
builder.Services.AddScoped<UserEncryptionContext>();

// HMAC key for email lookup: loaded from env, never stored in the DB.
// Generate with: openssl rand -base64 32
var hmacKeyBase64 = Environment.GetEnvironmentVariable("HMAC_KEY")
    ?? throw new InvalidOperationException("HMAC_KEY environment variable is not set.");
builder.Services.AddSingleton(Convert.FromBase64String(hmacKeyBase64));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FinanceTrackerDbContext>();
    await db.Database.MigrateAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
