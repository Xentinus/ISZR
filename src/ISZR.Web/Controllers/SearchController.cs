using ISZR.Web.Data;
using ISZR.Web.Models;
using ISZR.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace ISZR.Web.Controllers
{
    /// <summary>
    /// /Search/? Controller
    /// </summary>
    [Authorize(Policy = "Ugyintezo")]
    public class SearchController : Controller
    {
        private readonly DataContext _context;

        public SearchController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searching)
        {
            ViewData["searching"] = searching;

            // Üres keresés alapján
            if (string.IsNullOrEmpty(searching)) return View();

            ViewData["foundUsers"] = await _context.Users
                .Where(u => u.DisplayName.Contains(searching) && !u.IsArchived)
                .Include(u => u.Class)
                .Include(u => u.Position)
                .OrderBy(u => u.DisplayName)
                .ToListAsync();

            ViewData["foundRequests"] = await _context.Requests
                .Where(r => r.RequestId.ToString().Contains(searching))
                .Include(r => r.CreatedForUser)
                .Include(r => r.CreatedForUser.Position)
                .OrderBy(r => r.RequestId)
                .ToListAsync();

            ViewData["foundPermissions"] = await _context.Permissions
                .Where(p => p.Name.Contains(searching) || p.Description.Contains(searching) || p.ActiveDirectoryPermissions.Contains(searching) && !p.IsArchived)
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewData["foundParkings"] = await _context.Parkings
                .Where(p => p.LicensePlate.Contains(searching) || p.OwnerUser.DisplayName.Contains(searching) && !p.IsArchived)
                .Include(p => p.OwnerUser)
                .Include(p => p.OwnerUser.Position)
                .ToListAsync();

            ViewData["foundGroups"] = await _context.Groups
                .Where(g => g.Name.Contains(searching))
                .Include(g => g.GroupPermissions)
                .ThenInclude(gp => gp.Permission)
                .ToListAsync();

            return View();
        }

        public async Task<IActionResult> Profile(int? userId)
        {
            // Amennyiben az oldal nem tartalmaz azonosítót
            if (userId == null) return NotFound();

            // Felhasználó megkeresése a rendszerben
            User? user = await GetUserById(userId);

            // Amennyiben a felhasználó nem található az adatbázisban
            if (user == null) return NotFound();

            // Felület elkészítése
            var viewModel = new DashboardViewModel { User = user };

            // Felhasználó aktív parkolási engedélyeinek összeszedése
            viewModel.Parkings = await _context.Parkings
                .Where(p => p.OwnerUserId == user.UserId && !p.IsArchived)
                .OrderBy(p => p.LicensePlate)
                .ToListAsync();

            // Felhasználó által használt aktív PIN kódok
            viewModel.Phones = await _context.Phones
                .Where(p => p.PhoneUserId == user.UserId && !p.IsArchived)
                .OrderBy(p => p.PhoneCode)
                .ToListAsync();

            // Felhasználó utolsó igénylései
            viewModel.LastRequests = await _context.Requests
                .Where(r => r.CreatedForUser == user || r.CreatedByUser == user)
                .OrderByDescending(r => r.RequestId)
                .Take(8)
                .ToListAsync();

            // Lenyíló menük adatainak betöltése
            ViewData["ClassId"] = new SelectList(_context.Classes.Where(u => !u.IsArchived).OrderBy(u => u.Name), "ClassId", "Name");
            ViewData["PositionId"] = new SelectList(_context.Positions.Where(u => !u.IsArchived).OrderBy(u => u.Name), "PositionId", "Name");

            // Irányítópult megjelenítése a felhasználónak
            return View(viewModel);
        }

        /// <summary>
        /// Felhasználó elérhetőségének módosítása
        /// </summary>
        /// <param name="user">Megadott új felhasználói értékek</param>
        /// <param name="id">Felhasználó azonosítója</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile([Bind("UserId,Username,DisplayName,Rank,Genre,Location,Email,Phone,LastLogin,ClassId,PositionId,IsArchived")] User user, int? id)
        {
            // Azonosító meglétének ellenőrzése
            if (id == null || user.UserId != id || _context.Users == null) return NotFound();

            // Felhasználó megkeresése az adatbázisban
            var foundUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            // Felhasználó meglétének ellenőrzése
            if (foundUser == null) return NotFound();

            try
            {
                // Felhasználó adatainak felülírása a megadott értékekkel
                foundUser.DisplayName = user.DisplayName;
                foundUser.Genre = user.Genre;
                foundUser.Rank = user.Rank;
                foundUser.ClassId = user.ClassId;
                foundUser.PositionId = user.PositionId;
                foundUser.Location = user.Location;
                foundUser.Email = user.Email;
                foundUser.Phone = user.Phone;

                _context.Update(foundUser);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(foundUser.UserId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Felület újra megjelenítése
            return RedirectToAction(nameof(Profile), new { userId = foundUser.UserId });
        }

        /// <summary>
        /// Felhasználó megkeresése a rendszerben
        /// </summary>
        /// <returns>Felhasználó adatai</returns>
        private async Task<User?> GetUserById(int? id)
        {
            // Amennyiben nem érkezik id
            if (id == null) return null;

            // Felhasználó megkeresése a lekért id által
            return await _context.Users
                .Include(u => u.Class)
                .Include(u => u.Position)
                .FirstOrDefaultAsync(m => m.UserId == id);
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
    }
}
