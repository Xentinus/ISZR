using ISZR.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace ISZR.Web.Controllers
{
    /// <summary>
    /// /Groups/? Controller
    /// </summary>
    public class GroupsController : Controller
    {
        private readonly DataContext _context;

        public GroupsController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Csoportok megjelenítése
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Csoportok listájának lekérdezése
            var dataContext = _context.Groups.OrderBy(g => g.Name);

            // Felület megjelenítése a kért listával
            return View(await dataContext.ToListAsync());
        }

        /// <summary>
        /// Csoport archiválási állapotának megváltoztatása
        /// </summary>
        /// <param name="id">Csoport azonosító</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int? id)
        {
            // Azonosító meglétének ellenőrzése
            if (id == null || _context.Groups == null) return NotFound();

            // Csoport megkeresése az adatbázisban
            var group = await _context.Groups.FirstOrDefaultAsync(m => m.GroupId == id);

            // Csoport meglétének ellenőrzése
            if (group == null) return NotFound();

            try
            {
                // Archiválás értékének módosítása
                group.IsArchived = !group.IsArchived;

                // Csoport értékeinek frissítése az adatbázisban
                _context.Update(group);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupExists(group.GroupId))
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
        /// Adott csoport részletei
        /// </summary>
        /// <param name="id">Csoport azonosítója</param>
        public async Task<IActionResult> Details(int? id)
        {
            // Azonosító meglétének ellenőrzése
            if (id == null || _context.Groups == null) return NotFound();

            // Kért csoport kikeresése az adatbázisból
            var group = await _context.Groups.FirstOrDefaultAsync(m => m.GroupId == id);

            // Csoport meglétének ellenőrzése
            if (group == null) return NotFound();

            // Felület megjelenítése a kért csoporttal
            return View(group);
        }

        /// <summary>
        /// Csoport létrehozásának felülete
        /// </summary>
        public IActionResult Create()
        {
            // Lista elemek betöltése
            ViewData["Windows"] = new MultiSelectList(_context.Permissions.Where(p => p.Type == "Windows" && !p.IsArchived).OrderBy(p => p.Name), "ActiveDirectoryPermissions", "Name");
            ViewData["Fonix3"] = new MultiSelectList(_context.Permissions.Where(p => p.Type == "Főnix 3" && !p.IsArchived).OrderBy(p => p.Name), "Name", "Name");

            // Felület megjelenítése
            return View();
        }

        /// <summary>
        /// Csoport létrehozása a rendszerben
        /// </summary>
        /// <param name="windowsPermissions">Kért windows jogosultságok tömbben</param>
        /// <param name="fonix3Permissions">Kért főnix 3 jogosultságok tömbben</param>
        /// <param name="group">Megadott csoport adatai</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string[] windowsPermissions, string[] fonix3Permissions, [Bind("GroupId,Name,WindowsPermissions,FonixPermissions,IsArchived")] Group group)
        {
            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                // Windows jogosultságok Active-Directory értékeinek sorrendbe helyezése
                if (windowsPermissions.Length > 0) group.WindowsPermissions = PermissionList(windowsPermissions);

                // Főnix 3 jogosultságok neveinek sorrendbe helyezése
                if (fonix3Permissions.Length > 0) group.FonixPermissions = PermissionList(fonix3Permissions);

                try
                {
                    // Jogosultsági csoport hozzáadása a rendszerhez
                    _context.Add(group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(group.GroupId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Felhasználó átírányítása a csoportok listájára
                return RedirectToAction(nameof(Index));
            }

            // Lista elemek betöltése
            ViewData["Windows"] = new MultiSelectList(_context.Permissions.Where(p => p.Type == "Windows" && !p.IsArchived).OrderBy(p => p.Name), "ActiveDirectoryPermissions", "Name");
            ViewData["Fonix3"] = new MultiSelectList(_context.Permissions.Where(p => p.Type == "Főnix 3" && !p.IsArchived).OrderBy(p => p.Name), "Name", "Name");

            // Felület megjelenítése amennyiben hibásan lettek megadva az adatok
            return View(group);
        }

        /// <summary>
        /// Csoport adatainak szerkesztésének felülete
        /// </summary>
        /// <param name="id">Csoport azonosítója</param>
        public async Task<IActionResult> Edit(int? id)
        {
            // Csoport azonosító meglétének ellenőrzése
            if (id == null || _context.Groups == null) return NotFound();

            // Kért csoport megkeresése az adatbázisban
            var group = await _context.Groups.FindAsync(id);

            // Csoport meglétének ellenőrzése
            if (group == null) return NotFound();

            // Felület megjelenítése a kért csoport adataival
            return View(group);
        }

        /// <summary>
        /// Csoport adatainak szerkesztése
        /// </summary>
        /// <param name="id">Csoport azonosítója</param>
        /// <param name="group">Csoport megadott új értékei</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GroupId,Name,WindowsPermissions,FonixPermissions,IsArchived")] Group group)
        {
            // Azonosító meglétének ellenőrzése
            if (id != group.GroupId) return NotFound();

            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                // Null érték megtartása amennyiben üres a mező
                group.WindowsPermissions = group.WindowsPermissions == "" ? null : group.WindowsPermissions;
                group.FonixPermissions = group.FonixPermissions == "" ? null : group.FonixPermissions;

                try
                {
                    // Csoport adatainak felülírása
                    _context.Update(group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(group.GroupId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Felhasználó átírányítása a csoportok listájára
                return RedirectToAction(nameof(Index));
            }

            // Felület megjelenítése amennyiben hibás adatokat tartalmaz
            return View(group);
        }

        /// <summary>
        /// Jogosultságok sorrendbe helyezése elválasztással
        /// </summary>
        /// <param name="permissions">Jogosultságokat tartalmazó string tömb</param>
        /// <returns>Egy stringként adja vissza ;-al elválasztva a jogosutlságokat</returns>
        private string PermissionList(string[] permissions)
        {
            StringBuilder permissionList = new StringBuilder();
            foreach (string permission in permissions) permissionList.Append(permission[permission.Length - 1] == ';' ? $"{permission} " : $"{permission}; ");
            return permissionList.ToString();
        }

        /// <summary>
        /// Csoport meglétének ellenőrzése
        /// </summary>
        /// <param name="id">Csoport azonosítója</param>
        /// <returns>Létezik e az adott csoport (igaz/hamis)</returns>
        private bool GroupExists(int id)
        {
            return _context.Groups.Any(e => e.GroupId == id);
        }
    }
}