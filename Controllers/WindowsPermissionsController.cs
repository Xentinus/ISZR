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
    public class WindowsPermissionsController : Controller
    {
        private readonly DataContext _context;

        public WindowsPermissionsController(DataContext context)
        {
            _context = context;
        }

        // GET: WindowsPermissions
        public async Task<IActionResult> Index()
        {
            return View(await _context.WindowsPermission.ToListAsync());
        }

        // GET: WindowsPermissions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.WindowsPermission == null)
            {
                return NotFound();
            }

            var windowsPermission = await _context.WindowsPermission
                .FirstOrDefaultAsync(m => m.WindowsPermissionId == id);
            if (windowsPermission == null)
            {
                return NotFound();
            }

            return View(windowsPermission);
        }

        // GET: WindowsPermissions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: WindowsPermissions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WindowsPermissionId,Name,Description,Permission,Archived")] WindowsPermission windowsPermission)
        {
            if (ModelState.IsValid)
            {
                _context.Add(windowsPermission);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(windowsPermission);
        }

        // GET: WindowsPermissions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.WindowsPermission == null)
            {
                return NotFound();
            }

            var windowsPermission = await _context.WindowsPermission.FindAsync(id);
            if (windowsPermission == null)
            {
                return NotFound();
            }
            return View(windowsPermission);
        }

        // POST: WindowsPermissions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WindowsPermissionId,Name,Description,Permission,Archived")] WindowsPermission windowsPermission)
        {
            if (id != windowsPermission.WindowsPermissionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(windowsPermission);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WindowsPermissionExists(windowsPermission.WindowsPermissionId))
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
            return View(windowsPermission);
        }

        // GET: WindowsPermissions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.WindowsPermission == null)
            {
                return NotFound();
            }

            var windowsPermission = await _context.WindowsPermission
                .FirstOrDefaultAsync(m => m.WindowsPermissionId == id);
            if (windowsPermission == null)
            {
                return NotFound();
            }

            return View(windowsPermission);
        }

        // POST: WindowsPermissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.WindowsPermission == null)
            {
                return Problem("Entity set 'DataContext.WindowsPermission'  is null.");
            }
            var windowsPermission = await _context.WindowsPermission.FindAsync(id);
            if (windowsPermission != null)
            {
                _context.WindowsPermission.Remove(windowsPermission);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WindowsPermissionExists(int id)
        {
            return _context.WindowsPermission.Any(e => e.WindowsPermissionId == id);
        }
    }
}