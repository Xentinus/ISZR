﻿using ISZR.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using System.Text;
using System.Resources;
using ISZR.Web.Services;
using System.Net.NetworkInformation;

namespace ISZR.Web.Controllers
{
    /// <summary>
    /// /Requests/? Controller
    /// </summary>
    [Authorize(Policy = "Ugyintezo")]
    public class RequestsController : Controller
    {
        private readonly DataContext _context;
        private readonly EmailService _emailService;

        public RequestsController(DataContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        /// <summary>
        /// Igénylések megjelenítése szűrés alapján
        /// </summary>
        /// <param name="status">Igénylés státusza</param>
        /// <param name="type">Igénylés típusa</param>
        /// <param name="user">Kinek a számára zajlik az igénylés</param>
        public async Task<IActionResult> Index(string user, string type, string status, int? pageNumber)
        {
            // Értékek beállítása
            ViewData["user"] = user;
            ViewData["type"] = type;
            ViewData["status"] = status;


            // Igénylések listájának lekérdezése
            var dataContext = _context.Requests
                .Include(r => r.CreatedForUser)
                .Include(r => r.CreatedForUser.Position)
                .OrderByDescending(r => r.RequestId)
                .AsQueryable();

            // Státusz alapú szürés
            if (!string.IsNullOrEmpty(status) && status != "Mind")
            {
                dataContext = dataContext.Where(r => r.Status == status);
            }

            // Típus alapú szürés
            if (!string.IsNullOrEmpty(type) && type != "Mind")
            {
                dataContext = dataContext.Where(r => r.Type == type);
            }

            // Személy alapú szürés
            if (!string.IsNullOrEmpty(user))
            {
                dataContext = dataContext.Where(r => r.CreatedForUser.DisplayName.Contains(user));
            }

            // Állapot lista összeállítása
            List<string?> requestStatus = _context.Requests.Select(r => r.Status).Distinct().OrderBy(r => r).ToList();
            requestStatus.Insert(0, "Mind");

            ViewData["StatusList"] = requestStatus.Select(status => new SelectListItem
            {
                Text = status,
                Value = status
            }).ToList();

            // Típus lista összeállítása
            List<string?> requestTypes = _context.Requests.Select(r => r.Type).Distinct().OrderBy(r => r).ToList();
            requestTypes.Insert(0, "Mind");

            ViewData["TypeList"] = requestTypes.Select(type => new SelectListItem
            {
                Text = type,
                Value = type
            }).ToList();


            // Igénylési lista összeállítása
            await dataContext.ToListAsync();
            ViewData["dataLength"] = dataContext.Count();

            // Az oldal megjelenítése az igénylésekkel
            return View(await PaginatedList<Request>.CreateAsync(dataContext, pageNumber ?? 1));
        }

        /// <summary>
        /// Adott igénylés részleteinek megjelenítése
        /// </summary>
        /// <param name="id">Igénylés azonosítója</param>
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            // Amennyiben nem adtak meg azonosítót, az oldal megjelenítésének elutasítása
            if (id == null || _context.Requests == null) return NotFound();

            // Adott azonosítójú kérelem kikeresése
            var request = await _context.Requests
                .Include(r => r.CreatedByUser)
                .Include(r => r.CreatedForUser)
                .Include(r => r.ClosedByUser)
                .Include(r => r.CreatedByUser.Class)
                .Include(r => r.CreatedByUser.Position)
                .Include(r => r.CreatedForUser.Class)
                .Include(r => r.CreatedForUser.Class.Authorizer)
                .Include(r => r.CreatedForUser.Class.Authorizer.Position)
                .Include(r => r.CreatedForUser.Position)
                .Include(r => r.Car)
                .Include(r => r.Phone)
                .FirstOrDefaultAsync(m => m.RequestId == id);

            // Amennyiben a kért kérelem nem létezik, az oldal megjelenítésének elutasítása
            if (request == null) return NotFound();
            if (request.Type == null) return NotFound(request);

            ResourceManager resourceManager = new ResourceManager(typeof(RequestMessages));

            ViewData["desc"] = resourceManager.GetString(request.Type) ?? "";

            // Adminisztrátorok részére ClosedByUserId beállítása (adatbázisban nem írja még felül, csak ha státusz módosít)
            ViewData["ClosedByUserId"] = await GetUserIdAsync(_context);

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
        [Authorize(Policy = "Administrator")]
        public async Task<IActionResult> Details(int? id, string? status, int? resolverId)
        {
            // Igényléssel kapcsolat
            if (id == null || string.IsNullOrEmpty(status) || _context.Requests == null) return NotFound();

            // Adott azonosítójú kérelem kikeresése
            var request = await _context.Requests
                .Include(r => r.CreatedByUser)
                .Include(r => r.CreatedForUser)
                .Include(r => r.ClosedByUser)
                .Include(r => r.CreatedByUser.Class)
                .Include(r => r.CreatedByUser.Position)
                .Include(r => r.CreatedForUser.Class)
                .Include(r => r.CreatedForUser.Class.Authorizer)
                .Include(r => r.CreatedForUser.Class.Authorizer.Position)
                .Include(r => r.CreatedForUser.Position)
                .FirstOrDefaultAsync(m => m.RequestId == id);

            // Amennyiben a kért kérelem nem létezik, az oldal megjelenítésének elutasítása
            if (request == null) return NotFound();

            // Igénylés státuszának megváltoztatása
            request.Status = status;
            if (status == "Folyamatban")
            {
                // Folyamatban esetén időpont átírása
                request.ClosedDateTime = new DateTime();
            }
            else
            {
                // Más státusz alapján aktuális idő, és személy beállítása
                request.ClosedDateTime = DateTime.Now;
                request.ClosedByUserId = resolverId;
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

            // Értesítés küldése (amennyiben van e-mail cím)
            await _emailService.SendStatusChange(request, status);

            // Igénylés megjelenítése
            return View(request);
        }

        /// <summary>
        /// Igénylések megjelenítése szűrés alapján
        /// </summary>
        /// <param name="status">Igénylés státusza</param>
        /// <param name="type">Igénylés típusa</param>
        [AllowAnonymous]
        public async Task<IActionResult> ForYou(string type, string status, int? pageNumber)
        {
            // Értékek beállítása
            ViewData["type"] = type;
            ViewData["status"] = status;

            // Megtekintő felhasználó azonosítójának elmentése
            int? viewerId = await GetUserIdAsync(_context);
            if (viewerId == null) return NotFound();

            // Igénylések listájának lekérdezése
            var dataContext = _context.Requests
                .Where(r => r.CreatedForUserId == viewerId)
                .OrderByDescending(r => r.RequestId)
                .AsQueryable();

            // Státusz alapú szürés
            if (!string.IsNullOrEmpty(status) && status != "Mind")
            {
                dataContext = dataContext.Where(r => r.Status == status);
            }

            // Típus alapú szürés
            if (!string.IsNullOrEmpty(type) && type != "Mind")
            {
                dataContext = dataContext.Where(r => r.Type == type);
            }

            // Állapot lista összeállítása
            List<string?> requestStatus = _context.Requests.Select(r => r.Status).Distinct().OrderBy(r => r).ToList();
            requestStatus.Insert(0, "Mind");

            ViewData["StatusList"] = requestStatus.Select(status => new SelectListItem
            {
                Text = status,
                Value = status
            }).ToList();

            // Típus lista összeállítása
            List<string?> requestTypes = _context.Requests.Select(r => r.Type).Distinct().OrderBy(r => r).ToList();
            requestTypes.Insert(0, "Mind");

            ViewData["TypeList"] = requestTypes.Select(type => new SelectListItem
            {
                Text = type,
                Value = type
            }).ToList();


            // Igénylési lista összeállítása
            await dataContext.ToListAsync();
            ViewData["dataLength"] = dataContext.Count();

            // Az oldal megjelenítése az igénylésekkel
            return View(await PaginatedList<Request>.CreateAsync(dataContext, pageNumber ?? 1));
        }

        /// <summary>
        /// Végrehajtásra váró igénylések
        /// </summary>
        [Authorize(Policy = "Administrator")]
        public async Task<IActionResult> ToDo(int? pageNumber)
        {
            // Igénylések listájának lekérdezése
            var dataContext = _context.Requests
                .Include(r => r.CreatedByUser)
                .Include(r => r.CreatedForUser)
                .Include(r => r.CreatedByUser.Position)
                .Include(r => r.CreatedForUser.Position)
                .Where(r => r.Status == "Folyamatban")
                .OrderByDescending(r => r.RequestId)
                .AsQueryable();

            // Lezáró személy beállítása
            ViewData["ClosedByUserId"] = await GetUserIdAsync(_context);

            // Igénylési lista összeállítása
            await dataContext.ToListAsync();
            ViewData["dataLength"] = dataContext.Count();

            // Az oldal megjelenítése az igénylésekkel
            return View(await PaginatedList<Request>.CreateAsync(dataContext, pageNumber ?? 1));
        }

        /// <summary>
        /// Adott igénylés státuszának módosítása
        /// </summary>
        /// <param name="id">Igénylés azonosítója</param>
        /// <param name="status">Igénylés státusza</param>
        /// <param name="closedByUserId">Igénylést lezáró személy azonosítója</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrator")]
        public async Task<IActionResult> ToDo(int? id, string? status, int? closedByUserId)
        {
            // Igényléssel kapcsolat
            if (id == null || string.IsNullOrEmpty(status) || _context.Requests == null) return NotFound();

            // Adott azonosítójú kérelem kikeresése
            var request = await _context.Requests
                .Include(r => r.CreatedByUser)
                .Include(r => r.CreatedForUser)
                .Include(r => r.CreatedByUser.Position)
                .Include(r => r.CreatedForUser.Position)
                .FirstOrDefaultAsync(m => m.RequestId == id);

            // Amennyiben a kért kérelem nem létezik, az oldal megjelenítésének elutasítása
            if (request == null) return NotFound();

            // Igénylés státuszának megváltoztatása
            request.Status = status;
            if (status == "Folyamatban")
            {
                // Folyamatban esetén időpont átírása
                request.ClosedDateTime = new DateTime();
            }
            else
            {
                // Más státusz alapján aktuális idő, és személy beállítása
                request.ClosedDateTime = DateTime.Now;
                request.ClosedByUserId = closedByUserId;
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

            // Értesítés küldése (amennyiben van e-mail cím)
            await _emailService.SendStatusChange(request, status);

            // Végrehajtásokra váró feladatok frissítése
            return RedirectToAction(nameof(ToDo));
        }

        /// <summary>
        /// Új felhasználó jogosultságának igénylése
        /// </summary>
        public IActionResult NewUserAccess()
        {
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
        public async Task<IActionResult> NewUserAccess(int? selectedGroup, [Bind("UserId,Username,DisplayName,Rank,Genre,Location,Email,Phone,LastLogin,ClassId,PositionId,IsArchived")] User user)
        {
            // Új felhasználó adatainak meglétének ellenőrzése
            if (user == null || selectedGroup == null) return NotFound();

            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                // Kért csoport jogosultságainak lekérdezése
                Group? group = await GetGroupById(selectedGroup);
                Request? request = new();

                // Új felhasználó hozzáadása a rendszerhez
                int? newUserId = await CreateNewUserAndGetId(user);

                // Új felhasználó beállítása
                request.CreatedForUserId = newUserId;

                // Igénylést létrehozó személy azonosítója
                request.CreatedByUserId = await GetLoggedUserId();

                // Igénylés létrehozásának dátuma
                request.CreatedDateTime = DateTime.Now;

                // Igénylés típusa
                request.Type = "Új felhasználó részére jogosultság igénylés";

                // Alapértelmezett státusz
                request.Status = "Folyamatban";

                // Csoport jogosultságainak hozzáadása
                string groupWindowsPermissions = string.Join("; ", group.GroupPermissions.Where(gp => gp.Permission.Type == "Windows" && !gp.Permission.IsArchived).Select(gp => gp.Permission.ActiveDirectoryPermissions));
                request.WindowsPermissions = !string.IsNullOrEmpty(groupWindowsPermissions) ? groupWindowsPermissions : null;

                string groupFonixPermissions = string.Join("; ", group.GroupPermissions.Where(gp => gp.Permission.Type == "Főnix 3" && !gp.Permission.IsArchived).Select(gp => gp.Permission.Name));
                request.FonixPermissions = !string.IsNullOrEmpty(groupFonixPermissions) ? groupFonixPermissions : null;

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

                // Értesítés küldése (amennyiben van e-mail cím)
                await _emailService.SendNewRequestNotification(request);

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
        public IActionResult UserAdditionalAccess()
        {

            // Lista elemek betöltése
            ViewData["CreatedForUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

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
        public async Task<IActionResult> UserAdditionalAccess(string[]? windowsPermissions, string[]? fonix3Permissions, string inputWhy, [Bind("RequestId,Type,Status,Description,CreatedByUserId,CreatedForUserId")] Request request)
        {
            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                // Igénylést létrehozó személy azonosítója
                request.CreatedByUserId = await GetLoggedUserId();

                // Igénylés létrehozásának dátuma
                request.CreatedDateTime = DateTime.Now;

                // Igénylés típusa
                request.Type = "Meglévő felhasználó részére többletjogosultság";

                // Alapértelmezett státusz
                request.Status = "Folyamatban";

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

                // Értesítés küldése (amennyiben van e-mail cím)
                await _emailService.SendNewRequestNotification(request);

                // Igénylés megnyítása
                return RedirectToAction(nameof(Details), new { @id = request.RequestId });
            }

            // Amennyiben nem jók az értékek az oldal újratöltése
            ViewData["CreatedForUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

            // Felület újra megjelenítése, amennyiben az értékek hibásak voltak
            return View();
        }

        /// <summary>
        /// Új beosztáshoz járó jogosultsági felület
        /// </summary>
        public IActionResult UserChangePosition()
        {
            // Lista elemek betöltése
            ViewData["CreatedForUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

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
        public async Task<IActionResult> UserChangePosition(string? currentPermissions, int? selectedGroup, [Bind("RequestId,Type,Status,Description,CreatedByUserId,CreatedForUserId")] Request request)
        {
            // Kötelező adatok meglétének ellenőrzése
            if (string.IsNullOrEmpty(currentPermissions) || selectedGroup == null || request == null) return NotFound();

            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                // Kért csoport jogosultságainak lekérdezése
                Group? group = await GetGroupById(selectedGroup);

                // Amennyiben a kiválasztott jogosultsági csoport nem létezik
                if (group == null) return NotFound();

                // Igénylést létrehozó személy azonosítója
                request.CreatedByUserId = await GetLoggedUserId();

                // Igénylés létrehozásának dátuma
                request.CreatedDateTime = DateTime.Now;

                // Igénylés típusa
                request.Type = "Meglévő felhasználó új beosztásának jogosultságai";

                // Alapértelmezett státusz
                request.Status = "Folyamatban";

                // Igénylés leírása
                request.Description = $"<dl>\r\n<dt><i class=\"icon fas fa-people-arrows mr-2\"></i>Teendő a felhasználó jelenlegi jogosultságaival</dt>\r\n<dd>{currentPermissions}</dd>\r\n</dl>";

                // Csoport jogosultságainak hozzáadása
                string groupWindowsPermissions = string.Join("; ", group.GroupPermissions.Where(gp => gp.Permission.Type == "Windows" && !gp.Permission.IsArchived).Select(gp => gp.Permission.ActiveDirectoryPermissions));
                request.WindowsPermissions = !string.IsNullOrEmpty(groupWindowsPermissions) ? groupWindowsPermissions : null;

                string groupFonixPermissions = string.Join("; ", group.GroupPermissions.Where(gp => gp.Permission.Type == "Főnix 3" && !gp.Permission.IsArchived).Select(gp => gp.Permission.Name));
                request.FonixPermissions = !string.IsNullOrEmpty(groupFonixPermissions) ? groupFonixPermissions : null;

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

                // Értesítés küldése (amennyiben van e-mail cím)
                await _emailService.SendNewRequestNotification(request);

                // Igénylés megnyítása
                return RedirectToAction(nameof(Details), new { @id = request.RequestId });
            }

            // Amennyiben nem jók az értékek az oldal újratöltése
            ViewData["CreatedForUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

            ViewData["GroupId"] = new SelectList(_context.Groups.Where(g => !g.IsArchived).OrderBy(g => g.Name), "GroupId", "Name");

            // Felület újra megjelenítése, amennyiben az értékek hibásak voltak
            return View();
        }

        /// <summary>
        /// E-mail cím igénylés felülete
        /// </summary>
        public IActionResult Email()
        {
            // Lista elemek betöltése
            ViewData["CreatedForUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

            // Az oldal megjelenítése
            return View();
        }

        /// <summary>
        /// E-mail cím igénylés hozzáadása az adatbázishoz
        /// </summary>
        /// <param name="request">Igénylés értékei</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Email([Bind("RequestId,Type,Status,Description,CreatedByUserId,CreatedForUserId")] Request request)
        {
            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                // Igénylést létrehozó személy azonosítója
                request.CreatedByUserId = await GetLoggedUserId();

                // Igénylés létrehozásának dátuma
                request.CreatedDateTime = DateTime.Now;

                // Igénylés típusa
                request.Type = "E-mail cím igénylése";

                // Alapértelmezett státusz
                request.Status = "Folyamatban";

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

                // Értesítés küldése (amennyiben van e-mail cím)
                await _emailService.SendNewRequestNotification(request);

                // Igénylés megnyítása
                return RedirectToAction(nameof(Details), new { @id = request.RequestId });
            }

            // Amennyiben nem jók az értékek az oldal újratöltése
            ViewData["CreatedForUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

            // Felület újra megjelenítése, amennyiben az értékeket hibásan adták meg
            return View();
        }

        /// <summary>
        /// Telefonos PIN kód igénylés felülete
        /// </summary>
        public IActionResult Phone()
        {
            // Lista elemek betöltése
            try
            {
                ViewData["CreatedForUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
                {
                    u.UserId,
                    DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
                }), "UserId", "DisplayText");

                ViewData["PhoneId"] = new SelectList(_context.Phones.Where(p => !p.IsArchived).Where(p => p.PhoneUserId == null).OrderBy(p => p.PhoneCode), "PhoneId", "PhoneCode");
            }
            catch (RuntimeBinderException ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Felület megjelenítése
            return View();
        }

        /// <summary>
        /// Telefonos PIN kód igénylés hozzáadása az adatbázishoz
        /// </summary>
        /// <param name="request">Igénylés értékei</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Phone(int? selectedPIN, [Bind("RequestId,Type,Status,Description,CreatedByUserId,CreatedForUserId")] Request request)
        {
            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                // PIN kód megkeresése a rendszerben
                Phone? phone = await GetPhoneById(selectedPIN);

                // PIN kód meglétének ellenőrzése
                if (phone == null) return NotFound();

                // Személy beállítása
                phone.PhoneUserId = request.CreatedForUserId;

                try
                {
                    // PIN kód adatainak felülírása
                    _context.Update(phone);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhoneExists(phone.PhoneId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Igénylést létrehozó személy azonosítója
                request.CreatedByUserId = await GetLoggedUserId();

                // Igénylés létrehozásának dátuma
                request.CreatedDateTime = DateTime.Now;

                // Igénylés típusa
                request.Type = "Telefonos PIN kód igénylése";

                // Alapértelmezett státusz
                request.Status = "Folyamatban";

                // Kiválasztott pin kód hozzárendelése az igényléshez
                request.PhoneId = selectedPIN;

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

                // Értesítés küldése (amennyiben van e-mail cím)
                await _emailService.SendNewRequestNotification(request);

                // Igénylés megnyítása
                return RedirectToAction(nameof(Details), new { @id = request.RequestId });
            }

            // Amennyiben nem jók az értékek az oldal újratöltése
            try
            {
                ViewData["CreatedForUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
                {
                    u.UserId,
                    DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
                }), "UserId", "DisplayText");

                ViewData["PhoneId"] = new SelectList(_context.Phones.Where(p => !p.IsArchived).Where(p => p.PhoneUserId == null).OrderBy(p => p.PhoneCode), "PhoneId", "PhoneCode");
            }
            catch (RuntimeBinderException ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Felület megjelenítése
            return View();
        }

        /// <summary>
        /// Parkolási engedély igénylésének felülete
        /// </summary>
        public IActionResult Parking()
        {
            // Lista elemek betöltése
            ViewData["CreatedForUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

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
        public async Task<IActionResult> Parking(string? brand, string? modell, string? licensePlate, [Bind("RequestId,Type,Status,Description,CreatedByUserId,CreatedForUserId")] Request request)
        {
            // Autóval kapcsolatos adatok meglétének ellenőrzése
            if (string.IsNullOrEmpty(brand) || string.IsNullOrEmpty(modell) || string.IsNullOrEmpty(licensePlate)) return NotFound();

            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                // Új parkolási engedély létrehozása
                Parking? parking = new Parking();

                // Parkolási engedély adatainak elmentése
                parking.Brand = brand;
                parking.Modell = modell;
                parking.LicensePlate = licensePlate;
                parking.OwnerUserId = request.CreatedForUserId;

                try
                {
                    // Parkolási engedély hozzáadása az adatbázishoz
                    _context.Add(parking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParkingExists(parking.ParkingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Igénylést létrehozó személy azonosítója
                request.CreatedByUserId = await GetLoggedUserId();

                // Igénylés létrehozásának dátuma
                request.CreatedDateTime = DateTime.Now;

                // Igénylés típusa
                request.Type = "Parkolási engedély igénylése";

                // Alapértelmezett státusz
                request.Status = "Folyamatban";

                // Parkolási engedély hozzárendelése az igényléshez
                request.CarId = parking.ParkingId;

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

                // Értesítés küldése (amennyiben van e-mail cím)
                await _emailService.SendNewRequestNotification(request);

                // Igénylés megnyítása
                return RedirectToAction(nameof(Details), new { @id = request.RequestId });
            }

            // Amennyiben nem jók az értékek az oldal újratöltése
            ViewData["CreatedForUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

            // Felület megjelenítése
            return View();
        }

        /// <summary>
        /// Hikcentral jogosultság igénylésének felülete
        /// </summary>
        public IActionResult HikcentralPermission()
        {
            // Lista elemek betöltése
            ViewData["CreatedForUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

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
        public async Task<IActionResult> HikcentralPermission(string? permissionType, [Bind("RequestId,Type,Status,Description,CreatedByUserId,CreatedForUserId")] Request request)
        {
            // Autóval kapcsolatos adatok meglétének ellenőrzése
            if (string.IsNullOrEmpty(permissionType)) return NotFound();

            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                // Igénylést létrehozó személy azonosítója
                request.CreatedByUserId = await GetLoggedUserId();

                // Igénylés létrehozásának dátuma
                request.CreatedDateTime = DateTime.Now;

                // Igénylés típusa
                request.Type = "HikCentral jogosultság igénylése";

                // Alapértelmezett státusz
                request.Status = "Folyamatban";

                // Igénylés leírása
                request.Description = $"<dl>\r\n<dt><i class=\"icon fas fa-people-arrows mr-2\"></i>HikCentral jgoosultsági típus</dt>\r\n<dd>{permissionType}</dd>\r\n</dl>";

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

                // Értesítés küldése (amennyiben van e-mail cím)
                await _emailService.SendNewRequestNotification(request);

                // Igénylés megnyítása
                return RedirectToAction(nameof(Details), new { @id = request.RequestId });
            }

            // Amennyiben nem jók az értékek az oldal újratöltése
            ViewData["CreatedForUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

            // Felület megjelenítése
            return View();
        }

        /// <summary>
        /// Címkézett kamerafelvételek lementésének felülete
        /// </summary>
        public IActionResult RecordsByTags()
        {
            // Lista elemek betöltése
            ViewData["Cameras"] = new MultiSelectList(_context.Cameras.Where(c => !c.IsArchived).OrderBy(c => c.Name), "Name", "Name");
            ViewData["CreatedForUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

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
        public async Task<IActionResult> RecordsByTags(DateTime inputDate, string? inputWhy, string? inputTags, string[]? selectedCameras, [Bind("RequestId,Type,Status,Description,CreatedByUserId,CreatedForUserId")] Request request)
        {
            // Adatok meglétének ellenőrzése
            if (string.IsNullOrEmpty(inputWhy) || string.IsNullOrEmpty(inputTags) || selectedCameras == null) return NotFound();

            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                // Igénylést létrehozó személy azonosítója
                request.CreatedByUserId = await GetLoggedUserId();

                // Igénylés létrehozásának dátuma
                request.CreatedDateTime = DateTime.Now;

                // Igénylés típusa
                request.Type = "Kamerafelvétel lementése";

                // Alapértelmezett státusz
                request.Status = "Folyamatban";

                // Kamerák sorrendbe helyezése
                string cameras = string.Join(", ", selectedCameras);

                // Dátum átalakítása
                string inputDateString = inputDate.ToString("yyyy.MM.dd");

                // Igénylés leírása
                request.Description = $"<dl>\r\n<dt><i class=\"icon far fa-eye mr-2\"></i>Lementésének oka</dt>\r\n<dd>{inputWhy}</dd>\r\n<dt><i class=\"icon fas fa-calendar mr-2\"></i>Esemény dátuma</dt>\r\n<dd>{inputDateString}</dd>\r\n<dt><i class=\"icon fas fa-tags mr-2\"></i>Címkék megnevezése</dt>\r\n<dd>{inputTags}</dd>\r\n<dt><i class=\"icon fas fa-video mr-2\"></i>Megcímkézett kamerák</dt>\r\n<dd>{cameras}</dd>\r\n</dl>";

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

                // Értesítés küldése (amennyiben van e-mail cím)
                await _emailService.SendNewRequestNotification(request);

                // Igénylés megnyítása
                return RedirectToAction(nameof(Details), new { @id = request.RequestId });
            }

            // Amennyiben nem jók az értékek az oldal újratöltése
            ViewData["Cameras"] = new MultiSelectList(_context.Cameras.Where(c => !c.IsArchived).OrderBy(c => c.Name), "Name", "Name");
            ViewData["CreatedForUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

            // Felület megjelenítése
            return View();
        }

        /// <summary>
        /// Időpont alapú kamerafelvételek felülete
        /// </summary>
        public IActionResult RecordsByTime()
        {
            // Lenyíló menü elemeinek lekérdezése
            ViewData["Cameras"] = new MultiSelectList(_context.Cameras.Where(c => !c.IsArchived).OrderBy(c => c.Name), "Name", "Name");
            ViewData["CreatedForUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

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
        public async Task<IActionResult> RecordsByTime(string? inputWhy, string? recordsArray, [Bind("RequestId,Type,Status,Description,CreatedByUserId,CreatedForUserId")] Request request)
        {
            // Adatok meglétének ellenőrzése
            if (string.IsNullOrEmpty(inputWhy) || string.IsNullOrEmpty(recordsArray)) return NotFound();

            // Felvételek tömbösítése
            string[][]? records = JsonConvert.DeserializeObject<string[][]?>(recordsArray);

            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                // Igénylést létrehozó személy azonosítója
                request.CreatedByUserId = await GetLoggedUserId();

                // Igénylés létrehozásának dátuma
                request.CreatedDateTime = DateTime.Now;

                // Igénylés típusa
                request.Type = "Kamerafelvétel lementése";

                // Alapértelmezett státusz
                request.Status = "Folyamatban";

                // Igénylés leírása
                request.Description = $"<dl>\r\n<dt><i class=\"icon far fa-eye mr-2\"></i>Kamerafelvétel lementésének oka</dt>\r\n<dd>{inputWhy}</dd>\r\n</dl><br />" +
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

                // Értesítés küldése (amennyiben van e-mail cím)
                await _emailService.SendNewRequestNotification(request);

                // Igénylés megnyítása
                return RedirectToAction(nameof(Details), new { @id = request.RequestId });
            }

            // Amennyiben nem jók az értékek az oldal újratöltése
            ViewData["Cameras"] = new MultiSelectList(_context.Cameras.Where(c => !c.IsArchived).OrderBy(c => c.Name), "Name", "Name");
            ViewData["CreatedForUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName).Select(u => new
            {
                u.UserId,
                DisplayText = $"{u.DisplayName} bv.{u.Rank.ToLower()} ({u.Position.Name})"
            }), "UserId", "DisplayText");

            // Felület megjelenítése
            return View();
        }

        /// <summary>
        /// Igénylés meglétének ellenőrzése
        /// </summary>
        /// <param name="id">Igénylés azonosítója</param>
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
        /// Felhasználó megkeresése a rendszerben id által
        /// </summary>
        /// <param name="id">Felhasználó azonosítója</param>
        /// <returns>Felhasználó</returns>
        private async Task<User?> GetUserById(int? id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        }


        /// <summary>
        /// Csoport megkeresése a rendszerben csoport azonosító által
        /// </summary>
        /// <param name="id">Csoport azonosítója</param>
        /// <returns>Csoport</returns>
        private async Task<Group?> GetGroupById(int? id)
        {
            return await _context.Groups.Include(g => g.GroupPermissions).ThenInclude(gp => gp.Permission).FirstOrDefaultAsync(g => g.GroupId == id);
        }

        /// <summary>
        /// PIN kód megkeresése a rendszerben a kiválasztott PIN kód alapján
        /// </summary>
        /// <param name="id">PIN kód azonosítója</param>
        /// <returns>PIN kód az adatbázisban</returns>
        private async Task<Phone?> GetPhoneById(int? id)
        {
            return await _context.Phones.FirstOrDefaultAsync(p => p.PhoneId == id);
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

        /// <summary>
        /// PIN kód meglétének ellenőrzése
        /// </summary>
        /// <param name="id">PIN kód azonosítója</param>
        /// <returns>Létezik e az adott PIN kód (igaz/hamis)</returns>
        private bool PhoneExists(int id)
        {
            return (_context.Phones?.Any(e => e.PhoneId == id)).GetValueOrDefault();
        }

        /// <summary>
        /// Parkolási engedély meglétének ellenőrzése
        /// </summary>
        /// <param name="id">Parkolási engedély azonosítója</param>
        /// <returns>Létezik e az adott parkolási engedély (igaz/hamis)</returns>
        private bool ParkingExists(int id)
        {
            return (_context.Parkings?.Any(e => e.ParkingId == id)).GetValueOrDefault();
        }

        /// <summary>
        /// Az ISZR rendszerben kikeresésre kerül a felhasználó azonosítója
        /// </summary>
        /// <returns>ISZR felhasználói azonosító</returns>
        private async Task<int?> GetUserIdAsync(DataContext _context)
        {
            // Felhasználói azonosító megkeresése a rendszerben
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Username == User.Identity.Name);
            return user.UserId;
        }
    }
}