using ISZR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISZR.Controllers
{
    public class SettingsController : Controller
    {
        private readonly DataContext _context;

        public SettingsController(DataContext context)
        {
            _context = context;
        }

        // GET: Settings/Index
        public async Task<IActionResult> Index()
        {
            string? activeUsername = User.Identity?.Name;

            var user = await _context.Users
                .Include(u => u.Class)
                .Include(u => u.Position)
                .FirstOrDefaultAsync(m => m.Username == activeUsername);

            if (user == null) RedirectToAction("Index", "Dashboard");

            // Display registration page
            ViewData["ClassId"] = new SelectList(_context.Set<Class>(), "ClassId", "Name");
            ViewData["PositionId"] = new SelectList(_context.Set<Position>(), "PositionId", "Name");
            return View(user);
        }

        // POST: Settings/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("UserId,Username,DisplayName,Email,Phone,Rank,Location,LastLogin,ClassId,PositionId")] User user)
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
                return RedirectToAction("Index", "Dashboard");
            }
            ViewData["ClassId"] = new SelectList(_context.Set<Class>(), "ClassId", "Name", user.ClassId);
            ViewData["PositionId"] = new SelectList(_context.Set<Position>(), "PositionId", "Name", user.PositionId);
            return RedirectToAction("Index", "Dashboard");
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}