using Microsoft.AspNetCore.Mvc;

namespace ISZR.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DataContext _context;

        public DashboardController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // get username
            string? activeUsername = User.Identity?.Name;

            // looking for user data
            var user = await _context.Users
                .Include(u => u.Class)
                .Include(u => u.Position)
                .FirstOrDefaultAsync(m => m.Username == activeUsername);

            // user not exist
            if (user == null) return RedirectToAction("Index", "Welcome");

            // save user data
            ViewBag.CurrentUser = user;

            // return page
            return View();
        }
    }
}