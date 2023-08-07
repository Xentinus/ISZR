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

            // Amennyiben ügyintéző a felhasználó, annak statisztikáinak megjelenítése

            // Felhasználó összes igénylése
            viewModel.AllRequests = _context.Requests.Count(r => r.CreatedByUserId == user.UserId);

            // Felhasználó folyamatban lévő igénylései
            viewModel.InProgressRequests = _context.Requests.Count(r => r.Status == "Folyamatban" && r.CreatedByUserId == user.UserId);

            // Felhasználó végrehajtott igénylései
            viewModel.DoneRequests = _context.Requests.Count(r => r.Status == "Végrehajtva" && r.CreatedByUserId == user.UserId);

            // Felhasználó elutasított igénylései
            viewModel.DeniedRequests = _context.Requests.Count(r => r.Status == "Elutasítva" && r.CreatedByUserId == user.UserId);

            // Felhasználó aktív parkolási engedélyeinek összeszedése
            viewModel.Parkings = _context.Parkings.Where(p => p.OwnerUserId == user.UserId && !p.IsArchived).OrderBy(p => p.LicensePlate).ToList();

            // Felhasználó által használt aktív PIN kódok
            viewModel.Phones = _context.Phones.Where(p => p.PhoneUserId == user.UserId && !p.IsArchived).OrderBy(p => p.PhoneCode).ToList();

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
        public async Task<IActionResult> Settings([Bind("UserId,Username,DisplayName,Rank,Genre,Location,Email,Phone,LastLogin,ClassId,PositionId,IsArchived")] User user)
        {
            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                // Bejelentkezett felhasználó megkeresése
                User? loggedUser = await GetLoggedUser();
                if (loggedUser == null) { return Forbid(); }

                try
                {
                    // Felhasználó értékeinek felülírása
                    loggedUser.DisplayName = user.DisplayName;
                    loggedUser.Rank = user.Rank;
                    loggedUser.Genre = user.Genre;
                    loggedUser.Location = user.Location;
                    loggedUser.Email = user.Email;
                    loggedUser.Phone = user.Phone;
                    loggedUser.ClassId = user.ClassId;
                    loggedUser.PositionId = user.PositionId;

                    // Felhasználó értékeinek frissítése az adatbázisban
                    _context.Update(loggedUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(loggedUser.UserId))
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
                dataContext = dataContext.Where(r => r.Name.Contains(name));
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
        /// Rendszer állapotfelméréséről jelentés
        /// </summary>
        [Authorize(Policy = "Administrator")]
        public IActionResult HealthChecks()
        {
            var viewModel = new HealthChecksViewModel { };

            // Adatbázis ellenőrzése
            viewModel.DatabaseStatus = _databaseStatusService.IsDatabaseOnline();

            // Bejelentkezett felhasználók statisztikája
            viewModel.LoggedUserToday = _context.Users.Count(u => u.LastLogin.Date == DateTime.Now.Date);

            // Igénylések statisztika
            viewModel.RequestAll = _context.Requests.Count();
            viewModel.RequestAllDone = _context.Requests.Count(r => r.Status == "Végrehajtva");
            viewModel.RequestAllProgress = _context.Requests.Count(r => r.Status == "Folyamatban");
            viewModel.RequestAllDenied = viewModel.RequestAll - (viewModel.RequestAllDone + viewModel.RequestAllProgress);
            viewModel.RequestClosedToday = _context.Requests.Count(r => r.ClosedDateTime.Date == DateTime.Now.Date);
            viewModel.RequestClosedMonth = _context.Requests.Count(r => r.ClosedDateTime.Year == DateTime.Now.Year && r.ClosedDateTime.Month == DateTime.Now.Month);
            viewModel.RequestOpenToday = _context.Requests.Count(r => r.CreatedDateTime.Date == DateTime.Now.Date);
            viewModel.RequestOpenMonth = _context.Requests.Count(r => r.CreatedDateTime.Year == DateTime.Now.Year && r.CreatedDateTime.Month == DateTime.Now.Month);

            // Adat statisztika
            viewModel.ActiveUsers = _context.Users.Count(u => !u.IsArchived);
            viewModel.ActiveParkings = _context.Parkings.Count(p => !p.IsArchived);
            viewModel.ActiveGroups = _context.Groups.Count(g => !g.IsArchived);
            viewModel.ActiveWindowsPermission = _context.Permissions.Count(p => !p.IsArchived && p.Type == "Windows");
            viewModel.ActiveFonixPermission = _context.Permissions.Count(p => !p.IsArchived && p.Type == "Főnix 3");
            viewModel.ActivePhones = _context.Phones.Count(p => !p.IsArchived);
            viewModel.ActiveNotUsedPhone = _context.Phones.Count(p => !p.IsArchived && p.PhoneUserId == null);

            // Felület megjelenítése
            return View(viewModel);
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