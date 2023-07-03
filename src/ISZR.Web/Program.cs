global using ISZR.Web.Data;
global using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.IIS;

// Builder
var builder = WebApplication.CreateBuilder(args);

// Loggings
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Services
builder.Services.AddControllersWithViews(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

builder.Services.AddAuthentication(IISServerDefaults.AuthenticationScheme);

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
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext") ?? throw new InvalidOperationException("Adatbázis elérési útvonala nem található!")));

var app = builder.Build();

// Hiba megjelenítése
app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("CorsPolicy");

// Alapértelmezett útvonal beállítása
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Dashboard}/{id?}");

// Alkalmazás elindítása
app.Run();