using ISZR.Web.Models;
using ISZR.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;

namespace ISZR.Web.Controllers
{
    /// <summary>
    /// /Groups/? Controller
    /// </summary>
    [Authorize(Policy = "Administrator")]
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
        public async Task<IActionResult> Index(string name, bool status, int? pageNumber)
        {
            // Értékek beállítása
            ViewData["name"] = name;
            ViewData["status"] = status;

            // Csoportok listájának lekérdezése
            var dataContext = _context.Groups
                .OrderBy(g => g.Name)
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

            // Felület megjelenítése a kért listával
            return View(await PaginatedList<Group>.CreateAsync(dataContext, pageNumber ?? 1));
        }

        /// <summary>
        /// Csoport archiválási állapotának megváltoztatása
        /// </summary>
        /// <param name="id">Csoport azonosító</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int? pageNumber, int? id)
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
            return RedirectToAction(nameof(Index), new { status = !group.IsArchived });
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
            var group = await _context.Groups
                .Include(g => g.GroupPermissions)
                .ThenInclude(gp => gp.Permission)
                .FirstOrDefaultAsync(g => g.GroupId == id);

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
            // Igényelhető jogosultságok kilistázása
            var viewModel = new GroupViewModel
            {
                PermissionItems = _context.Permissions
                    .Where(p => !p.IsArchived)
                    .Select(p => new SelectListItem { Value = p.PermissionId.ToString(), Text = p.Name })
            };

            // Felület megjelenítése
            return View(viewModel);
        }

        /// <summary>
        /// Csoport létrehozása a rendszerben
        /// </summary>
        /// <param name="viewModel">Megadott csoport adatai</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupViewModel viewModel)
        {
            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                var group = new Group
                {
                    Name = viewModel.Name,
                    GroupPermissions = viewModel.SelectedPermissionIds
                        .Select(permissionId => new GroupPermission { PermissionId = permissionId })
                        .ToList()
                };

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
                return RedirectToAction(nameof(Index), new { status = true });
            }

            // Kiválaszható jogosultságok
            viewModel.PermissionItems = _context.Permissions
                .Where(p => !p.IsArchived)
                .Select(p => new SelectListItem { Value = p.PermissionId.ToString(), Text = p.Name });

            // Felület megjelenítése amennyiben hibásan lettek megadva az adatok
            return View(viewModel);
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
            var group = await _context.Groups
                .Include(g => g.GroupPermissions)
                .FirstOrDefaultAsync(g => g.GroupId == id);

            // Csoport meglétének ellenőrzése
            if (group == null) return NotFound();

            var viewModel = new GroupViewModel
            {
                GroupId = group.GroupId,
                Name = group.Name,
                SelectedPermissionIds = group.GroupPermissions.Select(gp => gp.PermissionId).ToList(),
                PermissionItems = _context.Permissions.Where(p => !p.IsArchived)
                    .Select(p => new SelectListItem { Value = p.PermissionId.ToString(), Text = p.Name })
            };

            // Felület megjelenítése a kért csoport adataival
            return View(viewModel);
        }

        /// <summary>
        /// Csoport adatainak szerkesztése
        /// </summary>
        /// <param name="viewModel">Megadott csoport adatok</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GroupViewModel viewModel)
        {
            var group = await _context.Groups
                        .Include(g => g.GroupPermissions)
                        .FirstOrDefaultAsync(g => g.GroupId == viewModel.GroupId);

            if (group == null)
            {
                return NotFound();
            }

            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                try
                {
                    group.Name = viewModel.Name;
                    group.GroupPermissions = viewModel.SelectedPermissionIds
                        .Select(permissionId => new GroupPermission { PermissionId = permissionId })
                        .ToList();

                    // Csoport adatainak felülírása
                    _context.Update(group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(viewModel.GroupId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Felhasználó átírányítása a csoportok listájára
                return RedirectToAction(nameof(Index), new { status = !group.IsArchived });
            }

            viewModel.PermissionItems = _context.Permissions
                .Where(p => !p.IsArchived)
                .Select(p => new SelectListItem { Value = p.PermissionId.ToString(), Text = p.Name });

            // Felület megjelenítése amennyiben hibás adatokat tartalmaz
            return View(viewModel);
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