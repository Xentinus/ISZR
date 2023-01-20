using ISZR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ISZR.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly DataContext _context;

        public AdministratorController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get username from PC
            string? activeUsername = User.Identity?.Name;
            if (activeUsername == null) return Forbid();

            // Regex for username
            if (!Regex.IsMatch(activeUsername, @"^XENTINUS-LAPTOP") || Regex.IsMatch(activeUsername, @"\.admin$")) return Forbid();

            // Request informations
            ViewBag.AllRequest = _context.Requests.Count();
            ViewBag.AllInProgressRequest = _context.Requests.Where(r => r.Status == "Folyamatban").Count();
            ViewBag.AllDoneRequest = _context.Requests.Where(r => r.Status == "Végrehajtva").Count();
            ViewBag.AllDeniedRequest = _context.Requests.Where(r => r.Status == "Elutasítva").Count();

            // Return Requests
            var dataContext = _context.Requests.Include(r => r.Class).Include(r => r.RequestAuthor).Include(r => r.RequestFor);
            return View(await dataContext.ToListAsync());
        }

        // GET: Requests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Requests == null) return NotFound();

            var request = await _context.Requests
                .Include(r => r.Class)
                .Include(r => r.RequestAuthor)
                .Include(r => r.RequestFor)
                .FirstOrDefaultAsync(m => m.RequestId == id);

            if (request == null) return NotFound();
            return View(request);
        }
    }
}
