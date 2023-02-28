using ISZR.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace ISZR.Web.Controllers
{
    public class GroupsController : Controller
    {
        private readonly DataContext _context;

        public GroupsController(DataContext context)
        {
            _context = context;
        }

        // GET: Groups
        public async Task<IActionResult> Index()
        {
            var dataContext = _context.Groups.Include(g => g.Class).OrderBy(g => g.Name);
            return View(await dataContext.ToListAsync());
        }

        // GET: Groups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Groups == null)
            {
                return NotFound();
            }

            var group = await _context.Groups
                .Include(g => g.Class)
                .FirstOrDefaultAsync(m => m.GroupId == id);
            if (group == null)
            {
                return NotFound();
            }

            ViewData["ClassId"] = new SelectList(_context.Classes, "ClassId", "Name", group.ClassId);
            return View(group);
        }

        // GET: Groups/Create
        public IActionResult Create()
        {
            ViewData["ClassId"] = new SelectList(_context.Classes, "ClassId", "Name");
            ViewData["Windows"] = new MultiSelectList(_context.Permissions.Where(p => p.Type == "Windows").OrderBy(p => p.Name), "ActiveDirectoryPermissions", "Name");
            ViewData["Fonix3"] = new MultiSelectList(_context.Permissions.Where(p => p.Type == "Főnix 3").OrderBy(p => p.Name), "Name", "Name");
            return View();
        }

        // POST: Groups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string[] windowsPermissions, string[] fonix3Permissions, [Bind("GroupId,Name,ClassId,WindowsPermissions,FonixPermissions")] Group group)
        {
            if (ModelState.IsValid)
            {
                // Windows jogosultságok Active-Directory értékeinek sorrendbe helyezése
                group.WindowsPermissions = PermissionList(windowsPermissions);

                // Főnix 3 jogosultságok neveinek sorrendbe helyezése
                group.FonixPermissions = PermissionList(fonix3Permissions);

                _context.Add(group);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClassId"] = new SelectList(_context.Classes, "ClassId", "Name", group.ClassId);
            ViewData["Windows"] = new MultiSelectList(_context.Permissions.Where(p => p.Type == "Windows").OrderBy(p => p.Name), "ActiveDirectoryPermissions", "Name");
            ViewData["Fonix3"] = new MultiSelectList(_context.Permissions.Where(p => p.Type == "Főnix 3").OrderBy(p => p.Name), "Name", "Name");
            return View(group);
        }

        // GET: Groups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Groups == null)
            {
                return NotFound();
            }

            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            ViewData["ClassId"] = new SelectList(_context.Classes, "ClassId", "Name", group.ClassId);
            return View(group);
        }

        // POST: Groups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GroupId,Name,ClassId,WindowsPermissions,FonixPermissions")] Group group)
        {
            if (id != group.GroupId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(group.GroupId))
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
            ViewData["ClassId"] = new SelectList(_context.Classes, "ClassId", "Name", group.ClassId);
            return View(group);
        }

        private string PermissionList(string[] permissions)
        {
            StringBuilder permissionList = new StringBuilder();
            foreach (string permission in permissions) permissionList.Append(permission[permission.Length - 1] == ';' ? $"{permission} " : $"{permission}; ");
            return permissionList.ToString();
        }

        private bool GroupExists(int id)
        {
            return _context.Groups.Any(e => e.GroupId == id);
        }
    }
}