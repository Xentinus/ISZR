using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ISZR.Data;
using ISZR.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;

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

            var user = await _context.User
                .Include(u => u.Class)
                .Include(u => u.Position)
                .FirstOrDefaultAsync(m => m.Username == activeUsername);

            if (user != null)
            {
                // Update Last login time
                try
                {
                    user.LastLogin = DateTime.Now;
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
            return View();
        }

        // POST: Welcome/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("UserId,Username,DisplayName,Email,Phone,Rank,Location,LastLogin,ClassId,PositionId")] User user)
        {
            if (ModelState.IsValid)
            {
                user.Username = User.Identity?.Name;
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClassId"] = new SelectList(_context.Set<Class>(), "ClassId", "Name", user.ClassId);
            ViewData["PositionId"] = new SelectList(_context.Set<Position>(), "PositionId", "Name", user.PositionId);
            return View(user);
        }
    }
}