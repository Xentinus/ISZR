using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ISZR.Data;
using ISZR.Models;

namespace ISZR.Controllers
{
    public class FonixPermissionsController : Controller
    {
        private readonly ISZRContext _context;

        public FonixPermissionsController(ISZRContext context)
        {
            _context = context;
        }

        // GET: FonixPermissions
        public async Task<IActionResult> Index()
        {
            return View(await _context.FonixPermission.ToListAsync());
        }

        // GET: FonixPermissions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.FonixPermission == null)
            {
                return NotFound();
            }

            var fonixPermission = await _context.FonixPermission
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fonixPermission == null)
            {
                return NotFound();
            }

            return View(fonixPermission);
        }

        // GET: FonixPermissions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FonixPermissions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,CountryView,View,Edit")] FonixPermission fonixPermission)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fonixPermission);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(fonixPermission);
        }

        // GET: FonixPermissions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.FonixPermission == null)
            {
                return NotFound();
            }

            var fonixPermission = await _context.FonixPermission.FindAsync(id);
            if (fonixPermission == null)
            {
                return NotFound();
            }
            return View(fonixPermission);
        }

        // POST: FonixPermissions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CountryView,View,Edit")] FonixPermission fonixPermission)
        {
            if (id != fonixPermission.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fonixPermission);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FonixPermissionExists(fonixPermission.Id))
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
            return View(fonixPermission);
        }

        // GET: FonixPermissions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.FonixPermission == null)
            {
                return NotFound();
            }

            var fonixPermission = await _context.FonixPermission
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fonixPermission == null)
            {
                return NotFound();
            }

            return View(fonixPermission);
        }

        // POST: FonixPermissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.FonixPermission == null)
            {
                return Problem("Entity set 'ISZRContext.FonixPermission'  is null.");
            }
            var fonixPermission = await _context.FonixPermission.FindAsync(id);
            if (fonixPermission != null)
            {
                _context.FonixPermission.Remove(fonixPermission);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FonixPermissionExists(int id)
        {
            return _context.FonixPermission.Any(e => e.Id == id);
        }
    }
}