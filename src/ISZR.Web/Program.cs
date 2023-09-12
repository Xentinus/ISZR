global using ISZR.Web.Data;
global using ISZR.Web.Middleware;
global using ISZR.Web.Services;
global using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.IIS;

// Alkalmazás felépítése
var builder = WebApplication.CreateBuilder(args);

// Loggings
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Services
builder.Services.AddControllersWithViews(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

// Felhasználó utolsó látógatás idejének frissítése
builder.Services.AddScoped<UpdateUserUptime>();

// E-mailes értesítés betöltése
builder.Services.AddScoped<EmailService>();

// Autentikáció beállítása Environment alapján
if (builder.Environment.IsDevelopment())
{
    // IIS Express (Development)
    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
}
else
{
    // IIS Server (Staging, Production)
    builder.Services.AddAuthentication(IISServerDefaults.AuthenticationScheme);
}

// Policyk létrehozása Windows Jogosultságok alapján
builder.Services.AddAuthorization(options =>
{
    // Adminisztrátori jogosultság
    options.AddPolicy("Administrator", policy =>
    {
        policy.RequireRole("BV.HU\\SKFB-ISZR-Admin");
        policy.RequireAuthenticatedUser();
    });

    // Ügyintézői jogosultság
    options.AddPolicy("Ugyintezo", policy =>
    {
        policy.RequireRole("BV.HU\\SKFB-ISZR-Ugyintezo");
        policy.RequireAuthenticatedUser();
    });

    // Megtekintői jogosultság
    options.AddPolicy("Megtekinto", policy =>
    {
        policy.RequireRole("BV.HU\\SKFB-ISZR");
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddRazorPages();

// Életjel ellenőrző rendszer
builder.Services.AddHealthChecks();

// Adatbázishoz való csatlakozás
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext") ?? throw new InvalidOperationException("Adatbázis elérési útvonala nem található!")));

// Services
builder.Services.AddSingleton<IDatabaseStatusService, DatabaseStatusService>();
builder.Services.AddSingleton<IUserService, UserService>();

// Alkalmazás elkészítése
var app = builder.Build();

// Hiba megjelenítése
app.UseDeveloperExceptionPage();

app.UseStaticFiles();

app.UseRouting();

// Felhasználó utolsó látógatás idejének frissítése
app.UseMiddleware<UpdateUserUptime>();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("CorsPolicy");

// Alapértelmezett útvonal beállítása
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Dashboard}/{id?}");

// Életjel ellenőrző rendszer elérése
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = _ => true // teljes health check
}).RequireAuthorization();

// Alkalmazás elindítása
app.Run();