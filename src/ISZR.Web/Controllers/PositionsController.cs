using ISZR.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISZR.Web.Controllers
{
    /// <summary>
    /// /Positions/? Controller
    /// </summary>
    [Authorize(Policy = "Administrator")]
    public class PositionsController : Controller
    {
        private readonly DataContext _context;

        public PositionsController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Beosztások listájának megjelenítése
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Beosztások listájának lekérdezése
            var dataContext = _context.Positions.OrderBy(u => u.Name);

            // Felület megjelenítése a kért listával
            return View(await dataContext.ToListAsync());
        }

        /// <summary>
        /// Beosztás archiválási állapotának megváltoztatása
        /// </summary>
        /// <param name="id">Beosztás azonosítója</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int? id)
        {
            // Azonosító meglétének ellenőrzése
            if (id == null || _context.Positions == null) return NotFound();

            // Beosztás megkeresése az adatbázisban
            var position = await _context.Positions
                .FirstOrDefaultAsync(m => m.PositionId == id);

            // Beosztás meglétének ellenőrzése
            if (position == null) return NotFound();

            try
            {
                // Archiválás értékének módosítása
                position.IsArchived = !position.IsArchived;

                // Beosztás értékeinek frissítése az adatbázisban
                _context.Update(position);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PositionExists(position.PositionId))
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
        /// Beosztás létrehozásának felülete
        /// </summary>
        public IActionResult Create()
        {
            // Felület megjelenítése
            return View();
        }

        /// <summary>
        /// Beosztás létrehozása az adatbázisban
        /// </summary>
        /// <param name="position">Beosztás megadott új értékei</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PositionId,Name,IsArchived")] Position position)
        {
            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                try
                {
                    // Beosztás hozzáadása az adatbázishoz
                    _context.Add(position);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PositionExists(position.PositionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Felhasználó átírányítása a beosztások listájára
                return RedirectToAction(nameof(Index));
            }

            // Felület újra megjelenítése, amennyiben az értékek hibásan lettek megadva
            return View(position);
        }

        /// <summary>
        /// Beosztás értékeinek felülírása az adatbázisban felület
        /// </summary>
        /// <param name="id">Beosztás azonosítója</param>
        public async Task<IActionResult> Edit(int? id)
        {
            // Azonosító meglétének ellenőrzése
            if (id == null || _context.Positions == null) return NotFound();

            // Beosztás megkeresése az adatbázisban
            var position = await _context.Positions.FindAsync(id);

            // Beosztás meglétének ellenőrzése
            if (position == null) return NotFound();

            // Felület megjelenítése a kért beosztás értékeivel
            return View(position);
        }

        /// <summary>
        /// Beosztás értékeinek felülírása az adatbázisban
        /// </summary>
        /// <param name="id">Beosztás azonosítója</param>
        /// <param name="position">Beosztás új értékei</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PositionId,Name,IsArchived")] Position position)
        {
            // Azonosító meglétének ellenőrzése
            if (id != position.PositionId) return NotFound();

            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                try
                {
                    // Beosztás értékeinek frissítése az adatbázisban
                    _context.Update(position);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PositionExists(position.PositionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Felhasználó átírányítása a beosztások listájára
                return RedirectToAction(nameof(Index));
            }

            // Felület újra megjelenítése, amennyiben a kért értékek hibásak
            return View(position);
        }

        /// <summary>
        /// Beosztás létének ellenőrzése
        /// </summary>
        /// <param name="id">Beosztás azonosítója</param>
        /// <returns>Létezik e a kért beosztás (igaz/hamis)</returns>
        private bool PositionExists(int id)
        {
            return _context.Positions.Any(e => e.PositionId == id);
        }
    }
}