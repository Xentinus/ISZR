using ISZR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISZR.Controllers
{
	public class RequestsController : Controller
	{
		private readonly DataContext _context;

		public RequestsController(DataContext context)
		{
			_context = context;
		}

		// GET: Requests
		public async Task<IActionResult> Index(string status, string type, int requestFor)
		{
			var dataContext = _context.Requests
				.Include(r => r.RequestAuthor)
				.Include(r => r.RequestFor)
				.OrderByDescending(r => r.RequestId)
				.AsQueryable();

			if (status != null && status != "Mind")
			{
				dataContext = dataContext.Where(r => r.Status == status);
				ViewBag.status = status;
			}
			if (type != null && type != "Mind")
			{
				dataContext = dataContext.Where(r => r.Type == type);
				ViewBag.type = type;
			}
			if (requestFor != 0 && requestFor.ToString() != "Mind")
			{
				dataContext = dataContext.Where(r => r.RequestForId == requestFor);
				ViewBag.requestForId = requestFor;
			}


			ViewData["RequestForId"] = new SelectList(_context.Users.OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			return View(await dataContext.ToListAsync());
		}

		// GET: Requests/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null || _context.Requests == null) return NotFound();

			var request = await _context.Requests
				.Include(r => r.RequestAuthor)
				.Include(r => r.RequestFor)
				.Include(r => r.RequestAuthor.Class)
				.Include(r => r.RequestAuthor.Position)
				.Include(r => r.RequestFor.Class)
				.Include(r => r.RequestFor.Position)
				.FirstOrDefaultAsync(m => m.RequestId == id);

			if (request == null) return NotFound();
			return View(request);
		}

		// GET: Meglévő felhasználó részére többletjogosultság
		public IActionResult UserAdditionalAccess()
		{
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			ViewData["Windows"] = new MultiSelectList(_context.Permissions.Where(p => p.Type == "Windows").OrderBy(p => p.Name), "ActiveDirectoryPermissions", "Name");
			ViewData["Fonix3"] = new MultiSelectList(_context.Permissions.Where(p => p.Type == "Főnix 3").OrderBy(p => p.Name), "Name", "Name");
			return View();
		}

		// POST: Meglévő felhasználó részére többletjogosultság
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UserAdditionalAccess(string[] windowsPermissions, string[] fonix3Permissions, [Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
			if (ModelState.IsValid)
			{
				request.RequestAuthorId = await RequestAuthorId();
				request.CreationDate = DateTime.Now;
				request.Type = "Meglévő felhasználó részére többletjogosultság";
				request.Status = "Folyamatban";

				// Windows jogosultságok hozzáadása
				foreach (string permission in windowsPermissions)
				{
					request.WindowsPermissions += permission[permission.Length - 1] == ';' ? $"{permission} " : $"{permission}; ";
				}

				// Főnix 3 jogosultságok hozzáadása
				foreach (string permission in fonix3Permissions)
				{
					request.FonixPermissions += $"{permission}; ";
				}

				request.Description = "Kérem engedélyezni a felhasználó részére többletjogosultság kiadását, a bv.hu tartományi rendszerben üzemelő szolgáltatások használatához.";
				_context.Add(request);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}
			ViewData["RequestForId"] = new SelectList(_context.Users.OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			return View();
		}

		// GET: Meglévő felhasználó részére e-mail cím igénylése
		public IActionResult Email()
		{
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			return View();
		}

		// POST: Meglévő felhasználó részére e-mail cím igénylése
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Email([Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
			if (ModelState.IsValid)
			{
				request.RequestAuthorId = await RequestAuthorId();
				request.CreationDate = DateTime.Now;
				request.Type = "Meglévő felhasználó részére e-mail cím igénylése";
				request.Status = "Folyamatban";

				request.Description = "Kérem engedélyezni a felhasználó részére e-mail cím elkészítését, a bv.hu tartományi rendszerben üzemelő szolgáltatások használatához.";
				_context.Add(request);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}
			ViewData["RequestForId"] = new SelectList(_context.Users.OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			return View();
		}

		// GET: Meglévő felhasználó részére telefonos PIN kód igénylése
		public IActionResult Phone()
		{
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			return View();
		}

		// POST: Meglévő felhasználó részére telefonos PIN kód igénylése
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Phone([Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
			if (ModelState.IsValid)
			{
				request.RequestAuthorId = await RequestAuthorId();
				request.CreationDate = DateTime.Now;
				request.Type = "Meglévő felhasználó részére telefonos PIN kód igénylése";
				request.Status = "Folyamatban";

				request.Description = "Kérem engedélyezni telefonos PIN kód kiadását a felhasználó részére, a bv.hu tartományi rendszerben üzemelő szolgáltatások használatához.";
				_context.Add(request);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}
			ViewData["RequestForId"] = new SelectList(_context.Users.OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			return View();
		}

		// GET: Meglévő felhasználó részére parkolási engedély igénylése
		public IActionResult Parking()
		{
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			return View();
		}

		// POST: Meglévő felhasználó részére parkolási engedély igénylése
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Parking([Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
			if (ModelState.IsValid)
			{
				request.RequestAuthorId = await RequestAuthorId();
				request.CreationDate = DateTime.Now;
				request.Type = "Meglévő felhasználó részére telefonos PIN kód igénylése";
				request.Status = "Folyamatban";

				request.Description = "Kérem engedélyezni telefonos PIN kód kiadását a felhasználó részére, a bv.hu tartományi rendszerben üzemelő szolgáltatások használatához.";
				_context.Add(request);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}
			ViewData["RequestForId"] = new SelectList(_context.Users.OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			return View();
		}

		private bool RequestExists(int id)
		{
			return _context.Requests.Any(e => e.RequestId == id);
		}

		private async Task<int> RequestAuthorId()
		{
			// Get username from pc
			string? activeUsername = User.Identity?.Name;
			if (activeUsername == null) return -1;

			// Looking for user
			var foundUser = await _context.Users
				.Include(u => u.Class)
				.Include(u => u.Position)
				.FirstOrDefaultAsync(m => m.Username == activeUsername);

			if (foundUser == null) return -1;

			// Return user id
			return foundUser.UserId;
		}
	}
}