using ISZR.Web.Components;
using ISZR.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISZR.Web.Controllers
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
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			var dataContext = _context.Requests
				.Include(r => r.RequestAuthor)
				.Include(r => r.RequestFor)
				.OrderByDescending(r => r.RequestId)
				.AsQueryable();

			// Amennyiben semmilyen érték nem létezik (vendég felhasználóknak, saját igénylések megtekintése)
			if (status == null && type == null && requestFor == 0)
			{
				int userId = await Account.GetUserId(_context);
				dataContext = dataContext.Where(r => r.RequestForId == userId);
			}
			else
			{
				// Szűrés csak ügyintézők számára engedélyezett
				if (!Account.IsUgyintezo()) return Forbid();

				// Státusz alapú szürés
				if (status != null && status != "Mind")
				{
					dataContext = dataContext.Where(r => r.Status == status);
					ViewBag.status = status;
				}

				// Típus alapú szürés
				if (type != null && type != "Mind")
				{
					dataContext = dataContext.Where(r => r.Type == type);
					ViewBag.type = type;
				}

				// Személy alapú szürés
				if (requestFor != 0 && requestFor.ToString() != "Mind")
				{
					dataContext = dataContext.Where(r => r.RequestForId == requestFor);
					ViewBag.requestForId = requestFor;
				}
			}

			// Maximális megengedett lista értéke 50
			dataContext = dataContext.Take(50);

			// Felhasználó alapú szűréshez lista
			ViewData["RequestForId"] = new SelectList(_context.Users.OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Az oldal megjelenítése az igénylésekkel
			return View(await dataContext.ToListAsync());
		}

		// GET: Requests/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Amennyiben nem adtak meg azonosítót, az oldal megjelenítésének elutasítása
			if (id == null || _context.Requests == null) return NotFound();

			// Adott azonosítójú kérelem kikeresése
			var request = await _context.Requests
				.Include(r => r.RequestAuthor)
				.Include(r => r.RequestFor)
				.Include(r => r.RequestAuthor.Class)
				.Include(r => r.RequestAuthor.Position)
				.Include(r => r.RequestFor.Class)
				.Include(r => r.RequestFor.Class.Authorizer)
				.Include(r => r.RequestFor.Class.Authorizer.Position)
				.Include(r => r.RequestFor.Position)
				.FirstOrDefaultAsync(m => m.RequestId == id);

			// Amennyiben a kért kérelem nem létezik, az oldal megjelenítésének elutasítása
			if (request == null) return NotFound();

			// Oldal megjelenítése a kért igényléssel
			return View(request);
		}

		// POST: Administrator/Details/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Details(int? id, string? status)
		{
			if (id == null || status == null || _context.Requests == null) return NotFound();

			var request = await _context.Requests
				.Include(r => r.RequestAuthor)
				.Include(r => r.RequestFor)
				.Include(r => r.RequestAuthor.Class)
				.Include(r => r.RequestAuthor.Position)
				.Include(r => r.RequestFor.Class)
				.Include(r => r.RequestFor.Position)
				.FirstOrDefaultAsync(m => m.RequestId == id);

			if (request == null) return NotFound();

			try
			{
				request.Status = status;
				if (status == "Folyamatban")
				{
					request.ResolveDate = new DateTime();
				}
				else
				{
					request.ResolveDate = DateTime.Now;
				}

				_context.Update(request);
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!RequestExists(request.RequestId))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

			return View(request);
		}

		// GET: Meglévő felhasználó részére többletjogosultság
		public async Task<IActionResult> UserAdditionalAccess()
		{
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Az oldalt csak ügyintézők tekinthetik meg
			if (!Account.IsUgyintezo()) return Forbid();

			// Lista elemek betöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			ViewData["Windows"] = new MultiSelectList(_context.Permissions.Where(p => p.Type == "Windows").OrderBy(p => p.Name), "ActiveDirectoryPermissions", "Name");
			ViewData["Fonix3"] = new MultiSelectList(_context.Permissions.Where(p => p.Type == "Főnix 3").OrderBy(p => p.Name), "Name", "Name");

			// Oldal megjelenítése
			return View();
		}

		// POST: Meglévő felhasználó részére többletjogosultság
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UserAdditionalAccess(string[] windowsPermissions, string[] fonix3Permissions, [Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
			// Megadott értékek ellenőrzése
			if (ModelState.IsValid)
			{
				// Igénylést létrehozó személy azonosítója
				request.RequestAuthorId = await RequestAuthorId();

				// Igénylés létrehozásának dátuma
				request.CreationDate = DateTime.Now;

				// Igénylés típusa
				request.Type = "Meglévő felhasználó részére többletjogosultság";

				// Alapértelmezett státusz
				request.Status = "Folyamatban";

				// Igénylés leírása
				request.Description = "Kérem engedélyezni a felhasználó részére többletjogosultság kiadását, a bv.hu tartományi rendszerben üzemelő szolgáltatások használatához.";

				// Windows jogosultságok Active-Directory értékeinek sorrendbe helyezése
				foreach (string permission in windowsPermissions)
				{
					request.WindowsPermissions += permission[permission.Length - 1] == ';' ? $"{permission} " : $"{permission}; ";
				}

				// Főnix 3 jogosultságok neveinek sorrendbe helyezése
				foreach (string permission in fonix3Permissions)
				{
					request.FonixPermissions += $"{permission}; ";
				}

				// Igénylés hozzáadása a rendszerhez
				_context.Add(request);
				await _context.SaveChangesAsync();

				// Igénylés megnyítása
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}

			// Amennyiben nem jók az értékek az oldal újratöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			return View();
		}

		// GET: Meglévő felhasználó részére e-mail cím igénylése
		public async Task<IActionResult> Email()
		{
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Az oldalt csak ügyintézők tekinthetik meg
			if (!Account.IsUgyintezo()) return Forbid();

			// Lista elemek betöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Az oldal megjelenítése
			return View();
		}

		// POST: Meglévő felhasználó részére e-mail cím igénylése
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Email([Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
			// Megadott értékek ellenőrzése
			if (ModelState.IsValid)
			{
				// Igénylést létrehozó személy azonosítója
				request.RequestAuthorId = await RequestAuthorId();

				// Igénylés létrehozásának dátuma
				request.CreationDate = DateTime.Now;

				// Igénylés típusa
				request.Type = "E-mail cím igénylése";

				// Alapértelmezett státusz
				request.Status = "Folyamatban";

				// Igénylés leírása
				request.Description = "Kérem engedélyezni a felhasználó részére e-mail cím elkészítését, a bv.hu tartományi rendszerben üzemelő szolgáltatások használatához.";

				// Igénylés hozzáadása a rendszerhez
				_context.Add(request);
				await _context.SaveChangesAsync();

				// Igénylés megnyítása
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}

			// Amennyiben nem jók az értékek az oldal újratöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			return View();
		}

		// GET: Meglévő felhasználó részére telefonos PIN kód igénylése
		public async Task<IActionResult> Phone()
		{
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Az oldalt csak ügyintézők tekinthetik meg
			if (!Account.IsUgyintezo()) return Forbid();

			// Lista elemek betöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Az oldal megjelenítése
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
				request.Type = "Telefonos PIN kód igénylése";
				request.Status = "Folyamatban";

				request.Description = "Kérem engedélyezni telefonos PIN kód kiadását a felhasználó részére, a bv.hu tartományi rendszerben üzemelő szolgáltatások használatához.";
				_context.Add(request);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}
			ViewData["RequestForId"] = new SelectList(_context.Users.OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			return View();
		}

		// GET: HikCentral jogosultság igénylése meglévő felhasználó részére
		public async Task<IActionResult> HikcentralPermission()
		{
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Az oldalt csak ügyintézők tekinthetik meg
			if (!Account.IsUgyintezo()) return Forbid();

			// Lista elemek betöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Az oldal megjelenítése
			return View();
		}

		// POST: HikCentral jogosultság igénylése meglévő felhasználó részére
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> HikcentralPermission(string permissionType, [Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
			// Megadott értékek ellenőrzése
			if (ModelState.IsValid)
			{
				// Igénylést létrehozó személy azonosítója
				request.RequestAuthorId = await RequestAuthorId();

				// Igénylés létrehozásának dátuma
				request.CreationDate = DateTime.Now;

				// Igénylés típusa
				request.Type = "HikCentral jogosultság igénylése";

				// Alapértelmezett státusz
				request.Status = "Folyamatban";

				// Igénylés leírása
				request.Description = $"Kérem engedélyezni a felhasználó részére {permissionType.ToLower()} típusú jogosultságot bíztosítani a Hikcentral programon belül, a kamerarendszer használatához.";

				// Igénylés hozzáadása a rendszerhez
				_context.Add(request);
				await _context.SaveChangesAsync();

				// Igénylés megnyítása
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}

			// Amennyiben nem jók az értékek az oldal újratöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			return View();
		}

		// GET: Kamerafelvétel lementése címkék alapján
		public async Task<IActionResult> RecordsByTags()
		{
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Az oldalt csak ügyintézők tekinthetik meg
			if (!Account.IsUgyintezo()) return Forbid();

			ViewData["Cameras"] = new MultiSelectList(_context.Cameras.OrderBy(p => p.Name), "Name", "Name");

			// Az oldal megjelenítése
			return View();
		}

		// POST: Kamerafelvétel lementése címkék alapján
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RecordsByTags(DateTime inputDate, string inputWhy, string inputTags, string[] selectedCameras, [Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
			// Megadott értékek ellenőrzése
			if (ModelState.IsValid)
			{
				// Igénylést létrehozó személy azonosítója
				request.RequestAuthorId = await RequestAuthorId();
				request.RequestForId = request.RequestAuthorId;

				// Igénylés létrehozásának dátuma
				request.CreationDate = DateTime.Now;

				// Igénylés típusa
				request.Type = "Kamerafelvétel lementése";

				// Alapértelmezett státusz
				request.Status = "Folyamatban";

				// Kamerák sorrendbe helyezése
				string cameras = string.Join(", ", selectedCameras);

				// Igénylés leírása
				request.Description = $"Kérem engedélyezni a kamerarendszerben rögzített adatok külső adattárolón történő tárolását, illetve felhasználását megkeresés alapján Bűnügyi vagy Felügyeleti szerv részére.<br /><br />" +
					$"<dl>\r\n<dt><i class=\"far fa-eye\"></i> Lementésének oka</dt>\r\n<dd>{inputWhy}</dd>\r\n<dt><i class=\"fas fa-calendar\"></i> Esemény dátuma</dt>\r\n<dd>{inputDate.ToString("yyyy.MM.dd")}</dd>\r\n<dt><i class=\"fas fa-tags\"></i> Címkék megnevezése</dt>\r\n<dd>{inputTags}</dd>\r\n<dt><i class=\"fas fa-video\"></i> Megcímkézett kamerák</dt>\r\n<dd>{cameras}</dd>\r\n</dl>";

				// Igénylés hozzáadása a rendszerhez
				_context.Add(request);
				await _context.SaveChangesAsync();

				// Igénylés megnyítása
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}

			// Amennyiben nem jók az értékek az oldal újratöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			return View();
		}

		// GET: Kamerafelvétel lementése címkék alapján
		public async Task<IActionResult> RecordsByTime()
		{
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Az oldalt csak ügyintézők tekinthetik meg
			if (!Account.IsUgyintezo()) return Forbid();

			ViewData["Cameras"] = new MultiSelectList(_context.Cameras.OrderBy(p => p.Name), "Name", "Name");

			// Az oldal megjelenítése
			return View();
		}

		// POST: Kamerafelvétel lementése címkék alapján
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RecordsByTime(DateTime inputDate, string inputWhy, string inputTags, string[] selectedCameras, [Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
			// Megadott értékek ellenőrzése
			if (ModelState.IsValid)
			{
				// Igénylést létrehozó személy azonosítója
				request.RequestAuthorId = await RequestAuthorId();
				request.RequestForId = request.RequestAuthorId;

				// Igénylés létrehozásának dátuma
				request.CreationDate = DateTime.Now;

				// Igénylés típusa
				request.Type = "Kamerafelvétel lementése";

				// Alapértelmezett státusz
				request.Status = "Folyamatban";

				// Kamerák sorrendbe helyezése
				string cameras = string.Join(", ", selectedCameras);

				// Igénylés leírása
				request.Description = $"Kérem engedélyezni a kamerarendszerben rögzített adatok külső adattárolón történő tárolását, illetve felhasználását megkeresés alapján Bűnügyi vagy Felügyeleti szerv részére.<br /><br />" +
					$"<dl>\r\n<dt><i class=\"far fa-eye\"></i> Lementésének oka</dt>\r\n<dd>{inputWhy}</dd>\r\n<dt><i class=\"fas fa-calendar\"></i> Esemény dátuma</dt>\r\n<dd>{inputDate.ToString("yyyy.MM.dd")}</dd>\r\n<dt><i class=\"fas fa-tags\"></i> Címkék megnevezése</dt>\r\n<dd>{inputTags}</dd>\r\n<dt><i class=\"fas fa-video\"></i> Megcímkézett kamerák</dt>\r\n<dd>{cameras}</dd>\r\n</dl>";

				// Igénylés hozzáadása a rendszerhez
				_context.Add(request);
				await _context.SaveChangesAsync();

				// Igénylés megnyítása
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}

			// Amennyiben nem jók az értékek az oldal újratöltése
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