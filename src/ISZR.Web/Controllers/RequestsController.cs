using ISZR.Web.Components;
using ISZR.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;

namespace ISZR.Web.Controllers
{
	/// <summary>
	/// /Requests/? Controller
	/// </summary>
	public class RequestsController : Controller
	{
		private readonly DataContext _context;

		public RequestsController(DataContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Igénylések megjelenítése szűrés alapján
		/// </summary>
		/// <param name="status">Igénylés státusza</param>
		/// <param name="type">Igénylés típusa</param>
		/// <param name="requestFor">Kinek a számára zajlik az igénylés</param>
		public async Task<IActionResult> Index(string status, string type, int requestFor)
		{
			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Igénylések listájának lekérdezése
			var dataContext = _context.Requests
				.Include(r => r.RequestAuthor)
				.Include(r => r.RequestFor)
				.OrderByDescending(r => r.RequestId)
				.AsQueryable();

			// Amennyiben semmilyen érték nem létezik (vendég felhasználóknak, saját igénylések megtekintése)
			if (string.IsNullOrEmpty(status) && string.IsNullOrEmpty(type) && requestFor == 0)
			{
				int userId = await Account.GetUserId(_context);
				dataContext = dataContext.Where(r => r.RequestForId == userId);
			}
			else
			{
				// Szűrés csak ügyintézők számára engedélyezett
				if (!Account.IsUgyintezo()) return Forbid();

				// Státusz alapú szürés
				if (!string.IsNullOrEmpty(status) && status != "Mind")
				{
					dataContext = dataContext.Where(r => r.Status == status);
					ViewBag.status = status;
				}

				// Típus alapú szürés
				if (!string.IsNullOrEmpty(type) && type != "Mind")
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
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Az oldal megjelenítése az igénylésekkel
			return View(await dataContext.ToListAsync());
		}

		/// <summary>
		/// Adott igénylés részleteinek megjelenítése
		/// </summary>
		/// <param name="id">Igénylés azonosítója</param>
		public async Task<IActionResult> Details(int? id)
		{
			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Amennyiben nem adtak meg azonosítót, az oldal megjelenítésének elutasítása
			if (id == null || _context.Requests == null) return NotFound();

			// Adott azonosítójú kérelem kikeresése
			var request = await _context.Requests
				.Include(r => r.RequestAuthor)
				.Include(r => r.RequestFor)
				.Include(r => r.Resolver)
				.Include(r => r.RequestAuthor.Class)
				.Include(r => r.RequestAuthor.Position)
				.Include(r => r.RequestFor.Class)
				.Include(r => r.RequestFor.Class.Authorizer)
				.Include(r => r.RequestFor.Class.Authorizer.Position)
				.Include(r => r.RequestFor.Position)
				.FirstOrDefaultAsync(m => m.RequestId == id);

			// Amennyiben a kért kérelem nem létezik, az oldal megjelenítésének elutasítása
			if (request == null) return NotFound();

			// Ha nem ügyintéző akkor csak a saját ügyeit tekintheti meg
			if (!Account.IsUgyintezo())
			{
				var userId = await Account.GetUserId(_context);
				if (request.RequestForId != userId)
				{
					return Forbid();
				}
			}

			// Adminisztrátorok részére ResolverId beállítása (adatbázisban nem írja még felül, csak ha státusz módosít)
			if (Account.IsAdmin()) request.ResolverId = await Account.GetUserId(_context);

			// Oldal megjelenítése a kért igényléssel
			return View(request);
		}

		/// <summary>
		/// Adott igénylés státuszának módosítása
		/// </summary>
		/// <param name="id">Igénylés azonosítója</param>
		/// <param name="status">Igénylés státusza</param>
		/// <param name="resolverId">Igénylést lezáró személy azonosítója</param>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Details(int? id, string? status, int? resolverId)
		{
			// Igényléssel kapcsolat
			if (id == null || string.IsNullOrEmpty(status) || _context.Requests == null) return NotFound();

			// Adott azonosítójú kérelem kikeresése
			var request = await _context.Requests
				.Include(r => r.RequestAuthor)
				.Include(r => r.RequestFor)
				.Include(r => r.Resolver)
				.Include(r => r.RequestAuthor.Class)
				.Include(r => r.RequestAuthor.Position)
				.Include(r => r.RequestFor.Class)
				.Include(r => r.RequestFor.Class.Authorizer)
				.Include(r => r.RequestFor.Class.Authorizer.Position)
				.Include(r => r.RequestFor.Position)
				.FirstOrDefaultAsync(m => m.RequestId == id);

			// Amennyiben a kért kérelem nem létezik, az oldal megjelenítésének elutasítása
			if (request == null) return NotFound();

			// Igénylés státuszának megváltoztatása
			request.Status = status;
			if (status == "Folyamatban")
			{
				// Folyamatban esetén időpont átírása
				request.ResolveDate = new DateTime();
			}
			else
			{
				// Más státusz alapján aktuális idő, és személy beállítása
				request.ResolveDate = DateTime.Now;
				request.ResolverId = resolverId;
			}

			try
			{
				// Igénylés státuszának frissítése
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

			// Igénylés megjelenítése
			return View(request);
		}

		/// <summary>
		/// Új felhasználó jogosultságának igénylése
		/// </summary>
		public async Task<IActionResult> NewUserAccess()
		{
			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Az oldalt csak ügyintézők tekinthetik meg
			if (!Account.IsUgyintezo()) return Forbid();

			// Listák adatainak lekérdezése
			ViewData["ClassId"] = new SelectList(_context.Classes.Where(c => !c.IsArchived).OrderBy(u => u.Name), "ClassId", "Name");
			ViewData["PositionId"] = new SelectList(_context.Positions.Where(u => !u.IsArchived).OrderBy(u => u.Name), "PositionId", "Name");
			ViewData["GroupId"] = new SelectList(_context.Groups.Where(g => !g.IsArchived).OrderBy(g => g.Name), "GroupId", "Name");

			// Oldal megjelenítése
			return View();
		}

		/// <summary>
		/// Új felhasználó jogosultságának igénylése
		/// </summary>
		/// <param name="selectedGroup">Kiválasztott jogosultsági csoport</param>
		/// <param name="user">Új felhasználóval kapcsolatos adatok</param>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> NewUserAccess(int? selectedGroup, [Bind("UserId,Username,DisplayName,Email,Phone,Rank,LastLogin,ClassId,PositionId,Genre")] User user)
		{
			// Új felhasználó adatainak meglétének ellenőrzése
			if (user == null || selectedGroup == null) return Forbid();

			// Megadott értékek ellenőrzése
			if (ModelState.IsValid)
			{
				// Kért csoport jogosultságainak lekérdezése
				Group? group = await GetGroupById(selectedGroup);
				Request? request = new();

				// Új felhasználó hozzáadása a rendszerhez
				int? newUserId = await CreateNewUserAndGetId(user);

				// Új felhasználó beállítása
				request.RequestForId = newUserId;

				// Igénylést létrehozó személy azonosítója
				request.RequestAuthorId = await GetLoggedUserId();

				// Igénylés létrehozásának dátuma
				request.CreationDate = DateTime.Now;

				// Igénylés típusa
				request.Type = "Új felhasználó részére jogosultság igénylés";

				// Alapértelmezett státusz
				request.Status = "Folyamatban";

				// Igénylés leírása
				request.Description = "Kérem engedélyezni új felhasználó részére jogosultság kiadását, a bv.hu tartományi rendszerben üzemelő szolgáltatások használatához.";

				// Csoport jogosultságainak hozzáadása
				request.WindowsPermissions = !string.IsNullOrEmpty(group.WindowsPermissions) ? group.WindowsPermissions : null;
				request.FonixPermissions = !string.IsNullOrEmpty(group.FonixPermissions) ? group.FonixPermissions : null;

				try
				{
					// Igénylés hozzáadása a rendszerhez
					await _context.Requests.AddAsync(request);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!RequestExists(request.RequestId))
					{
						return NotFound();
					}
					else if (!UserExists(user.UserId))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}

				// Igénylés megjelenítése
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}

			// Listák adatainak lekérdezése
			ViewData["ClassId"] = new SelectList(_context.Classes.Where(c => !c.IsArchived).OrderBy(u => u.Name), "ClassId", "Name");
			ViewData["PositionId"] = new SelectList(_context.Positions.Where(u => !u.IsArchived).OrderBy(u => u.Name), "PositionId", "Name");
			ViewData["GroupId"] = new SelectList(_context.Groups.Where(g => !g.IsArchived).OrderBy(g => g.Name), "GroupId", "Name");

			// Felület újra megjelenítése, amennyiben az értékek hibásak voltak
			return View();
		}

		/// <summary>
		/// Többletjogosultság igénylésének felülete
		/// </summary>
		public async Task<IActionResult> UserAdditionalAccess()
		{
			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Az oldalt csak ügyintézők tekinthetik meg
			if (!Account.IsUgyintezo()) return Forbid();

			// Lista elemek betöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			ViewData["Windows"] = new MultiSelectList(_context.Permissions.Where(p => p.Type == "Windows" && !p.IsArchived).OrderBy(p => p.Name), "ActiveDirectoryPermissions", "Name");
			ViewData["Fonix3"] = new MultiSelectList(_context.Permissions.Where(p => p.Type == "Főnix 3" && !p.IsArchived).OrderBy(p => p.Name), "Name", "Name");

			// Oldal megjelenítése
			return View();
		}

		/// <summary>
		/// Töbletjogosultság igénylés hozzáadása az adatbázishoz
		/// </summary>
		/// <param name="windowsPermissions">Kért Windows jogosultságok tömbben</param>
		/// <param name="fonix3Permissions">Kért Főnix 3 jogosultságok tömbben</param>
		/// <param name="request">Igénylés értékei</param>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UserAdditionalAccess(string[]? windowsPermissions, string[]? fonix3Permissions, [Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
			// Megadott értékek ellenőrzése
			if (ModelState.IsValid)
			{
				// Igénylést létrehozó személy azonosítója
				request.RequestAuthorId = await GetLoggedUserId();

				// Igénylés létrehozásának dátuma
				request.CreationDate = DateTime.Now;

				// Igénylés típusa
				request.Type = "Meglévő felhasználó részére többletjogosultság";

				// Alapértelmezett státusz
				request.Status = "Folyamatban";

				// Igénylés leírása
				request.Description = "Kérem engedélyezni a felhasználó részére többletjogosultság kiadását, a bv.hu tartományi rendszerben üzemelő szolgáltatások használatához.";

				// Windows jogosultságok Active-Directory értékeinek sorrendbe helyezése
				if (windowsPermissions.Length > 0)
				{
					StringBuilder windows = new();
					foreach (string permission in windowsPermissions) windows.Append(permission[^1] == ';' ? $"{permission} " : $"{permission}; ");
					request.WindowsPermissions = windows.ToString();
				}

				// Főnix 3 jogosultságok neveinek sorrendbe helyezése
				if (fonix3Permissions.Length > 0)
				{
					StringBuilder fonix3 = new();
					foreach (string permission in fonix3Permissions) fonix3.Append(permission[^1] == ';' ? $"{permission} " : $"{permission}; ");
					request.FonixPermissions = fonix3.ToString();
				}

				try
				{
					// Igénylés hozzáadása a rendszerhez
					_context.Add(request);
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

				// Igénylés megnyítása
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}

			// Amennyiben nem jók az értékek az oldal újratöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Felület újra megjelenítése, amennyiben az értékek hibásak voltak
			return View();
		}

		/// <summary>
		/// Új beosztáshoz járó jogosultsági felület
		/// </summary>
		public async Task<IActionResult> UserChangePosition()
		{
			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Az oldalt csak ügyintézők tekinthetik meg
			if (!Account.IsUgyintezo()) return Forbid();

			// Lista elemek betöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			ViewData["GroupId"] = new SelectList(_context.Groups.Where(g => !g.IsArchived).OrderBy(g => g.Name), "GroupId", "Name");

			// Oldal megjelenítése
			return View();
		}

		/// <summary>
		/// Új beosztáshoz tartozó jogosultsági igénylés hozzáadása az adatbázishoz
		/// </summary>
		/// <param name="request">Igénylés értékei</param>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UserChangePosition(string? currentPermissions, int? selectedGroup, [Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
			// Kötelező adatok meglétének ellenőrzése
			if (string.IsNullOrEmpty(currentPermissions) || selectedGroup == null || request == null) return Forbid();

			// Megadott értékek ellenőrzése
			if (ModelState.IsValid)
			{
				// Kért csoport jogosultságainak lekérdezése
				Group? group = await GetGroupById(selectedGroup);

				// Igénylést létrehozó személy azonosítója
				request.RequestAuthorId = await GetLoggedUserId();

				// Igénylés létrehozásának dátuma
				request.CreationDate = DateTime.Now;

				// Igénylés típusa
				request.Type = "Meglévő felhasználó új beosztásának jogosultságai";

				// Alapértelmezett státusz
				request.Status = "Folyamatban";

				// Igénylés leírása
				request.Description = "Kérem engedélyezni a felhasználó részére új beosztásának jogosultságainak kiadását, a bv.hu tartományi rendszerben üzemelő szolgáltatások használatához.<br /><br />" +
					$"<dl>\r\n<dt><i class=\"icon fas fa-people-arrows mr-2\"></i>Teendő a felhasználó jelenlegi jogosultságaival</dt>\r\n<dd>{currentPermissions}</dd>\r\n</dl>";

				// Csoport jogosultságainak hozzáadása
				request.WindowsPermissions = !string.IsNullOrEmpty(group.WindowsPermissions) ? group.WindowsPermissions : null;
				request.FonixPermissions = !string.IsNullOrEmpty(group.FonixPermissions) ? group.FonixPermissions : null;

				try
				{
					// Igénylés hozzáadása a rendszerhez
					_context.Add(request);
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

				// Igénylés megnyítása
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}

			// Amennyiben nem jók az értékek az oldal újratöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");
			ViewData["GroupId"] = new SelectList(_context.Groups.Where(g => !g.IsArchived).OrderBy(g => g.Name), "GroupId", "Name");

			// Felület újra megjelenítése, amennyiben az értékek hibásak voltak
			return View();
		}

		/// <summary>
		/// E-mail cím igénylés felülete
		/// </summary>
		public async Task<IActionResult> Email()
		{
			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Az oldalt csak ügyintézők tekinthetik meg
			if (!Account.IsUgyintezo()) return Forbid();

			// Lista elemek betöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Az oldal megjelenítése
			return View();
		}

		/// <summary>
		/// E-mail cím igénylés hozzáadása az adatbázishoz
		/// </summary>
		/// <param name="request">Igénylés értékei</param>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Email([Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
			// Megadott értékek ellenőrzése
			if (ModelState.IsValid)
			{
				// Igénylést létrehozó személy azonosítója
				request.RequestAuthorId = await GetLoggedUserId();

				// Igénylés létrehozásának dátuma
				request.CreationDate = DateTime.Now;

				// Igénylés típusa
				request.Type = "E-mail cím igénylése";

				// Alapértelmezett státusz
				request.Status = "Folyamatban";

				// Igénylés leírása
				request.Description = "Kérem engedélyezni a felhasználó részére e-mail cím elkészítését, a bv.hu tartományi rendszerben üzemelő szolgáltatások használatához.";


				try
				{
					// Igénylés hozzáadása a rendszerhez
					_context.Add(request);
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

				// Igénylés megnyítása
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}

			// Amennyiben nem jók az értékek az oldal újratöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Felület újra megjelenítése, amennyiben az értékeket hibásan adták meg
			return View();
		}

		/// <summary>
		/// Telefonos PIN kód igénylés felülete
		/// </summary>
		public async Task<IActionResult> Phone()
		{
			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Az oldalt csak ügyintézők tekinthetik meg
			if (!Account.IsUgyintezo()) return Forbid();

			// Lista elemek betöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Felület megjelenítése
			return View();
		}

		/// <summary>
		/// Telefonos PIN kód igénylés hozzáadása az adatbázishoz
		/// </summary>
		/// <param name="request">Igénylés értékei</param>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Phone([Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
			// Megadott értékek ellenőrzése
			if (ModelState.IsValid)
			{
				// Igénylést létrehozó személy azonosítója
				request.RequestAuthorId = await GetLoggedUserId();

				// Igénylés létrehozásának dátuma
				request.CreationDate = DateTime.Now;

				// Igénylés típusa
				request.Type = "Telefonos PIN kód igénylése";

				// Alapértelmezett státusz
				request.Status = "Folyamatban";

				// Igénylés leírása
				request.Description = "Kérem engedélyezni telefonos PIN kód kiadását a felhasználó részére, a bv.hu tartományi rendszerben üzemelő szolgáltatások használatához.";

				try
				{
					// Igénylés hozzáadása a rendszerhez
					_context.Add(request);
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

				// Igénylés megnyítása
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}

			// Amennyiben nem jók az értékek az oldal újratöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Felület megjelenítése
			return View();
		}

		/// <summary>
		/// Parkolási engedély igénylésének felülete
		/// </summary>
		public async Task<IActionResult> Parking()
		{
			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Az oldalt csak ügyintézők tekinthetik meg
			if (!Account.IsUgyintezo()) return Forbid();

			// Lista elemek betöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Felület megjelenítése
			return View();
		}

		/// <summary>
		/// Parkolási engedély igénylésének hozzáadása az adatbázishoz
		/// </summary>
		/// <param name="brand">Jármű márkája</param>
		/// <param name="modell">Jármű modellje</param>
		/// <param name="licensePlate">Jármű rendszáma</param>
		/// <param name="request">Igénylés értékei</param>
		/// <returns></returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Parking(string? brand, string? modell, string? licensePlate, [Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
			// Autóval kapcsolatos adatok meglétének ellenőrzése
			if (string.IsNullOrEmpty(brand) || string.IsNullOrEmpty(modell) || string.IsNullOrEmpty(licensePlate)) return Forbid();

			// Megadott értékek ellenőrzése
			if (ModelState.IsValid)
			{
				// Igénylést létrehozó személy azonosítója
				request.RequestAuthorId = await GetLoggedUserId();

				// Igénylés létrehozásának dátuma
				request.CreationDate = DateTime.Now;

				// Igénylés típusa
				request.Type = "Parkolási engedély igénylése";

				// Alapértelmezett státusz
				request.Status = "Folyamatban";

				// Igénylés leírása
				request.Description = $"Kérem engedélyezni a felhasználó részére, az alábbi jármű parkolási engedélyének kiállítását.<br /><br />" +
					$"<dl>\r\n<dt><i class=\"icon fas fa-car mr-2\"></i>Jármű típusa</dt>\r\n<dd>{brand} {modell}</dd>\r\n<dt><i class=\"icon fas fa-parking mr-2\"></i>Jármű rendszáma</dt>\r\n<dd>{licensePlate}</dd>\r\n</dl>";

				try
				{
					// Igénylés hozzáadása a rendszerhez
					_context.Add(request);
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

				// Igénylés megnyítása
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}

			// Amennyiben nem jók az értékek az oldal újratöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Felület megjelenítése
			return View();
		}

		/// <summary>
		/// Hikcentral jogosultság igénylésének felülete
		/// </summary>
		public async Task<IActionResult> HikcentralPermission()
		{
			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Az oldalt csak ügyintézők tekinthetik meg
			if (!Account.IsUgyintezo()) return Forbid();

			// Lista elemek betöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Felület megjelenítése
			return View();
		}

		/// <summary>
		/// Hikcentral jogosultság igénylésének hozzáadása az adatbázishoz
		/// </summary>
		/// <param name="permissionType">Jogosultsági típus</param>
		/// <param name="request">Igénylés értékei</param>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> HikcentralPermission(string? permissionType, [Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
            // Autóval kapcsolatos adatok meglétének ellenőrzése
            if (string.IsNullOrEmpty(permissionType)) return Forbid();

            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
			{
				// Igénylést létrehozó személy azonosítója
				request.RequestAuthorId = await GetLoggedUserId();

				// Igénylés létrehozásának dátuma
				request.CreationDate = DateTime.Now;

				// Igénylés típusa
				request.Type = "HikCentral jogosultság igénylése";

				// Alapértelmezett státusz
				request.Status = "Folyamatban";

				// Igénylés leírása
				request.Description = $"Kérem engedélyezni a felhasználó részére {permissionType.ToLower()} típusú jogosultságot bíztosítani a Hikcentral programon belül, a kamerarendszer használatához.";

				try
				{
					// Igénylés hozzáadása a rendszerhez
					_context.Add(request);
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

				// Igénylés megnyítása
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}

			// Amennyiben nem jók az értékek az oldal újratöltése
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Felület megjelenítése
			return View();
		}

		/// <summary>
		/// Címkézett kamerafelvételek lementésének felülete
		/// </summary>
		public async Task<IActionResult> RecordsByTags()
		{
			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Az oldalt csak ügyintézők tekinthetik meg
			if (!Account.IsUgyintezo()) return Forbid();

			// Lista elemek betöltése
			ViewData["Cameras"] = new MultiSelectList(_context.Cameras.Where(c => !c.IsArchived).OrderBy(c => c.Name), "Name", "Name");
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Felület megjelenítése
			return View();
		}

		/// <summary>
		/// Címkézett kamerafelvételek lementésének hozzáadása az adatbázisba
		/// </summary>
		/// <param name="inputDate">Esemény dátuma</param>
		/// <param name="inputWhy">Lementésének oka</param>
		/// <param name="inputTags">Címkék megnevezése</param>
		/// <param name="selectedCameras">Kiválasztott kamerák</param>
		/// <param name="request">Igénylés értékei</param>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RecordsByTags(DateTime inputDate, string? inputWhy, string? inputTags, string[]? selectedCameras, [Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
			// Adatok meglétének ellenőrzése
			if (string.IsNullOrEmpty(inputWhy) || string.IsNullOrEmpty(inputTags) || selectedCameras == null) return Forbid();

			// Megadott értékek ellenőrzése
			if (ModelState.IsValid)
			{
				// Igénylést létrehozó személy azonosítója
				request.RequestAuthorId = await GetLoggedUserId();

				// Igénylés létrehozásának dátuma
				request.CreationDate = DateTime.Now;

				// Igénylés típusa
				request.Type = "Kamerafelvétel lementése";

				// Alapértelmezett státusz
				request.Status = "Folyamatban";

				// Kamerák sorrendbe helyezése
				string cameras = string.Join(", ", selectedCameras);

				// Dátum átalakítása
				string inputDateString = inputDate.ToString("yyyy.MM.dd");

                // Igénylés leírása
                request.Description = $"Kérem engedélyezni a kamerarendszerben rögzített adatok külső adattárolón történő tárolását, illetve felhasználását megkeresés alapján Bűnügyi vagy Felügyeleti szerv részére.<br /><br />" +
					$"<dl>\r\n<dt><i class=\"icon far fa-eye mr-2\"></i>Lementésének oka</dt>\r\n<dd>{inputWhy}</dd>\r\n<dt><i class=\"icon fas fa-calendar mr-2\"></i>Esemény dátuma</dt>\r\n<dd>{inputDateString}</dd>\r\n<dt><i class=\"icon fas fa-tags mr-2\"></i>Címkék megnevezése</dt>\r\n<dd>{inputTags}</dd>\r\n<dt><i class=\"icon fas fa-video mr-2\"></i>Megcímkézett kamerák</dt>\r\n<dd>{cameras}</dd>\r\n</dl>";

				try
				{
					// Igénylés hozzáadása a rendszerhez
					_context.Add(request);
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

				// Igénylés megnyítása
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}

			// Amennyiben nem jók az értékek az oldal újratöltése
			ViewData["Cameras"] = new MultiSelectList(_context.Cameras.Where(c => !c.IsArchived).OrderBy(c => c.Name), "Name", "Name");
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Felület megjelenítése
			return View();
		}

		/// <summary>
		/// Időpont alapú kamerafelvételek felülete
		/// </summary>
		public async Task<IActionResult> RecordsByTime()
		{
			// Az ISZR-ben nem megtalálható személyek kizására
			if (!await Account.IsUserExists(_context)) return Forbid();

			// Az oldalt csak ügyintézők tekinthetik meg
			if (!Account.IsUgyintezo()) return Forbid();

			// Lenyíló menü elemeinek lekérdezése
			ViewData["Cameras"] = new MultiSelectList(_context.Cameras.Where(c => !c.IsArchived).OrderBy(c => c.Name), "Name", "Name");
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Felület megjelenítése
			return View();
		}

		/// <summary>
		/// Időpont alapú kamerafelvétel hozzáadása az adatbázishoz
		/// </summary>
		/// <param name="inputWhy">Lementés oka</param>
		/// <param name="recordsArray">Felvételek tömbje stringként tárolva</param>
		/// <param name="request">Igénylés értékei</param>
		/// <returns></returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RecordsByTime(string? inputWhy, string? recordsArray, [Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
		{
			// Adatok meglétének ellenőrzése
			if (string.IsNullOrEmpty(inputWhy) || string.IsNullOrEmpty(recordsArray)) return Forbid();

			// Felvételek tömbösítése
			string[][]? records = JsonConvert.DeserializeObject<string[][]?>(recordsArray);

			// Megadott értékek ellenőrzése
			if (ModelState.IsValid)
			{
				// Igénylést létrehozó személy azonosítója
				request.RequestAuthorId = await GetLoggedUserId();

				// Igénylés létrehozásának dátuma
				request.CreationDate = DateTime.Now;

				// Igénylés típusa
				request.Type = "Kamerafelvétel lementése";

				// Alapértelmezett státusz
				request.Status = "Folyamatban";

				// Igénylés leírása
				request.Description = $"Kérem engedélyezni a kamerarendszerben rögzített adatok külső adattárolón történő tárolását, illetve felhasználását megkeresés alapján Bűnügyi vagy Felügyeleti szerv részére.<br /><br />" +
					$"<dl>\r\n<dt><i class=\"icon far fa-eye mr-2\"></i>Kamerafelvétel lementésének oka</dt>\r\n<dd>{inputWhy}</dd>\r\n</dl><br />" +
					$"<div class=\"card\">\r\n<div class=\"card-body p-0\">\r\n<table class=\"table\">\r\n<thead class=\"bg-light\">\r\n<tr>\r\n<th><i class=\"icon fas fa-video mr-2\"></i>Kamera</th>\r\n<th><i class=\"icon fas fa-play mr-2\"></i>Felvétel kezdete</th>\r\n<th><i class=\"icon fas fa-stop mr-2\"></i>Felvétel vége</th>\r\n</tr>\r\n</thead>\r\n<tbody>\r\n";

				// Kamerafelvételek hozzáadása a táblázhoz
				StringBuilder recordTable = new();
				foreach (string[] record in records) recordTable.Append($"<tr><td>{record[0]}</td><td>{record[1]}</td><td>{record[2]}</td></tr>");
				request.Description += recordTable.ToString();

				// Táblázat lezárása
				request.Description += $"</tbody>\r\n</table>\r\n</div>\r\n</div>";

				try
				{
					// Igénylés hozzáadása a rendszerhez
					_context.Add(request);
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

				// Igénylés megnyítása
				return RedirectToAction(nameof(Details), new { @id = request.RequestId });
			}

			// Amennyiben nem jók az értékek az oldal újratöltése
			ViewData["Cameras"] = new MultiSelectList(_context.Cameras.Where(c => !c.IsArchived).OrderBy(c => c.Name), "Name", "Name");
			ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			// Felület megjelenítése
			return View();
		}

		/// <summary>
		/// Igénylés meglétének ellenőrzése
		/// </summary>
		/// <param name="id">Igénylés azonosítója</param>
		/// <returns>Létezik e az adott igénylés (igaz/hamis)</returns>
		private bool RequestExists(int id)
		{
			return _context.Requests.Any(e => e.RequestId == id);
		}

		/// <summary>
		/// Felhasználói azonosító megkeresése a rendszerben
		/// </summary>
		/// <param name="id">Felhasználói azonosító</param>
		/// <returns>Létezik e a felhasználói azonosító (igaz/hamis)</returns>
		private bool UserExists(int id)
		{
			return _context.Users.Any(e => e.UserId == id);
		}

		/// <summary>
		/// Csoport megkeresése a rendszerben csoport azonosító által
		/// </summary>
		/// <param name="id">Csoport azonosítója</param>
		/// <returns>Csoport</returns>
		private async Task<Group?> GetGroupById(int? id)
		{
			return await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == id);
		}

		private async Task<int?> CreateNewUserAndGetId(User? newUser)
		{
			if (newUser == null) return null;
			// Új felhasználó hozzáadása a rendszerhez
			await _context.Users.AddAsync(newUser);
			await _context.SaveChangesAsync();
			return newUser.UserId;
		}

		/// <summary>
		/// Bejelentkezett felhasználó azonosítójának megszerzése
		/// </summary>
		/// <returns>Felhasználói azonosító</returns>
		private async Task<int?> GetLoggedUserId()
		{
			// Felhasználónév lekérdezése a számítógéptől
			string? activeUsername = User.Identity?.Name;
			if (activeUsername == null) return null;

			// Felhasználó megkeresése az adatbázisban
			var foundUser = await _context.Users
				.Include(u => u.Class)
				.Include(u => u.Position)
				.FirstOrDefaultAsync(m => m.Username == activeUsername);

			// Felhasználó meglétének ellenőrzése
			if (foundUser == null) return null;

			// Return user id
			return foundUser.UserId;
		}
	}
}