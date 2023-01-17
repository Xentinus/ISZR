using ISZR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ISZR.Controllers
{
    public class WelcomeController : Controller
    {
        private readonly DataContext _context;

        public WelcomeController(DataContext context)
        {
            _context = context;
        }

        // GET: Welcome/Index
        public async Task<IActionResult> Index()
        {
            string? activeUsername = User.Identity?.Name;

            var user = await _context.Users
                .Include(u => u.Class)
                .Include(u => u.Position)
                .FirstOrDefaultAsync(m => m.Username == activeUsername);

            if (user != null && user.LogonCount > 0)
            {
                // Update Last login time
                try
                {
                    user.LastLogin = DateTime.Now;
                    user.LogonCount++;
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }

                // Redirect to Dashboard
                return RedirectToAction("Index", "Dashboard");
            }

            // Display registration page
            ViewData["ClassId"] = new SelectList(_context.Set<Class>(), "ClassId", "Name");
            ViewData["PositionId"] = new SelectList(_context.Set<Position>(), "PositionId", "Name");

            // Return full form if user exists
            if (user?.LogonCount == 0) return View(user);

            // Return empty form
            return View();
        }

        // POST: Welcome/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("UserId,Username,DisplayName,Email,Phone,Rank,Location,LastLogin,ClassId,PositionId")] User user)
        {
            if (ModelState.IsValid)
            {
                string? activeUsername = User.Identity?.Name;

                var existsUser = await _context.Users
                    .Include(u => u.Class)
                    .Include(u => u.Position)
                    .FirstOrDefaultAsync(m => m.Username == activeUsername);

                if (existsUser == null)
                {
                    user.Username = User.Identity?.Name;
                    user.LogonCount = 1;
                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    existsUser.DisplayName = user.DisplayName;
                    existsUser.Rank = user.Rank;
                    existsUser.ClassId = user.ClassId;
                    existsUser.PositionId = user.PositionId;
                    existsUser.Phone = user.Phone;
                    existsUser.Email = user.Email;
                    existsUser.Location = user.Location;
                    existsUser.LogonCount = 1;
                    _context.Update(existsUser);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            // Display registration page
            ViewData["ClassId"] = new SelectList(_context.Set<Class>(), "ClassId", "Name", user.ClassId);
            ViewData["PositionId"] = new SelectList(_context.Set<Position>(), "PositionId", "Name", user.PositionId);

            // Return full form
            return View(user);
        }
    }
}