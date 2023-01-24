using ISZR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISZR.Controllers
{
    public class RequestsController : Controller
    {
        private readonly DataContext _context;

        public RequestsController(DataContext context)
        {
            _context = context;
        }

        // GET: Requests
        public async Task<IActionResult> Index()
        {
            // Dashboard informations
            ViewBag.All = _context.Requests.Count();

            // Dashboard informations
            ViewBag.All = _context.Requests.Count();
            ViewBag.InProgress = _context.Requests.Where(r => r.Status == "Folyamatban").Count();
            ViewBag.Done = _context.Requests.Where(r => r.Status == "Végrehajtva").Count();
            ViewBag.Denied = _context.Requests.Where(r => r.Status == "Elutasítva").Count();

            var dataContext = _context.Requests.Include(r => r.RequestAuthor).Include(r => r.RequestFor);
            return View(await dataContext.ToListAsync());
        }

        // GET: Permissions
        public IActionResult Permissions()
        {
            ViewData["RequestForId"] = new SelectList(_context.Users, "UserId", "DisplayName");
            return View();
        }

        // GET: Requests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Requests == null) return NotFound();

            var request = await _context.Requests
                .Include(r => r.RequestAuthor)
                .Include(r => r.RequestFor)
                .FirstOrDefaultAsync(m => m.RequestId == id);

            if (request == null) return NotFound();
            return View(request);
        }

        // GET: Requests/Create
        public IActionResult Create()
        {
            ViewData["ClassId"] = new SelectList(_context.Classes, "ClassId", "Name");
            ViewData["RequestAuthorId"] = new SelectList(_context.Users, "UserId", "DisplayName");
            ViewData["RequestForId"] = new SelectList(_context.Users, "UserId", "DisplayName");
            return View();
        }

        // POST: Requests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestId,Type,Status,Description,RequestedPermissions,RequestAuthorId,RequestForId")] Request request)
        {
            if (ModelState.IsValid)
            {
                request.CreationDate = DateTime.Now;
                _context.Add(request);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RequestAuthorId"] = new SelectList(_context.Users, "UserId", "DisplayName", request.RequestAuthorId);
            ViewData["RequestForId"] = new SelectList(_context.Users, "UserId", "DisplayName", request.RequestForId);
            return View(request);
        }

        // GET: Requests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Requests == null)
            {
                return NotFound();
            }

            var request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            ViewData["RequestAuthorId"] = new SelectList(_context.Users, "UserId", "DisplayName", request.RequestAuthorId);
            ViewData["RequestForId"] = new SelectList(_context.Users, "UserId", "DisplayName", request.RequestForId);
            return View(request);
        }

        // POST: Requests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RequestId,Type,Status,Description,RequestedPermissions,RequestAuthorId,RequestForId")] Request request)
        {
            if (id != request.RequestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["RequestAuthorId"] = new SelectList(_context.Users, "UserId", "DisplayName", request.RequestAuthorId);
            ViewData["RequestForId"] = new SelectList(_context.Users, "UserId", "DisplayName", request.RequestForId);
            return View(request);
        }

        // GET: Requests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Requests == null)
            {
                return NotFound();
            }

            var request = await _context.Requests
                .Include(r => r.RequestAuthor)
                .Include(r => r.RequestFor)
                .FirstOrDefaultAsync(m => m.RequestId == id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // POST: Requests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Requests == null)
            {
                return Problem("Entity set 'DataContext.Request'  is null.");
            }
            var request = await _context.Requests.FindAsync(id);
            if (request != null)
            {
                _context.Requests.Remove(request);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RequestExists(int id)
        {
            return _context.Requests.Any(e => e.RequestId == id);
        }
    }
}