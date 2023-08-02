using ISZR.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CSharp.RuntimeBinder;

namespace ISZR.Web.Controllers
{
    /// <summary>
    /// /Phones/? Controller
    /// </summary>
    [Authorize(Policy = "Administrator")]
    public class PhonesController : Controller
    {
        private readonly DataContext _context;

        public PhonesController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// PIN kódok megjelenítése
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // PIN kódok listájának lekérdezése
            var dataContext = _context.Phones.Where(p => !p.IsArchived).Include(p => p.PhoneUser).Include(p => p.PhoneUser.Position).OrderBy(p => p.PhoneCode);

            // Felület megjelenítése a kért listával
            return View(await dataContext.ToListAsync());
        }

        /// <summary>
        /// PIN kód archiválási állapotának megváltoztatása
        /// </summary>
        /// <param name="id">PIN kód azonosítója</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int? id)
        {
            // Azonosító meglétének ellenőrzése
            if (id == null || _context.Phones == null) return NotFound();

            // PIN kód megkeresése az adatbázisban
            var pin = await _context.Phones.FirstOrDefaultAsync(m => m.PhoneId == id);

            // PIN kód meglétének ellenőrzése
            if (pin == null) return NotFound();

            try
            {
                // Archiválás értékének módosítása
                pin.IsArchived = !pin.IsArchived;

                // PIN kód értékeinek frissítése az adatbázisban
                _context.Update(pin);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhoneExists(pin.PhoneId))
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
        /// PIN kód létrehozásának felülete
        /// </summary>
        public IActionResult Create()
        {
            // Lista elemek betöltése
            try
            {
                ViewData["PhoneUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
                {
                    u.UserId,
                    DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
                }), "UserId", "DisplayText");
            }
            catch (RuntimeBinderException ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Felület megjelenítése
            return View();
        }

        /// <summary>
        /// PIN kód létrehozása a rendszerben
        /// </summary>
        /// <param name="phone">Megadott PIN kód adatai</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PhoneId,PhoneCode,PhoneUserId,IsArchived")] Phone phone)
        {
            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(phone);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhoneExists(phone.PhoneId))
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
            try
            {
                ViewData["PhoneUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
                {
                    u.UserId,
                    DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
                }), "UserId", "DisplayText");
            }
            catch (RuntimeBinderException ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Felület megjelenítése amennyiben hibásan lettek megadva az adatok
            return View(phone);
        }

        /// <summary>
        /// PIN kód szerkesztésének felülete
        /// </summary>
        /// <param name="id">PIN kód azonosítója</param>
        public async Task<IActionResult> Edit(int? id)
        {
            // PIN kód azonosítójánakmeglétének ellenőrzése
            if (id == null || _context.Phones == null) return NotFound();

            // Kért PIN kód megkeresése az adatbázisban
            var phone = await _context.Phones.FindAsync(id);

            // PIN kód meglétének ellenőrzése
            if (phone == null) return NotFound();

            // Lista elemek betöltése
            try
            {
                ViewData["PhoneUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
                {
                    u.UserId,
                    DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
                }), "UserId", "DisplayText");
            }
            catch (RuntimeBinderException ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Felület megjelenítése a kért PIN kód adataival
            return View(phone);
        }

        /// <summary>
        /// PIN kód adatainak szerkesztése
        /// </summary>
        /// <param name="id">PIN kód azonosítója</param>
        /// <param name="phone">Megadott PIN kód új értékei</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PhoneId,PhoneCode,PhoneUserId,IsArchived")] Phone phone)
        {
            // Azonosító meglétének ellenőrzése
            if (id != phone.PhoneId) return NotFound();

            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                try
                {
                    // PIN kód adatainak felülírása
                    _context.Update(phone);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhoneExists(phone.PhoneId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Felhasználó átirányítása a PIN kódok listájára
                return RedirectToAction(nameof(Index));
            }

            // Lista elemek betöltése
            try
            {
                ViewData["PhoneUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
                {
                    u.UserId,
                    DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
                }), "UserId", "DisplayText");
            }
            catch (RuntimeBinderException ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Felület megjelenítése amennyiben hibás adatokat tartalmaz
            return View(phone);
        }

        /// <summary>
        /// PIN kód meglétének ellenőrzése
        /// </summary>
        /// <param name="id">PIN kód azonosítója</param>
        /// <returns>Létezik e az adott PIN kód (igaz/hamis)</returns>
        private bool PhoneExists(int id)
        {
            return (_context.Phones?.Any(e => e.PhoneId == id)).GetValueOrDefault();
        }
    }
}