using ISZR.Web.Models;
using ISZR.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace ISZR.Web.Controllers
{
    /// <summary>
    /// /Home/? Controller
    /// </summary>
    [AllowAnonymous]
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
            if (user == null) return Forbid();

            var viewModel = new DashboardViewModel { User = user };

            // Amennyiben ügyintéző a felhasználó, annak statisztikáinak megjelenítése
            if (true)
            {
                // Felhasználó összes igénylése
                viewModel.AllRequests = _context.Requests.Count(r => r.CreatedByUserId == user.UserId);

                // Felhasználó folyamatban lévő igénylései
                viewModel.InProgressRequests = _context.Requests.Count(r => r.Status == "Folyamatban" && r.CreatedByUserId == user.UserId);

                // Felhasználó végrehajtott igénylései
                viewModel.DoneRequests = _context.Requests.Count(r => r.Status == "Végrehajtva" && r.CreatedByUserId == user.UserId);

                // Felhasználó elutasított igénylései
                viewModel.DeniedRequests = _context.Requests.Count(r => r.Status == "Elutasítva" && r.CreatedByUserId == user.UserId);
            }

            // Irányítópult megjelenítése a felhasználónak
            return View(viewModel);
        }

        /// <summary>
        /// Felhasználók beállításainak megjelenítése
        /// </summary>
        public async Task<IActionResult> Settings()
        {
            // Bejelentkezett felhasználó megkeresése
            User? user = await GetLoggedUser();

            // Amennyiben a felhasználó nem található az adatbázisban, annak továbbirányítása a regisztrációs oldalra
            if (user == null) return Forbid();

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
        public async Task<IActionResult> Settings([Bind("UserId,Username,DisplayName,Rank,Genre,Location,Email,Phone,LastLogin,LogonCount,ClassId,PositionId,IsArchived")] User user)
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
                .Where(p => !p.IsArchived)
                .OrderBy(p => p.Name)
                .AsQueryable();

            // Jogosultságok szűrése név alapján, amennyiben a felhasználó szűrt név alapján
            if (name != null && name != "")
            {
                dataContext = dataContext.Where(p => p.Name.ToLower().Contains(name.ToLower()));
            }

            // Jogosultságok szűrése típus alapján, amennyiben a felhasználó szűrt típus alapján
            if (type != null && type != "Mind")
            {
                if (type == "Windows jogosultság")
                {
                    // Windows jogosultságok szűrése
                    dataContext = dataContext.Where(p => p.Type == "Windows");
                }
                else
                {
                    // Főnix 3 jogosultságok szűrése
                    dataContext = dataContext.Where(p => p.Type == "Főnix 3");
                }
            }

            // Jogosultsági magyarázat megjelnítése
            return View(await dataContext.ToListAsync());
        }

        /// <summary>
        /// Rendszer állapotfelméréséről jelentés
        /// </summary>
        [Authorize(Policy = "Administrator")]
        public IActionResult HealthCheck()
        {
            // Felhasználókkal kapcsolatos statisztikák kiszámítása
            ViewBag.AllUsers = _context.Users.Count();
            ViewBag.TodayUsers = _context.Users.Count(u => u.LastLogin.Date == DateTime.Now.Date);
            ViewBag.ActiveUsers = _context.Users.Count(u => !u.IsArchived);
            ViewBag.ArchivedUsers = ViewBag.AllUsers - ViewBag.ActiveUsers;

            // Jogosultságokkal kapcsolatos statisztikák kiszámítása
            ViewBag.AllPermissions = _context.Permissions.Count();
            ViewBag.WindowsPermissions = _context.Permissions.Count(p => p.Type == "Windows");
            ViewBag.FonixPermissions = _context.Permissions.Count(p => p.Type == "Főnix 3");
            ViewBag.ActivePermissions = _context.Permissions.Count(p => !p.IsArchived);
            ViewBag.ArchivedPermissions = ViewBag.AllPermissions - ViewBag.ActivePermissions;

            // Igénylésekkel kapcsolatos statisztikák kiszámítása
            ViewBag.AllRequests = _context.Requests.Count();
            ViewBag.TodayDoneRequests = _context.Requests.Count(r => r.ClosedDateTime.Date == DateTime.Now.Date);
            ViewBag.DoneRequests = _context.Requests.Count(r => r.Status == "Végrehajtva");
            ViewBag.InProgressRequests = _context.Requests.Count(r => r.Status == "Folyamatban");
            ViewBag.DeniedRequests = _context.Requests.Count(r => r.Status == "Elutasítva");
            ViewBag.MonthRequests = _context.Requests.Count(r => r.ClosedDateTime.Month == DateTime.Now.Month);
            ViewBag.YearRequests = _context.Requests.Count(r => r.ClosedDateTime.Year == DateTime.Now.Year);
            ViewBag.TodayNewRequests = _context.Requests.Count(r => r.CreatedDateTime.Date == DateTime.Now.Date);

            // Osztályokkal kapcsolatos statisztikák kiszámítása
            ViewBag.AllClasses = _context.Classes.Count();
            ViewBag.ActiveClasses = _context.Classes.Count(c => !c.IsArchived);
            ViewBag.ArchivedClasses = ViewBag.AllClasses - ViewBag.ActiveClasses;

            // Csoportokkal kapcsolatos statisztikák kiszámítása
            ViewBag.AllGroups = _context.Groups.Count();
            ViewBag.ActiveGroups = _context.Groups.Count(c => !c.IsArchived);
            ViewBag.ArchivedGroups = ViewBag.AllGroups - ViewBag.ActiveGroups;

            // Beosztásokkal kapcsolatos statisztikák kiszámítása
            ViewBag.AllPositions = _context.Positions.Count();
            ViewBag.ActivePositions = _context.Positions.Count(c => !c.IsArchived);
            ViewBag.ArchivedPositions = ViewBag.AllPositions - ViewBag.ActivePositions;

            // Kamerákkal kapcsolatos statisztikák kiszámítása
            ViewBag.AllCameras = _context.Cameras.Count();
            ViewBag.ActiveCameras = _context.Cameras.Count(c => !c.IsArchived);
            ViewBag.ArchivedCameras = ViewBag.AllCameras - ViewBag.ActiveCameras;

            // Felület megjelenítése
            return View();
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