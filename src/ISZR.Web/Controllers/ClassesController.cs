using ISZR.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISZR.Web.Controllers
{
    /// <summary>
    /// /Classes/? Controller
    /// </summary>
    [Authorize(Policy = "Administrator")]
    public class ClassesController : Controller
    {
        private readonly DataContext _context;

        public ClassesController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Osztályok listájának megjelenítése
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index(string name, string user, bool status, int? pageNumber)
        {
            // Értékek beállítása
            ViewData["name"] = name;
            ViewData["user"] = user;
            ViewData["status"] = status;

            // Osztályok listájának lekérdezése
            var dataContext = _context.Classes
                .Include(c => c.Authorizer)
                .Include(c => c.Authorizer.Position)
                .OrderBy(c => c.Name)
                .AsQueryable();

            // Státusz alapú szűrés
            if (status)
            {
                dataContext = dataContext.Where(r => !r.IsArchived);
            }
            else
            {
                dataContext = dataContext.Where(r => r.IsArchived);
            }

            // Név alapú szürés
            if (!string.IsNullOrEmpty(name))
            {
                dataContext = dataContext.Where(r => r.Name.Contains(name));
            }

            // Engedélyező neve alapú szürés
            if (!string.IsNullOrEmpty(user))
            {
                dataContext = dataContext.Where(r => r.Authorizer.DisplayName.Contains(user));
            }

            // Igénylési lista összeállítása
            await dataContext.ToListAsync();
            ViewData["dataLength"] = dataContext.Count();

            // Felület megjelenítése a kért listával
            return View(await PaginatedList<Class>.CreateAsync(dataContext, pageNumber ?? 1));
        }

        /// <summary>
        /// Osztály archiválási állapotának megváltoztatása
        /// </summary>
        /// <param name="id">Osztály azonosítója</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int? pageNumber, int? id)
        {
            // Azonosító meglétének ellenőrzése
            if (id == null || _context.Classes == null) return NotFound();

            // Osztály megkeresése az adatbázisban
            var @class = await _context.Classes
                .FirstOrDefaultAsync(m => m.ClassId == id);

            // Osztály meglétének ellenőrzése
            if (@class == null) return NotFound();

            try
            {
                // Archiválás értékének módosítása
                @class.IsArchived = !@class.IsArchived;

                // Osztály értékeinek frissítése az adatbázisban
                _context.Update(@class);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClassExists(@class.ClassId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Felület újra megjelenítése
            return RedirectToAction(nameof(Index), new {status = !@class.IsArchived});
        }

        /// <summary>
        /// Osztály létrehozásának felülete
        /// </summary>
        public IActionResult Create()
        {
            // Lista elem betöltése
            ViewData["AuthorizerId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

            // Felület megjelenítése
            return View();
        }

        /// <summary>
        /// Osztály létrehozása az adatbázisban
        /// </summary>
        /// <param name="class">Osztály megadott adatai</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClassId,Name,AuthorizerId,IsArchived")] Class @class)
        {
            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                try
                {
                    // Osztály hozzáadása az adatbázishoz
                    _context.Add(@class);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(@class.ClassId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Felhasználó átírányítása az osztályok listájára
                return RedirectToAction(nameof(Index), new {status = true});
            }

            // Lista elem betöltése
            ViewData["AuthorizerId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

            // Felület újra megjelenítése, amennyiben hibás értékeket adott meg
            return View(@class);
        }

        /// <summary>
        /// Osztály szerkesztésének felülete
        /// </summary>
        /// <param name="id">Osztály azonosítója</param>
        public async Task<IActionResult> Edit(int? id)
        {
            // Azonosító meglétének ellenőrzése
            if (id == null || _context.Classes == null) return NotFound();

            // Osztály megkeresése az adatbázisban
            var @class = await _context.Classes.FindAsync(id);

            // Osztály meglétének ellenőrzése
            if (@class == null) return NotFound();

            // Lista elem betöltése
            ViewData["AuthorizerId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

            // Felület megjelenítése a kért osztály adataival
            return View(@class);
        }

        /// <summary>
        /// Ostály adatainak felülírása az adatbázisban
        /// </summary>
        /// <param name="id">Osztály azonosítja</param>
        /// <param name="class">Osztály megadott új értékei</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClassId,Name,AuthorizerId,IsArchived")] Class @class)
        {
            // Azonosító meglétének ellenőrzése
            if (id != @class.ClassId) return NotFound();

            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                try
                {
                    // Osztály hozzáadása az adatbázishoz
                    _context.Update(@class);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(@class.ClassId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Felhasználó átírányítása az osztályok listájára
                return RedirectToAction(nameof(Index), new {status = !@class.IsArchived});
            }

            // Lista elem betöltése
            ViewData["AuthorizerId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

            // Felület újra megjelenítése, amennyiben hibás értékeket adott meg
            return View(@class);
        }

        /// <summary>
        /// Osztály létezésének ellenőrzése
        /// </summary>
        /// <param name="id">Osztály azonosítója</param>
        /// <returns>Létezik e az osztály (igaz/hamis)</returns>
        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.ClassId == id);
        }
    }
}