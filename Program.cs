global using ISZR.Data;
global using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Negotiate;

// Builder
var builder = WebApplication.CreateBuilder(args);

// Loggings
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Services
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
	options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddRazorPages();
builder.Services.AddDbContext<DataContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext") ?? throw new InvalidOperationException("Adatbázis elérési útvonala nem található!")));

var app = builder.Build();

// Only Show Development Error View when Env is Dev
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}
else
{
	var options = new DeveloperExceptionPageOptions
	{
		SourceCodeLineCount = 2
	};

	app.UseDeveloperExceptionPage(options);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Default page
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Welcome}/{action=Index}/{id?}");

app.Run();