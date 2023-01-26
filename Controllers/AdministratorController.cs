using Microsoft.AspNetCore.Mvc;
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

            // Dashboard informations
            ViewBag.All = _context.Requests.Count();
            ViewBag.InProgress = _context.Requests.Where(r => r.Status == "Folyamatban").Count();
            ViewBag.Done = _context.Requests.Where(r => r.Status == "Végrehajtva").Count();
            ViewBag.Denied = _context.Requests.Where(r => r.Status == "Elutasítva").Count();

            // Return Requests
            var dataContext = _context.Requests.Include(r => r.RequestAuthor).Include(r => r.RequestFor).OrderByDescending(r => r.RequestId);
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
    }
}