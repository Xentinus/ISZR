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
        public async Task<IActionResult> Index(string name, string license, bool status, int? pageNumber)
        {
            // Értékek beállítása
            ViewData["name"] = name;
            ViewData["license"] = license;
            ViewData["status"] = status;

            // Parkolási engedélyek listájának lekérdezése
            var dataContext = _context.Parkings
                .Include(p => p.OwnerUser)
                .Include(p => p.OwnerUser.Position)
                .OrderBy(p => p.OwnerUser.DisplayName)
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
                dataContext = dataContext.Where(r => r.OwnerUser.DisplayName.Contains(name));
            }

            // Rendszám alapú szürés
            if (!string.IsNullOrEmpty(license))
            {
                dataContext = dataContext.Where(r => r.LicensePlate.Contains(license));
            }

            // Igénylési lista összeállítása
            await dataContext.ToListAsync();
            ViewData["dataLength"] = dataContext.Count();

            // Felület megjelenítése a kért listával
            return View(await PaginatedList<Parking>.CreateAsync(dataContext, pageNumber ?? 1));
        }

        /// <summary>
        /// Parkolási engedély archiválási állapotának megváltoztatása
        /// </summary>
        /// <param name="id">Parkolási engedélyhez tartozó azonosító</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int? pageNumber, int? id)
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
            return RedirectToAction(nameof(Index), new {status = !vehicle.IsArchived});
        }

        /// <summary>
        /// Parkolási engedély létrehozásának felülete
        /// </summary>
        public IActionResult Create()
        {
            // Lista elemek betöltése
            ViewData["OwnerUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

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
                return RedirectToAction(nameof(Index), new {status = true});
            }

            // Lista elemek betöltése
            ViewData["OwnerUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

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
            ViewData["OwnerUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

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
                return RedirectToAction(nameof(Index), new {status = !parking.IsArchived});
            }

            // Lista elemek betöltése
            ViewData["OwnerUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

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