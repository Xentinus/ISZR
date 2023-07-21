global using ISZR.Web.Data;
global using ISZR.Web.Middleware;
global using Microsoft.EntityFrameworkCore;
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

// IIS Server alapú felhasználó beazonosítás
builder.Services.AddAuthentication(IISServerDefaults.AuthenticationScheme);

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
    options.AddPolicy("Ugyintezo", policy => {
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

/// HSTS - HTTPS-forgalom biztonságossá tétele
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromHours(2);
});

// Adatbázishoz való csatlakozás
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext") ?? throw new InvalidOperationException("Adatbázis elérési útvonala nem található!")));

// Alkalmazás elkészítése
var app = builder.Build();

// Hiba megjelenítése
app.UseDeveloperExceptionPage();

// Csak HTTTPS fogralom engedélyezése
app.UseHttpsRedirection();
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

// Alkalmazás elindítása
app.Run();