using ISZR.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISZR.Web.Controllers
{
	public class ReportsController : Controller
	{
		private readonly DataContext _context;

		public ReportsController(DataContext context)
		{
			_context = context;
		}

		// GET: Reports
		public async Task<IActionResult> Index()
		{
			// Felhasználó alapú szűréshez lista
			ViewData["ReportUserId"] = new SelectList(_context.Users.Where(u => !u.IsArchived).OrderBy(u => u.DisplayName), "UserId", "DisplayName");

			var dataContext = _context.Reports.Include(r => r.ReportUser);
			return View(await dataContext.ToListAsync());
		}

		/// <summary>
		/// Hibabejelentés állapotának megváltoztatása
		/// </summary>
		/// <param name="id">Hibabejelentés azonosítója</param>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(int? id)
		{
			// Azonosító meglétének ellenőrzése
			if (id == null || _context.Reports == null) return NotFound();

			// Hibabejelentés megkeresése az adatbázisban
			var report = await _context.Reports.FirstOrDefaultAsync(m => m.ReportId == id);

			// Hibabejelentés meglétének ellenőrzése
			if (report == null) return NotFound();

			try
			{
				// Állapot értékének módosítása
				report.IsSolved = !report.IsSolved;

				// Hibabejelentés értékeinek frissítése az adatbázisban
				_context.Update(report);
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ReportExists(report.ReportId))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

			// Felület újra megjelenítése
			return RedirectToAction(nameof(Index));
		}

		// GET: Reports/Create
		public IActionResult Create()
		{
			ViewData["ReportUserId"] = new SelectList(_context.Users, "UserId", "DisplayName");
			return View();
		}

		// POST: Reports/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ReportId,ReportUserId,Title,Description,IsSolved")] Report report)
		{
			if (ModelState.IsValid)
			{
				// Hibabejelentést létrehozó személy azonosítója
				report.ReportUserId = await GetLoggedUserId();

				_context.Add(report);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["ReportUserId"] = new SelectList(_context.Users, "UserId", "DisplayName", report.ReportUserId);
			return View(report);
		}

		/// <summary>
		/// Bejelentkezett felhasználó azonosítójának megszerzése
		/// </summary>
		/// <returns>Felhasználói azonosító</returns>
		private async Task<int?> GetLoggedUserId()
		{
			// Felhasználónév lekérdezése a számítógéptől
			string? activeUsername = User.Identity?.Name;
			if (activeUsername == null) return null;

			// Felhasználó megkeresése az adatbázisban
			var foundUser = await _context.Users
				.Include(u => u.Class)
				.Include(u => u.Position)
				.FirstOrDefaultAsync(m => m.Username == activeUsername);

			// Felhasználó meglétének ellenőrzése
			if (foundUser == null) return null;

			// Return user id
			return foundUser.UserId;
		}

		/// <summary>
		/// Hibabejelentés meglétének ellenőrzése
		/// </summary>
		/// <param name="id">Hibabejelentés azonosítója</param>
		/// <returns>Létezik e az adott hibabejelentés (igaz/hamis)</returns>
		private bool ReportExists(int id)
		{
			return _context.Reports.Any(e => e.ReportId == id);
		}
	}
}