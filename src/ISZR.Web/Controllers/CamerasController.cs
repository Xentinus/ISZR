using ISZR.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISZR.Web.Controllers
{
    /// <summary>
    /// /Cameras/? Controller
    /// </summary>
    [Authorize(Policy = "Administrator")]
    public class CamerasController : Controller
    {
        private readonly DataContext _context;

        public CamerasController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Kamerák listájának megjelenítése
        /// </summary>
        public async Task<IActionResult> Index(string name, bool status, int? pageNumber)
        {
            // Értékek beállítása
            ViewData["name"] = name;
            ViewData["status"] = status;

            // Kamera lista elkészítése
            var dataContext = _context.Cameras
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

            // Igénylési lista összeállítása
            await dataContext.ToListAsync();
            ViewData["dataLength"] = dataContext.Count();

            // Felület megjelnítése a listával
            return View(await PaginatedList<Camera>.CreateAsync(dataContext, pageNumber ?? 1));
        }

        /// <summary>
        /// Kamera archiválási állapotának megváltoztatása
        /// </summary>
        /// <param name="id">Kamera azonosítójó</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int? pageNumber, int? id)
        {
            // Azonosító meglétének ellenőrzése
            if (id == null || _context.Cameras == null) return NotFound();

            // Kamera megkeresése az adatbázisban
            var camera = await _context.Cameras
                .FirstOrDefaultAsync(m => m.CameraId == id);

            // Kamera meglétének ellenőrzése
            if (camera == null) return NotFound();

            try
            {
                // Archiválás értékének módosítása
                camera.IsArchived = !camera.IsArchived;

                // Kamera értékeinek frissítése az adatbázisban
                _context.Update(camera);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CameraExists(camera.CameraId))
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
        /// Kamera hozzáadásának felülete
        /// </summary>
        public IActionResult Create()
        {
            // Felület megjelenítése
            return View();
        }

        /// <summary>
        /// Kamera hozzáadása a rendszerhez
        /// </summary>
        /// <param name="camera">Kamera új értékei</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CameraId,Name,IsArchived")] Camera camera)
        {
            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                try
                {
                    // Kamera hozzáadása a rendszerhez
                    _context.Add(camera);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CameraExists(camera.CameraId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Felhasználó átírányítása a kamerák listájára
                return RedirectToAction(nameof(Index));
            }

            // Hibásan megadott értékek esetén, felület újra megjelenítése
            return View(camera);
        }

        /// <summary>
        /// Kamera szerkesztésének felülete
        /// </summary>
        /// <param name="id">Kamera azonosítója</param>
        public async Task<IActionResult> Edit(int? id)
        {
            // Azonosító meglétének ellenőrzése
            if (id == null || _context.Cameras == null) return NotFound();

            // Kamera megkeresése az adatbázisban
            var camera = await _context.Cameras.FindAsync(id);

            // Kamera meglétének ellenőrzése
            if (camera == null) return NotFound();

            // Felület megjelenítése a kért kamerával
            return View(camera);
        }

        /// <summary>
        /// Kamera módosítása a megadott értékek alapján
        /// </summary>
        /// <param name="id">Kamera azonosítója</param>
        /// <param name="camera">Kamera megadott új értékei</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CameraId,Name,IsArchived")] Camera camera)
        {
            // Azonosítók ellenőrzése
            if (id != camera.CameraId) return NotFound();

            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                try
                {
                    // Kamera értékeinek frissítése az adatbázisban
                    _context.Update(camera);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CameraExists(camera.CameraId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Felhasználó átirányítása a kamerák listájára
                return RedirectToAction(nameof(Index));
            }

            // Felület megjelenítése
            return View(camera);
        }

        /// <summary>
        /// Kamera meglétének ellenőrzése
        /// </summary>
        /// <param name="id">Kamera azonosítója</param>
        /// <returns>Létezik e az adott kamera (igaz/hamis)</returns>
        private bool CameraExists(int id)
        {
            return _context.Cameras.Any(e => e.CameraId == id);
        }
    }
}