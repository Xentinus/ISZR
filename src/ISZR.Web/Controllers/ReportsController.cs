using ISZR.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISZR.Web.Controllers
{
    /// <summary>
    /// /Reports/? Controller
    /// </summary>
    [Authorize(Policy = "Administrator")]
    public class ReportsController : Controller
    {
        private readonly DataContext _context;

        public ReportsController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Hibabejelentések megjelenítése szűrés alapján
        /// </summary>
        /// <param name="reportUser">Bejelentő</param>
        /// <param name="text">Címben és leírásban keresendő szavak</param>
        /// <param name="status">Hibabejelentések státusza</param>
        public async Task<IActionResult> Index(int reportUser, string text, bool status)
        {
            // Hibabejelentések listájának lekérdezése
            var dataContext = _context.Reports
                .OrderByDescending(r => r.ReportId)
                .AsQueryable();

            // Státusz alapú szűrés
            if (status)
            {
                dataContext = dataContext.Where(r => r.IsSolved);
                ViewBag.status = status;
            }
            else
            {
                dataContext = dataContext.Where(r => !r.IsSolved);
                ViewBag.status = status;
            }

            // Szöveg alapú szűrés
            if (!string.IsNullOrEmpty(text))
            {
                dataContext = dataContext.Where(r => r.Description.Contains(text));
                ViewBag.text = text;
            }

            // Személy alapú szürés
            if (reportUser != 0 && reportUser.ToString() != "Mind")
            {
                dataContext = dataContext.Where(r => r.ReportUserId == reportUser);
                ViewBag.reportUser = reportUser;
            }

            // Maximális megengedett lista értéke 50
            dataContext = dataContext.Take(50);

            // Felhasználó alapú szűréshez lista
            ViewData["ReportUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

            // Az oldal megjelenítése a hibabejelentésekkel
            return View(await dataContext.ToListAsync());
        }

        /// <summary>
        /// Hibabejelentés állapotának megváltoztatása
        /// </summary>
        /// <param name="id">Hibabejelentés azonosítója</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int? id)
        {
            // Azonosító meglétének ellenőrzése
            if (id == null || _context.Reports == null) return NotFound();

            // Hibabejelentés megkeresése az adatbázisban
            var report = await _context.Reports.FirstOrDefaultAsync(m => m.ReportId == id);

            // Hibabejelentés meglétének ellenőrzése
            if (report == null) return NotFound();

            try
            {
                // Állapot értékének módosítása
                report.IsSolved = !report.IsSolved;

                // Hibabejelentés értékeinek frissítése az adatbázisban
                _context.Update(report);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportExists(report.ReportId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Felület újra megjelenítése
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Hibabejelentés felületének megjelenítése
        /// </summary>
        [Authorize(Policy = "Ugyintezo")]
        public IActionResult Create()
        {
            // Felhasználó alapú szűréshez lista
            ViewData["ReportUserId"] = new SelectList(_context.Users, "UserId", "DisplayName");

            // Felület megjelenítése
            return View();
        }

        /// <summary>
        /// Hibabejelentés létrehozása az adatbázisban
        /// </summary>
        /// <param name="report">Hibabejelentés</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReportId,ReportUserId,Title,Description,IsSolved")] Report report)
        {
            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                // Hibabejelentést létrehozó személy azonosítója
                report.ReportUserId = await GetLoggedUserId();

                // Hibabejelentés hozzáadása az adatbázishoz
                _context.Add(report);
                await _context.SaveChangesAsync();

                // Felhasználó átnavigálása a felhasználói profilba
                return RedirectToAction("Dashboard", "Home");
            }

            // Felhasználó alapú szűréshez lista
            ViewData["ReportUserId"] = new SelectList(_context.Users, "UserId", "DisplayName", report.ReportUserId);

            // Felület megjelenítése
            return View(report);
        }

        /// <summary>
        /// Bejelentkezett felhasználó azonosítójának megszerzése
        /// </summary>
        /// <returns>Felhasználói azonosító</returns>
        private async Task<int?> GetLoggedUserId()
        {
            // Felhasználónév lekérdezése a számítógéptől
            string? activeUsername = User.Identity?.Name;
            if (activeUsername == null) return null;

            // Felhasználó megkeresése az adatbázisban
            var foundUser = await _context.Users
                .Include(u => u.Class)
                .Include(u => u.Position)
                .FirstOrDefaultAsync(m => m.Username == activeUsername);

            // Felhasználó meglétének ellenőrzése
            if (foundUser == null) return null;

            // Return user id
            return foundUser.UserId;
        }

        /// <summary>
        /// Hibabejelentés meglétének ellenőrzése
        /// </summary>
        /// <param name="id">Hibabejelentés azonosítója</param>
        /// <returns>Létezik e az adott hibabejelentés (igaz/hamis)</returns>
        private bool ReportExists(int id)
        {
            return _context.Reports.Any(e => e.ReportId == id);
        }
    }
}