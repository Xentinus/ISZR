using ISZR.Web.Components;
using ISZR.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ISZR.Web.Controllers
{
	/// <summary>
	/// /Permissions/? Controller
	/// </summary>
	public class PermissionsController : Controller
	{
		private readonly DataContext _context;

		public PermissionsController(DataContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Jogosultságok listájának megjelenítése
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> Index()
		{
			// Admin jogosultság ellenőrzése
			if (!Account.IsAdmin()) return Forbid();

			// Jogosultságok listájának lekérdezése
			var dataContext = _context.Permissions.OrderBy(u => u.Name);

			// Felület megjelenítése a kért listával
			return View(await dataContext.ToListAsync());
		}

		/// <summary>
		/// Jogosultság archiválási állapotának megváltoztatása
		/// </summary>
		/// <param name="id">Jogosultság azonosítója</param>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(int? id)
		{
			// Azonosító meglétének ellenőrzése
			if (id == null || _context.Permissions == null) return NotFound();

			// Jogosultság megkeresése az adatbázisban
			var permission = await _context.Permissions
				.FirstOrDefaultAsync(m => m.PermissionId == id);

			// Jogosultság meglétének ellenőrzése
			if (permission == null) return NotFound();

			try
			{
				// Archiválás értékének módosítása
				permission.IsArchived = !permission.IsArchived;

				// Jogosultság értékeinek frissítése az adatbázisban
				_context.Update(permission);
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!PermissionExists(permission.PermissionId))
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
		/// Adott jogosultság értékeinek megjelenítése
		/// </summary>
		/// <param name="id">Jogosultság azonosítója</param>
		public async Task<IActionResult> Details(int? id)
		{
			// Admin jogosultság ellenőrzése
			if (!Account.IsAdmin()) return Forbid();

			// Azonosítók meglétének ellenőrzése
			if (id == null || _context.Permissions == null) return NotFound();

			// Jogosultság megkeresése az adatbázisban
			var permission = await _context.Permissions.FirstOrDefaultAsync(m => m.PermissionId == id);

			// Jogosultság meglétének ellenőrzése
			if (permission == null) return NotFound();

			// Felület megjelenítése a kért osztály értékeivel
			return View(permission);
		}

		/// <summary>
		/// Jogosultság létrehozásának felülete
		/// </summary>
		/// <param name="type">Jogosultsági típus</param>
		public IActionResult Create(string? type)
		{
			// Admin jogosultság ellenőrzése
			if (!Account.IsAdmin()) return Forbid();

			// Típus meglétének ellenőrzése
			if (type == null) return NotFound();

			// Típus beállítása
			ViewData["type"] = type;

			// Felület megjelenítése típus alapján
			return View();
		}

		/// <summary>
		/// Jogosultság létrehozása az adatbázisban
		/// </summary>
		/// <param name="permission">Jogosultság új értékei</param>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("PermissionId,Name,Type,Description,ActiveDirectoryPermissions,IsArchived")] Permission permission)
		{
			// Megadott értékek ellenőrzése
			if (ModelState.IsValid)
			{
				try
				{
					// Jogosultság hozzáadása az adatbázishoz
					_context.Add(permission);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!PermissionExists(permission.PermissionId))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}

				// Felhasználó átírányítása a jogosultságok listájához
				return RedirectToAction(nameof(Index));
			}

			// Felület újra megjelenítése, amennyiben a megadott értékek hibásak
			return View(permission);
		}

		/// <summary>
		/// Jogosultság értékeinek szerkesztésének felülete
		/// </summary>
		/// <param name="id">Jogosultsági azonosító</param>
		public async Task<IActionResult> Edit(int? id)
		{
			// Admin jogosultság ellenőrzése
			if (!Account.IsAdmin()) return Forbid();

			// Azonosító meglétének ellenőrzése
			if (id == null || _context.Permissions == null) return NotFound();

			// Jogosultság megkeresése az adatbázisban
			var permission = await _context.Permissions.FindAsync(id);

			// Jogosultság meglétének ellenőrzése
			if (permission == null) return NotFound();

			// Felület megjelenítése a kért jogosultság értékeivel
			return View(permission);
		}

		/// <summary>
		/// Jogosultság értékeinek szerkesztése az adatbázisban
		/// </summary>
		/// <param name="id">Jogosultsági azonosító</param>
		/// <param name="permission">Jogosultság új értékei</param>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("PermissionId,Name,Type,Description,ActiveDirectoryPermissions,IsArchived")] Permission permission)
		{
			// Azonosító meglétének ellenőrzése
			if (id != permission.PermissionId) return NotFound();

			// Megadott értékek ellenőrzése
			if (ModelState.IsValid)
			{
				try
				{
					// Jogosultság értékeinek felülírása az adatbázisban
					_context.Update(permission);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!PermissionExists(permission.PermissionId))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}

				// Felhasználó átírányítása a jogosultságok listájára
				return RedirectToAction(nameof(Index));
			}

			// Felület újra megjelenítése, amennyiben a kért értékek hibásak
			return View(permission);
		}

		/// <summary>
		/// Jogosultság meglétének ellenőrzése az adatbázisban
		/// </summary>
		/// <param name="id">Jogosultsági azonosító</param>
		/// <returns>Létezi e a kért jogosultság (igaz/hamis)</returns>
		private bool PermissionExists(int id)
		{
			return _context.Permissions.Any(e => e.PermissionId == id);
		}
	}
}