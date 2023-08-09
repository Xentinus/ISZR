using ISZR.Web.Data;
using ISZR.Web.Models;
using ISZR.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            return View();
        }

        public async Task<IActionResult> Profile(int? userId)
        {
            // Amennyiben az oldal nem tartalmaz azonosítót
            if (userId == null) return Forbid();

            // Felhasználó megkeresése a rendszerben
            User? user = await GetUserById(userId);

            // Amennyiben a felhasználó nem található az adatbázisban
            if (user == null) return Forbid();

            // Felület elkészítése
            var viewModel = new DashboardViewModel { User = user };

            // Felhasználó aktív parkolási engedélyeinek összeszedése
            viewModel.Parkings = _context.Parkings.Where(p => p.OwnerUserId == user.UserId && !p.IsArchived).OrderBy(p => p.LicensePlate).ToList();

            // Felhasználó által használt aktív PIN kódok
            viewModel.Phones = _context.Phones.Where(p => p.PhoneUserId == user.UserId && !p.IsArchived).OrderBy(p => p.PhoneCode).ToList();

            // Irányítópult megjelenítése a felhasználónak
            return View(viewModel);
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
    }
}
