using ISZR.Web.Components;
using ISZR.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISZR.Web.Controllers
{
    /// <summary>
    /// /Users/? Controller
    /// </summary>
    public class UsersController : Controller
    {
        private readonly DataContext _context;

        public UsersController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Felhasználók listájának megjelenítése
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Admin jogosultság ellenőrzése
            if (!Account.IsAdmin()) return Forbid();

            // Felhasználók listájának lekérdezése
            var dataContext = _context.Users.Include(u => u.Class).Include(u => u.Position).OrderBy(u => u.DisplayName);

            // Felület megjelenítése a kért listával
            return View(await dataContext.ToListAsync());
        }

        /// <summary>
        /// Felhasználó archiválási állapotának megváltoztatása
        /// </summary>
        /// <param name="id">Felhasználói azonosító</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int? id)
        {
            // Azonosító meglétének ellenőrzése
            if (id == null || _context.Users == null) return NotFound();

            // Felhasználó megkeresése az adatbázisban
            var user = await _context.Users
                .Include(u => u.Class)
                .Include(u => u.Position)
                .FirstOrDefaultAsync(m => m.UserId == id);

            // Felhasználó meglétének ellenőrzése
            if (user == null) return NotFound();

            try
            {
                // Archiválás értékének módosítása
                user.IsArchived = !user.IsArchived;

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

            // Felület újra megjelenítése
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Adott felhasználó részletei
        /// </summary>
        /// <param name="id">Felhasználói azonosító</param>
        public async Task<IActionResult> Details(int? id)
        {
            // Admin jogosultság ellenőrzése
            if (!Account.IsAdmin()) return Forbid();

            // Azonosító meglétének ellenőrzése
            if (id == null || _context.Users == null) return NotFound();

            // Felhasználó megkeresése az adatbázisban
            var user = await _context.Users
                .Include(u => u.Class)
                .Include(u => u.Position)
                .FirstOrDefaultAsync(m => m.UserId == id);

            // Felhasználó meglétének ellenőrzése
            if (user == null) return NotFound();

            // Felület megjelenítése a kért felhasználóval
            return View(user);
        }

        /// <summary>
        /// Új felhasználó hozzáadásának a felülete
        /// </summary>
        public IActionResult Create()
        {
            // Admin jogosultság ellenőrzése
            if (!Account.IsAdmin()) return Forbid();

            // Lenyíló menük adatainak betöltése
            ViewData["ClassId"] = new SelectList(_context.Classes.Where(u => !u.IsArchived).OrderBy(u => u.Name), "ClassId", "Name");
            ViewData["PositionId"] = new SelectList(_context.Positions.Where(u => !u.IsArchived).OrderBy(u => u.Name), "PositionId", "Name");

            // Felület megjelenítése
            return View();
        }

        /// <summary>
        /// Új felhasználó hozzáadása a rendszerhez
        /// </summary>
        /// <param name="user">Felhasználó új megadott értékei</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Username,DisplayName,Email,Phone,Rank,LastLogin,ClassId,PositionId,Genre")] User user)
        {
            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                // Felhasználó meglétének ellenrőzése az adatbázisban
                User? foundUser = await CheckUsername(user.Username);

                // Amennyiben az adott felhasználónévvel rendelkezik ember, felhasználó átirányítása az adott felhasználó nevű felhasználó részleteire
                if (foundUser != null) return RedirectToAction(nameof(Details), new { @id = foundUser.UserId });

                // Felhasználó hozzáadása a rendszerhez
                _context.Add(user);
                await _context.SaveChangesAsync();

                // Adminisztrátor átirányítása a felhasználók listájához
                return RedirectToAction(nameof(Index));
            }

            // Lenyíló menük értékeinek betöltése
            ViewData["ClassId"] = new SelectList(_context.Classes.Where(u => !u.IsArchived).OrderBy(u => u.Name), "ClassId", "Name");
            ViewData["PositionId"] = new SelectList(_context.Positions.Where(u => !u.IsArchived).OrderBy(u => u.Name), "PositionId", "Name");

            // Felület újra megjelenítése, amennyiben a felhasználó hibásan adta meg a kért adatokat
            return View(user);
        }

        /// <summary>
        /// Felhasználó értékeinek módosításának felülete
        /// </summary>
        /// <param name="id">Felhasználói azonosító</param>
        public async Task<IActionResult> Edit(int? id)
        {
            // Admin jogosultság ellenőrzése
            if (!Account.IsAdmin()) return Forbid();

            // Azonosító meglétének ellenőrzése
            if (id == null || _context.Users == null) return NotFound();

            // Felhasználó megkeresése az adatbázisban
            var user = await _context.Users.FindAsync(id);

            // Felhasználó meglétének ellenőrzése
            if (user == null) return NotFound();

            // Lenyíló menük értékeinek betöltése
            ViewData["ClassId"] = new SelectList(_context.Classes.Where(u => !u.IsArchived).OrderBy(u => u.Name), "ClassId", "Name");
            ViewData["PositionId"] = new SelectList(_context.Positions.Where(u => !u.IsArchived).OrderBy(u => u.Name), "PositionId", "Name", user.PositionId);

            // Felület megjelenítése a kért felhasználó értékeivel
            return View(user);
        }

        /// <summary>
        /// Felhasználó értékeinek felülírása az adatbázisban
        /// </summary>
        /// <param name="id">Felhasználói azonosító</param>
        /// <param name="user">Felhasználó új értékei</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Username,DisplayName,Email,Phone,Rank,LastLogin,LogonCount,ClassId,PositionId,Genre")] User user)
        {
            // Azonosítók meglétének ellenőrzése
            if (id != user.UserId) return NotFound();

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
                return RedirectToAction(nameof(Index));
            }

            // Lenyíló menük értékeinek lekérdezése
            ViewData["ClassId"] = new SelectList(_context.Classes.Where(u => !u.IsArchived).OrderBy(u => u.Name), "ClassId", "Name");
            ViewData["PositionId"] = new SelectList(_context.Positions.Where(u => !u.IsArchived).OrderBy(u => u.Name), "PositionId", "Name", user.PositionId);

            // Felület újra megjelenítése, amennyiben a kért értékeket hibásan adták meg
            return View(user);
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
        /// Felhasználó megkeresése a rendszerben
        /// </summary>
        /// <param name="username">Felhasználó név</param>
        /// <returns>Felhasználó amennyiben létezik</returns>
        private async Task<User?> CheckUsername(string? username)
        {
            // Felhasználónév meglétének ellenőrzése
            if (username == null) return null;

            // Megtalált felhasználó visszaadása (amennyiben nem talált, nul értéket fog visszaadni
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}