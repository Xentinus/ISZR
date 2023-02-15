using ISZR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
			// Get username from pc
			string? activeUsername = User.Identity?.Name;
			if (activeUsername == null) return NotFound();

			// Looking for user
			var user = await _context.Users.FirstOrDefaultAsync(m => m.Username == activeUsername);

			// Checking user exits and first login
			if (user != null && user.LogonCount > 0)
			{
				try
				{
					// Update Last login time
					user.LastLogin = DateTime.Now;
					// Update login count
					user.LogonCount++;

					// Update user
					_context.Update(user);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					throw;
				}

				// Redirect to Dashboard
				return RedirectToAction("Dashboard", "Home");
			}

			// Display registration page
			ViewData["ClassId"] = new SelectList(_context.Set<Class>(), "ClassId", "Name");
			ViewData["PositionId"] = new SelectList(_context.Set<Position>(), "PositionId", "Name");

			// Return full form if user exists (first login)
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
				// Get username from pc
				string? activeUsername = User.Identity?.Name;
				if (activeUsername == null) return NotFound();

				// Looking for user
				var foundUser = await _context.Users
					.Include(u => u.Class)
					.Include(u => u.Position)
					.FirstOrDefaultAsync(m => m.Username == activeUsername);

				if (foundUser == null)
				{
					user.Username = activeUsername;

					// Add login count
					user.LogonCount++;

					// Update user
					_context.Add(user);
					await _context.SaveChangesAsync();
				}
				else
				{
					// Update Pre created user informations with new one
					foundUser.DisplayName = user.DisplayName;
					foundUser.Rank = user.Rank;
					foundUser.Class = user.Class;
					foundUser.ClassId = user.ClassId;
					foundUser.Position = user.Position;
					foundUser.PositionId = user.PositionId;
					foundUser.Phone = user.Phone;
					foundUser.Email = user.Email;
					foundUser.Location = user.Location;

					// Add login count
					foundUser.LogonCount++;

					// Update user
					_context.Update(foundUser);
					await _context.SaveChangesAsync();
				}

				// Redirect to Dashboard
				return RedirectToAction("Dashboard", "Home");
			}

			// Display registration page
			ViewData["ClassId"] = new SelectList(_context.Set<Class>(), "ClassId", "Name", user.ClassId);
			ViewData["PositionId"] = new SelectList(_context.Set<Position>(), "PositionId", "Name", user.PositionId);

			// Return full form
			return View(user);
		}
	}
}