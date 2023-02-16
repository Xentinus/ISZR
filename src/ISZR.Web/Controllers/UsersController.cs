﻿using ISZR.Web.Components;
using ISZR.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISZR.Web.Controllers
{
	public class UsersController : Controller
	{
		private readonly DataContext _context;

		public UsersController(DataContext context)
		{
			_context = context;
		}

		// GET: Users
		public async Task<IActionResult> Index()
		{
			// Admin jogosultság ellenőrzése
			if (!Account.IsAdmin()) return Forbid();

			// Dashboard informations
			ViewBag.All = _context.Users.Count();
			ViewBag.Active = _context.Users.Where(u => u.IsArchived == false).Count();
			ViewBag.Archived = ViewBag.All - ViewBag.Active;

			var dataContext = _context.Users.Include(u => u.Class).Include(u => u.Position);
			return View(await dataContext.ToListAsync());
		}

		// GET: Users/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			// Admin jogosultság ellenőrzése
			if (!Account.IsAdmin()) return Forbid();

			if (id == null || _context.Users == null)
			{
				return NotFound();
			}

			var user = await _context.Users
				.Include(u => u.Class)
				.Include(u => u.Position)
				.FirstOrDefaultAsync(m => m.UserId == id);
			if (user == null)
			{
				return NotFound();
			}

			return View(user);
		}

		// GET: Users/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			// Admin jogosultság ellenőrzése
			if (!Account.IsAdmin()) return Forbid();

			if (id == null || _context.Users == null)
			{
				return NotFound();
			}

			var user = await _context.Users.FindAsync(id);
			if (user == null)
			{
				return NotFound();
			}
			ViewData["ClassId"] = new SelectList(_context.Set<Class>(), "ClassId", "Name", user.ClassId);
			ViewData["PositionId"] = new SelectList(_context.Set<Position>(), "PositionId", "Name", user.PositionId);
			return View(user);
		}

		// POST: Users/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("UserId,Username,DisplayName,Email,Phone,Rank,Location,LastLogin,LogonCount,ClassId,PositionId")] User user)
		{
			if (id != user.UserId) return NotFound();

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
				return RedirectToAction(nameof(Index));
			}
			ViewData["ClassId"] = new SelectList(_context.Set<Class>(), "ClassId", "Name", user.ClassId);
			ViewData["PositionId"] = new SelectList(_context.Set<Position>(), "PositionId", "Name", user.PositionId);
			return View(user);
		}

		private bool UserExists(int id)
		{
			return _context.Users.Any(e => e.UserId == id);
		}
	}
}