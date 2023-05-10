using ISZR.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISZR.Web.Controllers
{
    /// <summary>
    /// /Welcome/? Controller
    /// </summary>
    public class WelcomeController : Controller
    {
        private readonly DataContext _context;

        public WelcomeController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Üdvözlő felület megjelenítése
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            // Az oldalt megnyító felhasználó kikeresése az adatbázisban
            User? user = await GetLoggedUser();

            // Felhasználó meglétének és belépés számának ellenőrzése
            if (user != null && user.LogonCount > 0)
            {
                // Amennyiben a felhasználó létrezik, annak új belépési idejének mentése és számláló hozzáadása
                user.LastLogin = DateTime.Now;
                user.LogonCount++;

                try
                {
                    // Bejelentkezett felhasználó értékeinek frissítése
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Bejelentkezett felhasználó átírányítása az irányítópultra
                return RedirectToAction("Dashboard", "Home");
            }

            // Regisztrációhoz szükséges listák értékeinek betöltése
            ViewData["ClassId"] = new SelectList(_context.Classes.Where(u => !u.IsArchived).OrderBy(u => u.Name), "ClassId", "Name");
            ViewData["PositionId"] = new SelectList(_context.Positions.Where(u => !u.IsArchived).OrderBy(u => u.Name), "PositionId", "Name");

            // Felület megjelenítése a felhasználó értékeivel amennyiben a felhasználó létezik de még egyszer sem lépett be
            if (user?.LogonCount == 0) return View(user);

            // Felület megjelenítése amennyiben a felhasználó nem létezik
            return View();
        }

        /// <summary>
        /// Üdvözlő felületen új felhasználói profil létrehozása
        /// </summary>
        /// <param name="user">Felhasználó megadott értékei</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("UserId,Username,DisplayName,Email,Location,Phone,Rank,LastLogin,ClassId,PositionId,Genre")] User user)
        {
            // Megadott értékek ellenőrzése
            if (ModelState.IsValid)
            {
                // Felhasználó megkeresése az adatbázisban
                User? foundUser = await CheckUsername(user.Username);

                // Amennyiben a felhasználó még nem létezik az adatbázisban
                if (foundUser == null)
                {
                    // Új felasználó felhasználónevének lekérdezése
                    user.Username = GetUsername();

                    // Alapértelmezett bejelentkezési szám megnövelése
                    user.LogonCount++;

                    try
                    {
                        // Felhasználó hozzáadása az adatbázishoz
                        _context.Add(user);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!UserExists(user.UserId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    // Amennyiben a felhasználó létezik annak értékeinek felülírása a megadottakkal
                    foundUser.DisplayName = user.DisplayName;
                    foundUser.Rank = user.Rank;
                    foundUser.Class = user.Class;
                    foundUser.ClassId = user.ClassId;
                    foundUser.Position = user.Position;
                    foundUser.PositionId = user.PositionId;
                    foundUser.Location = user.Location;
                    foundUser.Phone = user.Phone;
                    foundUser.Email = user.Email;
                    foundUser.Genre = user.Genre;

                    // Bejelentkezési szám megnövelése
                    foundUser.LogonCount++;

                    try
                    {
                        // Felhasználó frissítése az adatbázisban
                        _context.Update(foundUser);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!UserExists(user.UserId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                // Felhasználó átírányítása az irányítópultra
                return RedirectToAction("Dashboard", "Home");
            }

            // Regisztrációhoz szükséges listák értékeinek betöltése
            ViewData["ClassId"] = new SelectList(_context.Classes.Where(u => !u.IsArchived).OrderBy(u => u.Name), "ClassId", "Name");
            ViewData["PositionId"] = new SelectList(_context.Positions.Where(u => !u.IsArchived).OrderBy(u => u.Name), "PositionId", "Name");

            // Felület újra megjelenítése, amennyiben a felhasználó hibás értékeket adott meg
            return View(user);
        }

        /// <summary>
        /// Bejelentkezett felhasználó megkeresése a rendszerben
        /// </summary>
        /// <returns>Bejelentkezett felhasználó adatai</returns>
        private async Task<User?> GetLoggedUser()
        {
            // Felhasználónév lekérése a számítógéptől
            string? activeUsername = GetUsername();

            // Amennyiben nem található a rendszerben felhasználónév (pl linux), kérelem elutasítása
            if (activeUsername == null) return null;

            // Megtalált felhasználó visszaadása (amennyiben nem talált, null értéket fog visszaadni)
            return await _context.Users
                .Include(u => u.Class)
                .Include(u => u.Position)
                .FirstOrDefaultAsync(m => m.Username == activeUsername);
        }

        /// <summary>
        /// Bejelentkezett felhasználó felhasználónevének lekérdezése
        /// </summary>
        /// <returns>Felhasználó felhasználóneve</returns>
        private string? GetUsername()
        {
            return User.Identity?.Name;
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
        /// Felhasználó megkeresése a rendszerben
        /// </summary>
        /// <param name="username">Felhasználó név</param>
        /// <returns>Felhasználó amennyiben létezik</returns>
        private async Task<User?> CheckUsername(string? username)
        {
            // Felhasználónév meglétének ellenőrzése
            if (username == null) return null;

            // Megtalált felhasználó visszaadása (amennyiben nem talált, nul értéket fog visszaadni
            return await _context.Users
                .Include(u => u.Class)
                .Include(u => u.Position)
                .FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}