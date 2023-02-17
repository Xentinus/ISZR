using ISZR.Web.Components;
using ISZR.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ISZR.Web.Controllers
{
	public class ClassesController : Controller
	{
		private readonly DataContext _context;

		public ClassesController(DataContext context)
		{
			_context = context;
		}

		// GET: Classes
		public async Task<IActionResult> Index()
		{
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Admin jogosultság ellenőrzése
			if (!Account.IsAdmin()) return Forbid();

			// Dashboard informations
			ViewBag.All = _context.Classes.Count();
			ViewBag.Active = _context.Classes.Where(c => c.IsArchived == false).Count();
			ViewBag.Archived = ViewBag.All - ViewBag.Active;

			return View(await _context.Classes.ToListAsync());
		}

		// GET: Classes/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Admin jogosultság ellenőrzése
			if (!Account.IsAdmin()) return Forbid();

			if (id == null || _context.Classes == null) return NotFound();

			var @class = await _context.Classes
				.FirstOrDefaultAsync(m => m.ClassId == id);

			if (@class == null) return NotFound();

			///
			//@class.Users = await _context.User
			//   .Where(u => u.ClassId == id)
			//  .ToListAsync();

			return View(@class);
		}

		// GET: Classes/Create
		public IActionResult Create()
		{
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Admin jogosultság ellenőrzése
			if (!Account.IsAdmin()) return Forbid();

			return View();
		}

		// POST: Classes/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ClassId,Name")] Class @class)
		{
			if (ModelState.IsValid)
			{
				_context.Add(@class);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(@class);
		}

		// GET: Classes/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Admin jogosultság ellenőrzése
			if (!Account.IsAdmin()) return Forbid();

			if (id == null || _context.Classes == null)
			{
				return NotFound();
			}

			var @class = await _context.Classes.FindAsync(id);
			if (@class == null)
			{
				return NotFound();
			}
			return View(@class);
		}

		// POST: Classes/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("ClassId,Name")] Class @class)
		{
			if (id != @class.ClassId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(@class);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ClassExists(@class.ClassId))
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
			return View(@class);
		}

		// GET: Classes/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			// ISZR használati jog ellenőrzése
			if (!Account.IsUser()) return Forbid();

			// Admin jogosultság ellenőrzése
			if (!Account.IsAdmin()) return Forbid();

			if (id == null || _context.Classes == null)
			{
				return NotFound();
			}

			var @class = await _context.Classes
				.FirstOrDefaultAsync(m => m.ClassId == id);
			if (@class == null)
			{
				return NotFound();
			}

			return View(@class);
		}

		// POST: Classes/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (_context.Classes == null)
			{
				return Problem("Entity set 'DataContext.Class'  is null.");
			}
			var @class = await _context.Classes.FindAsync(id);
			if (@class != null)
			{
				_context.Classes.Remove(@class);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool ClassExists(int id)
		{
			return _context.Classes.Any(e => e.ClassId == id);
		}
	}
}