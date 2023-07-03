using ISZR.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISZR.Web.Controllers
{
    /// <summary>
    /// /Parkings/? Controller
    /// </summary>
    [Authorize(Policy = "Administrator")]
    public class ParkingsController : Controller
    {
        private readonly DataContext _context;

        public ParkingsController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Parkolási engedélyek megjelenítése
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Parkolási engedélyek listájának lekérdezése
            var dataContext = _context.Parkings.Include(p => p.OwnerUser).OrderBy(p => p.OwnerUser.DisplayName);

            // Felület megjelenítése a kért listával
            return View(await dataContext.ToListAsync());
        }

        /// <summary>
        /// Parkolási engedély archiválási állapotának megváltoztatása
        /// </summary>
        /// <param name="id">Parkolási engedélyhez tartozó azonosító</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int? id)
        {
            // Azonosító meglétének ellenőrzése
            if (id == null || _context.Parkings == null) return NotFound();

            // Parkolási engedély megkeresése az adatbázisban
            var vehicle = await _context.Parkings.FirstOrDefaultAsync(m => m.ParkingId == id);

            // Parkolási engedély meglétének ellenőrzése
            if (vehicle == null) return NotFound();

            try
            {
                // Archiválás értékének módosítása
                vehicle.IsArchived = !vehicle.IsArchived;

                // Parkolási engedély értékeinek frissítése az adatbázisban
                _context.Update(vehicle);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParkingExists(vehicle.ParkingId))
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
        /// Parkolási engedély létrehozásának felülete
        /// </summary>
        public IActionResult Create()
        {
            // Lista elemek betöltése
            ViewData["OwnerUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

            // Felület megjelenítése
            return View();
        }

        /// <summary>
        /// Parkolási engedély létrehozása a rendszerben
        /// </summary>
        /// <param name="parking">Megadott parkolási engedély adatai</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ParkingId,Brand,Modell,LicensePlate,OwnerUserId,IsArchived")] Parking parking)
        {
            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(parking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParkingExists(parking.ParkingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Felhasználó átirányítása a parkolási engedélyek listájára
                return RedirectToAction(nameof(Index));
            }

            // Lista elemek betöltése
            ViewData["OwnerUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

            // Felület megjelenítése amennyiben hibásan lettek megadva az adatok
            return View(parking);
        }

        /// <summary>
        /// Parkolási engedély szerkesztésének felülete
        /// </summary>
        /// <param name="id">Parkolási engedély azonosítója</param>
        public async Task<IActionResult> Edit(int? id)
        {
            // Parkolási engedély azonosítójának meglétének ellenőrzése
            if (id == null || _context.Parkings == null) return NotFound();

            // Kért parkolási engedély megkeresése az adatbázisban
            var parking = await _context.Parkings.FindAsync(id);

            // Parkolási engedély meglétének ellenőrzése
            if (parking == null) return NotFound();

            // Lista elemek betöltése
            ViewData["OwnerUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

            // Felület megjelenítése a kért parkolási engedély adataival
            return View(parking);
        }

        /// <summary>
        /// Parkolási engedély adatainak szerkesztése
        /// </summary>
        /// <param name="id">Parkolási engedély azonosítója</param>
        /// <param name="parking">Megadott parkolási engedély új értékei</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ParkingId,Brand,Modell,LicensePlate,OwnerUserId,IsArchived")] Parking parking)
        {
            // Azonosító meglétének ellenőrzése
            if (id != parking.ParkingId) return NotFound();

            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                try
                {
                    // Parkolási engedély adatainak felülírása
                    _context.Update(parking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParkingExists(parking.ParkingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Felhasználó átirányítása a parkolási engedélyek listájára
                return RedirectToAction(nameof(Index));
            }

            // Lista elemek betöltése
            ViewData["OwnerUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

            // Felület megjelenítése amennyiben hibás adatokat tartalmaz
            return View(parking);
        }

        /// <summary>
        /// Parkolási engedély meglétének ellenőrzése
        /// </summary>
        /// <param name="id">Parkolási engedély azonosítója</param>
        /// <returns>Létezik e az adott parkolási engedély (igaz/hamis)</returns>
        private bool ParkingExists(int id)
        {
            return (_context.Parkings?.Any(e => e.ParkingId == id)).GetValueOrDefault();
        }
    }
}