using ISZR.Web.Components;
using ISZR.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ISZR.Web.Controllers
{
	public class PermissionsController : Controller
	{
		private readonly DataContext _context;

		public PermissionsController(DataContext context)
		{
			_context = context;
		}

		// GET: Permissions
		public async Task<IActionResult> Index()
		{
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Admin jogosultság ellenőrzése
			if (!Account.IsAdmin()) return Forbid();

			// Dashboard informations
			ViewBag.Permissions = _context.Permissions.Count();
			ViewBag.WindowsPermissions = _context.Permissions.Where(r => r.Type == "Windows").Count();
			ViewBag.Fonix3Permissions = _context.Permissions.Where(r => r.Type == "Főnix 3").Count();

            var dataContext = _context.Permissions.OrderBy(u => u.Name);
            return View(await dataContext.ToListAsync());
		}

		// GET: Permissions/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Admin jogosultság ellenőrzése
			if (!Account.IsAdmin()) return Forbid();

			if (id == null || _context.Permissions == null)
			{
				return NotFound();
			}

			var permission = await _context.Permissions
				.FirstOrDefaultAsync(m => m.PermissionId == id);
			if (permission == null)
			{
				return NotFound();
			}

			return View(permission);
		}

		// GET: Permissions/Create
		public IActionResult Create(string? type)
		{
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Admin jogosultság ellenőrzése
			if (!Account.IsAdmin()) return Forbid();

			if (type == null) return NotFound();
			ViewData["type"] = type;
			return View();
		}

		// POST: Permissions/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Name,Type,Description,ActiveDirectoryPermissions")] Permission permission)
		{
			if (ModelState.IsValid)
			{
				_context.Add(permission);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(permission);
		}

		// GET: Permissions/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Admin jogosultság ellenőrzése
			if (!Account.IsAdmin()) return Forbid();

			if (id == null || _context.Permissions == null) return NotFound();
			var permission = await _context.Permissions.FindAsync(id);
			if (permission == null) return NotFound();
			return View(permission);
		}

		// POST: Permissions/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("PermissionId,Name,Type,Description,ActiveDirectoryPermissions")] Permission permission)
		{
			if (id != permission.PermissionId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
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
				return RedirectToAction(nameof(Index));
			}
			return View(permission);
		}

		private bool PermissionExists(int id)
		{
			return _context.Permissions.Any(e => e.PermissionId == id);
		}
	}
}