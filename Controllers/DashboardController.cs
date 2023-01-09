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
            var user = await _context.User
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