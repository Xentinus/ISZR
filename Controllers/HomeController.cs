using ISZR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace ISZR.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _context;

        public HomeController(DataContext context)
        {
            _context = context;
        }

        // GET: Home/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // get username
            string? activeUsername = User.Identity?.Name;

            // looking for user data
            var user = await _context.Users
                .Include(u => u.Class)
                .Include(u => u.Position)
                .FirstOrDefaultAsync(m => m.Username == activeUsername);

            // user not exist
            if (user == null) return RedirectToAction("Index", "Welcome");

            // save user data
            ViewBag.CurrentUser = user;

            ViewBag.AllRequest = _context.Requests.Count();
            ViewBag.AllRequestUser = _context.Requests.Where(r => r.RequestAuthor == user).Count();

            // return page
            return View(user);
        }

        // GET: Home/Settings
        public async Task<IActionResult> Settings()
        {
            string? activeUsername = User.Identity?.Name;

            var user = await _context.Users
                .Include(u => u.Class)
                .Include(u => u.Position)
                .FirstOrDefaultAsync(m => m.Username == activeUsername);

            if (user == null) RedirectToAction("Index", "Welcome");

            // Display registration page
            ViewData["ClassId"] = new SelectList(_context.Set<Class>(), "ClassId", "Name");
            ViewData["PositionId"] = new SelectList(_context.Set<Position>(), "PositionId", "Name");
            return View(user);
        }

        // POST: Settings/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings([Bind("UserId,Username,DisplayName,Email,Phone,Rank,Location,LastLogin,ClassId,PositionId")] User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
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
                return RedirectToAction(nameof(Dashboard));
            }
            ViewData["ClassId"] = new SelectList(_context.Set<Class>(), "ClassId", "Name", user.ClassId);
            ViewData["PositionId"] = new SelectList(_context.Set<Position>(), "PositionId", "Name", user.PositionId);
            return RedirectToAction(nameof(Dashboard));
        }

        public IActionResult FAQ()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}