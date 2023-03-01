using ISZR.Web.Components;
using ISZR.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace ISZR.Web.Controllers
{
    /// <summary>
    /// /Home/? Controller
    /// </summary>
    public class HomeController : Controller
    {
        private readonly DataContext _context;

        public HomeController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Irányítópult megjelenítése a felhasználó értékeivel, statisztikáival.
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            // Bejelentkezett felhasználó megkeresése
            User? user = await GetLoggedUser();

            // Amennyiben a felhasználó nem található az adatbázisban, annak továbbirányítása a regisztrációs oldalra
            if (user == null) return RedirectToAction("Index", "Welcome");

            // Amennyiben ügyintéző a felhasználó, annak statisztikáinak megjelenítése
            if (Account.IsUgyintezo())
            {
                // Felhasználó összes igénylése
                ViewBag.AllRequests = _context.Requests.Where(r => r.RequestAuthorId == user.UserId).Count();

                // Felhasználó folyamatban lévő igénylései
                ViewBag.InProgressRequests = _context.Requests.Where(r => r.Status == "Folyamatban" && r.RequestAuthorId == user.UserId).Count();

                // Felhasználó végrehajtott igénylései
                ViewBag.DoneRequests = _context.Requests.Where(r => r.Status == "Végrehajtva" && r.RequestAuthorId == user.UserId).Count();

                // Felhasználó elutasított igénylései
                ViewBag.DeniedRequests = _context.Requests.Where(r => r.Status == "Elutasítva" && r.RequestAuthorId == user.UserId).Count();
            }

            // Irányítópult megjelenítése a felhasználónak
            return View(user);
        }

        /// <summary>
        /// Felhasználók beállításainak megjelenítése
        /// </summary>
        public async Task<IActionResult> Settings()
        {
            // Bejelentkezett felhasználó megkeresése
            User? user = await GetLoggedUser();

            // Amennyiben a felhasználó nem található az adatbázisban, annak továbbirányítása a regisztrációs oldalra
            if (user == null) return RedirectToAction("Index", "Welcome");

            // Lenyíló menük adatainak betöltése
            ViewData["ClassId"] = new SelectList(_context.Classes.Where(u => !u.IsArchived).OrderBy(u => u.Name), "ClassId", "Name");
            ViewData["PositionId"] = new SelectList(_context.Positions.Where(u => !u.IsArchived).OrderBy(u => u.Name), "PositionId", "Name");

            // Felhasználó beállításainak megjelenítése
            return View(user);
        }

        /// <summary>
        /// Felhasználó által megadott felhasználói beállítások felülírása.
        /// </summary>
        /// <param name="user">Felhasználó</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings([Bind("UserId,Username,DisplayName,Email,Phone,Rank,Location,LastLogin,LogonCount,ClassId,PositionId,Genre")] User user)
        {
            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                try
                {
                    // Felhasználó értékeinek frissítése az adatbázisban
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Felhasználó visszairányítása az irányítópultba
                return RedirectToAction(nameof(Dashboard));
            }

            // Lenyíló menük adatainak betöltése
            ViewData["ClassId"] = new SelectList(_context.Classes.Where(u => !u.IsArchived).OrderBy(u => u.Name), "ClassId", "Name");
            ViewData["PositionId"] = new SelectList(_context.Positions.Where(u => !u.IsArchived).OrderBy(u => u.Name), "PositionId", "Name", user.PositionId);

            // Felhasználó beállításainak megjelenítése
            return View(user);
        }

        /// <summary>
        /// Felhasználók által gyakran ismételt kérdések megjelenítése a felhasználók részére.
        /// </summary>
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
        public async Task<IActionResult> Permissions(string name, string type)
        {
            // A rendszerben található jogosultságok betöltése
            var dataContext = _context.Permissions
                .OrderBy(r => r.Name)
                .AsQueryable();

            // Jogosultságok szűrése név alapján, amennyiben a felhasználó szűrt név alapján
            if (name != null && name != "")
            {
                dataContext = dataContext.Where(r => r.Name.ToLower().Contains(name.ToLower()));
            }

            // Jogosultságok szűrése típus alapján, amennyiben a felhasználó szűrt típus alapján
            if (type != null && type != "Mind")
            {
                if (type == "Windows jogosultság")
                {
                    // Windows jogosultságok szűrése
                    dataContext = dataContext.Where(r => r.Type == "Windows");
                }
                else
                {
                    // Főnix 3 jogosultságok szűrése
                    dataContext = dataContext.Where(r => r.Type == "Főnix 3");
                }
            }

            // Jogosultsági magyarázat megjelnítése
            return View(await dataContext.ToListAsync());
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