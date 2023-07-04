global using ISZR.Web.Data;
global using ISZR.Web.Middleware;
global using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.IIS;

// Builder
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
    options.AddPolicy("Administrator", policy =>
    {
        policy.RequireRole("BV.HU\\SKFB-ISZR-Admin");
        policy.RequireAuthenticatedUser();
    });
    options.AddPolicy("Ugyintezo", policy => {
        policy.RequireRole("BV.HU\\SKFB-ISZR-Ugyintezo");
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddRazorPages();

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