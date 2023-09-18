using ISZR.Web.Models;
using ISZR.Web.Services;
using ISZR.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace ISZR.Web.Controllers
{
    /// <summary>
    /// /Home/? Controller
    /// </summary>
    [Authorize(Policy = "Megtekinto")]
    public class HomeController : Controller
    {
        private readonly DataContext _context;
        private readonly IDatabaseStatusService _databaseStatusService;

        public HomeController(DataContext context, IDatabaseStatusService databaseStatusService)
        {
            _context = context;
            _databaseStatusService = databaseStatusService;
        }

        /// <summary>
        /// Irányítópult megjelenítése a felhasználó értékeivel, statisztikáival.
        /// </summary>

        public async Task<IActionResult> Dashboard()
        {
            // Bejelentkezett felhasználó megkeresése
            User? user = await GetLoggedUser();

            // Amennyiben a felhasználó nem található az adatbázisban, annak továbbirányítása a regisztrációs oldalra
            if (user == null) return Forbid();

            var viewModel = new DashboardViewModel { User = user };

            // Felhasználó aktív parkolási engedélyeinek összeszedése
            viewModel.Parkings = await _context.Parkings
                .Where(p => p.OwnerUserId == user.UserId && !p.IsArchived)
                .OrderBy(p => p.LicensePlate)
                .ToListAsync();

            // Felhasználó által használt aktív PIN kódok
            viewModel.Phones = await _context.Phones
                .Where(p => p.PhoneUserId == user.UserId && !p.IsArchived)
                .OrderBy(p => p.PhoneCode)
                .ToListAsync();

            // Felhasználó utolsó igénylései
            viewModel.LastRequests = await _context.Requests
                .Where(r => r.CreatedForUser == user || r.CreatedByUser == user)
                .OrderByDescending(r => r.RequestId)
                .Take(8)
                .ToListAsync();

            // Lenyíló menük adatainak betöltése
            ViewData["ClassId"] = new SelectList(_context.Classes.Where(u => !u.IsArchived).OrderBy(u => u.Name), "ClassId", "Name");
            ViewData["PositionId"] = new SelectList(_context.Positions.Where(u => !u.IsArchived).OrderBy(u => u.Name), "PositionId", "Name");

            // Irányítópult megjelenítése a felhasználónak
            return View(viewModel);
        }

        /// <summary>
        /// Felhasználó elérhetőségének módosítása
        /// </summary>
        /// <param name="user">Megadott új felhasználói értékek</param>
        /// <param name="id">Felhasználó azonosítója</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dashboard([Bind("UserId,Username,DisplayName,Rank,Genre,Location,Email,Phone,LastLogin,ClassId,PositionId,IsArchived")] User user, int? id)
        {
            // Azonosító meglétének ellenőrzése
            if (id == null || user.UserId != id || _context.Users == null) return NotFound();

            // Felhasználó megkeresése az adatbázisban
            var foundUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            // Felhasználó meglétének ellenőrzése
            if (foundUser == null) return NotFound();

            try
            {
                // Felhasználó adatainak felülírása a megadott értékekkel
                foundUser.DisplayName = user.DisplayName;
                foundUser.Genre = user.Genre;
                foundUser.Rank = user.Rank;
                foundUser.ClassId = user.ClassId;
                foundUser.PositionId = user.PositionId;
                foundUser.Location = user.Location;
                foundUser.Email = user.Email;
                foundUser.Phone = user.Phone;

                _context.Update(foundUser);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(foundUser.UserId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Felület újra megjelenítése
            return RedirectToAction(nameof(Dashboard));
        }

        /// <summary>
        /// Felhasználók által gyakran ismételt kérdések megjelenítése a felhasználók részére.
        /// </summary>
        [Authorize(Policy = "Ugyintezo")]
        public IActionResult FAQ()
        {
            // Gyakran ismételt kérdések oldalának megjelenítése
            return View();
        }

        /// <summary>
        /// A rendszerben található jogosultságok megjelenítése magyarázattal a felhasználó részére.
        /// </summary>
        /// <param name="name">Szűrés név alapján</param>
        /// <param name="type">Szűrés típus alapján</param>
        public async Task<IActionResult> Permissions(string name, string type, int? pageNumber)
        {
            // Kapott értékek beállítása
            ViewData["name"] = name;
            ViewData["type"] = type;

            // A rendszerben található jogosultságok betöltése
            var dataContext = _context.Permissions
                .Where(p => !p.IsArchived)
                .OrderBy(p => p.Name)
                .AsQueryable();

            // Jogosultságok szűrése név alapján, amennyiben a felhasználó szűrt név alapján
            if (!string.IsNullOrEmpty(name))
            {
                dataContext = dataContext.Where(r => r.Name.Contains(name) || r.Description.Contains(name));
            }

            // Jogosultságok szűrése típus alapján, amennyiben a felhasználó szűrt típus alapján
            if (type != null && type != "Mind")
            {
                dataContext = dataContext.Where(p => p.Type == type);
            }

            // Típus lista összeállítása
            List<string?> permissionTypes = _context.Permissions.Select(r => r.Type).Distinct().OrderBy(r => r).ToList();
            permissionTypes.Insert(0, "Mind");

            ViewData["TypeList"] = permissionTypes.Select(type => new SelectListItem
            {
                Text = type,
                Value = type
            }).ToList();

            // Igénylési lista összeállítása
            await dataContext.ToListAsync();
            ViewData["dataLength"] = dataContext.Count();

            // Jogosultsági magyarázat megjelnítése
            return View(await PaginatedList<Permission>.CreateAsync(dataContext, pageNumber ?? 1, 25));
        }

        /// <summary>
        /// Az aktuális folyamatban hiba történt, ennek tájékoztatása a felhasználó felé.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Hiba üzenet megjelenítése a felhasználó részére
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Felhasználói azonosító megkeresése a rendszerben
        /// </summary>
        /// <param name="id">Felhasználói azonosító</param>
        /// <returns>Létezik e a felhasználói azonosító (igaz/hamis)</returns>
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        /// <summary>
        /// Bejelentkezett felhasználó megkeresése a rendszerben
        /// </summary>
        /// <returns>Bejelentkezett felhasználó adatai</returns>
        private async Task<User?> GetLoggedUser()
        {
            // Felhasználónév lekérése a számítógéptől
            string? activeUsername = User.Identity?.Name;

            // Amennyiben nem található a rendszerben felhasználónév (pl linux), kérelem elutasítása
            if (activeUsername == null) return null;

            // Felhasználó megkeresése a lekért felhasználónév által
            var foundUser = await _context.Users
                .Include(u => u.Class)
                .Include(u => u.Position)
                .FirstOrDefaultAsync(m => m.Username == activeUsername);

            // Megtalált felhasználó visszaadása (amennyiben nem talált, null értéket fog visszaadni)
            return foundUser;
        }
    }
}