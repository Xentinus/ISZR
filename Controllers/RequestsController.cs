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
		public async Task<IActionResult> Index()
		{
			// Dashboard informations
			ViewBag.All = _context.Requests.Count();

			// Dashboard informations
			ViewBag.All = _context.Requests.Count();
			ViewBag.InProgress = _context.Requests.Where(r => r.Status == "Folyamatban").Count();
			ViewBag.Done = _context.Requests.Where(r => r.Status == "Végrehajtva").Count();
			ViewBag.Denied = _context.Requests.Where(r => r.Status == "Elutasítva").Count();

			var dataContext = _context.Requests.Include(r => r.RequestAuthor).Include(r => r.RequestFor).OrderByDescending(r => r.RequestId);
			return View(await dataContext.ToListAsync());
		}

		// GET: Meglévő felhasználó részére többletjogosultság
		public IActionResult Tobbletjogosultsag()
		{
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			ViewData["Windows"] = new MultiSelectList(_context.Permissions.Where(p => p.Type == "Windows").OrderBy(p => p.Name), "ActiveDirectoryPermissions", "Name");
			ViewData["Fonix3"] = new MultiSelectList(_context.Permissions.Where(p => p.Type == "Főnix 3").OrderBy(p => p.Name), "Name", "Name");
			return View();
		}

		// POST: Meglévő felhasználó részére többletjogosultság
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Tobbletjogosultsag(string[] Windows, string[] Fonix3, [Bind("RequestId,Type,Status,Description,RequestedPermissions,RequestAuthorId,RequestForId")] Request request)
		{
			if (ModelState.IsValid)
			{
				request.RequestAuthorId = await RequestAuthorId();
				request.CreationDate = DateTime.Now;
				request.Type = "Meglévő felhasználó részére többletjogosultság";
				request.Status = "Folyamatban";

				// Windows jogosultságok hozzáadása
				foreach (string permission in Windows)
				{
					request.WindowsPermissions += $"{permission}; ";
				}

				// Főnix 3 jogosultságok hozzáadása
				foreach (string name in Fonix3)
				{
					request.FonixPermissions += $"{name}; ";
				}

				request.Description = "Kérem engedélyezni a felhasználó részére többletjogosultság kiadását, a bv.hu tartományi rendszerben üzemelő szolgáltatások használatához.";
				_context.Add(request);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}
			ViewData["RequestForId"] = new SelectList(_context.Users.OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			return View();
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