using ISZR.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
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

			// Regex for username and development env
			if (!Regex.IsMatch(activeUsername, @"^(XENTINUS-|.*\.admin)$")) return Forbid();

			// Dashboard informations
			ViewBag.All = _context.Requests.Count();
			ViewBag.InProgress = _context.Requests.Where(r => r.Status == "Folyamatban").Count();
			ViewBag.Done = _context.Requests.Where(r => r.Status == "Végrehajtva").Count();
			ViewBag.Denied = _context.Requests.Where(r => r.Status == "Elutasítva").Count();

			// Return Requests
			var dataContext = _context.Requests.Include(r => r.RequestAuthor).Include(r => r.RequestFor).OrderByDescending(r => r.RequestId);
			return View(await dataContext.ToListAsync());
		}

		// GET: Administrator/Details/5
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

		// POST: Administrator/Details/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Details(int? id, string? status)
		{
			if (id == null || status == null || _context.Requests == null) return NotFound();

			var request = await _context.Requests
				.Include(r => r.RequestAuthor)
				.Include(r => r.RequestFor)
				.Include(r => r.RequestAuthor.Class)
				.Include(r => r.RequestAuthor.Position)
				.Include(r => r.RequestFor.Class)
				.Include(r => r.RequestFor.Position)
				.FirstOrDefaultAsync(m => m.RequestId == id);

			if (request == null) return NotFound();

			try
			{
				request.Status = status;
				if (status == "Folyamatban")
				{
					request.ResolveDate = new DateTime();
				}
				else
				{
					request.ResolveDate = DateTime.Now;
				}

				_context.Update(request);
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!RequestExists(request.RequestId))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

			return View(request);
		}

		private bool RequestExists(int id)
		{
			return _context.Requests.Any(e => e.RequestId == id);
		}
	}
}