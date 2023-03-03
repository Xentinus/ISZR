using ISZR.Web.Components;
using ISZR.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            if (id == null || status == null || _context.Requests == null) return NotFound();

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
            try
            {
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
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserAdditionalAccess(string[] windowsPermissions, string[] fonix3Permissions, [Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
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
                    StringBuilder windows = new StringBuilder();
                    foreach (string permission in windowsPermissions) windows.Append(permission[permission.Length - 1] == ';' ? $"{permission} " : $"{permission}; ");
                    request.WindowsPermissions = windows.ToString();
                }

                // Főnix 3 jogosultságok neveinek sorrendbe helyezése
                if (fonix3Permissions.Length > 0)
                {
                    StringBuilder fonix3 = new StringBuilder();
                    foreach (string permission in fonix3Permissions) fonix3.Append(permission[permission.Length - 1] == ';' ? $"{permission} " : $"{permission}; ");
                    request.FonixPermissions = fonix3.ToString();
                }

                // Igénylés hozzáadása a rendszerhez
                _context.Add(request);
                await _context.SaveChangesAsync();

                // Igénylés megnyítása
                return RedirectToAction(nameof(Details), new { @id = request.RequestId });
            }

            // Amennyiben nem jók az értékek az oldal újratöltése
            ViewData["RequestForId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

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

                // Igénylés hozzáadása a rendszerhez
                _context.Add(request);
                await _context.SaveChangesAsync();

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

                // Igénylés hozzáadása a rendszerhez
                _context.Add(request);
                await _context.SaveChangesAsync();

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
        public async Task<IActionResult> Parking(string brand, string modell, string licensePlate, [Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
        {
            // Autóval kapcsolatos adatok meglétének ellenőrzése
            if (brand == null || modell == null || licensePlate == null) return Forbid();

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
                    $"<dl>\r\n<dt><i class=\"fas fa-car\"></i> Jármű típusa</dt>\r\n<dd>{brand} {modell}</dd>\r\n<dt><i class=\"fas fa-parking\"></i> Jármű rendszáma</dt>\r\n<dd>{licensePlate}</dd>\r\n</dl>";

                // Igénylés hozzáadása a rendszerhez
                _context.Add(request);
                await _context.SaveChangesAsync();

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
        public async Task<IActionResult> HikcentralPermission(string permissionType, [Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
        {
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

                // Igénylés hozzáadása a rendszerhez
                _context.Add(request);
                await _context.SaveChangesAsync();

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
        public async Task<IActionResult> RecordsByTags(DateTime inputDate, string inputWhy, string inputTags, string[] selectedCameras, [Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
        {
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

            // Felület megjelenítése
            return View();
        }

        /// <summary>
        /// Időpont alapú kamerafelvétel hozzáadása az adatbázishoz
        /// </summary>
        /// <param name="request">Igénylés értékei</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordsByTime([Bind("RequestId,Type,Status,Description,RequestAuthorId,RequestForId")] Request request)
        {
            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                // Igénylést létrehozó személy azonosítója
                request.RequestAuthorId = await GetLoggedUserId();
                request.RequestForId = request.RequestAuthorId;

                // Igénylés létrehozásának dátuma
                request.CreationDate = DateTime.Now;

                // Igénylés típusa
                request.Type = "Kamerafelvétel lementése";

                // Alapértelmezett státusz
                request.Status = "Folyamatban";

                // Igénylés hozzáadása a rendszerhez
                _context.Add(request);
                await _context.SaveChangesAsync();

                // Igénylés megnyítása
                return RedirectToAction(nameof(Details), new { @id = request.RequestId });
            }

            // Amennyiben nem jók az értékek az oldal újratöltése
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
        /// Bejelentkezett felhasználó azonosítójának megszerzése
        /// </summary>
        /// <returns>Felhasználói azonosító</returns>
        private async Task<int> GetLoggedUserId()
        {
            // Felhasználónév lekérdezése a számítógéptől
            string? activeUsername = User.Identity?.Name;
            if (activeUsername == null) return -1;

            // Felhasználó megkeresése az adatbázisban
            var foundUser = await _context.Users
                .Include(u => u.Class)
                .Include(u => u.Position)
                .FirstOrDefaultAsync(m => m.Username == activeUsername);

            // Felhasználó meglétének ellenőrzése
            if (foundUser == null) return -1;

            // Return user id
            return foundUser.UserId;
        }
    }
}