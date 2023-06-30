global using ISZR.Web.Data;
global using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Negotiate;

// Builder
var builder = WebApplication.CreateBuilder(args);

// Loggings
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Services
builder.Services.AddControllersWithViews(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
        .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Ugyintezo", policy =>
    {
        policy.RequireRole("SKFB-ISZR-Ugyintezo");
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