using System.Diagnostics;
using Blogenesis.Data;
using Blogenesis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blogenesis.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            //show all blogs

            var allBlogs = await _context.Blogs
                .Include(b => b.User)
                .Where(b => b.IsPublished == true)
                .OrderByDescending(b => b.DateCreated)
                .ToListAsync(); 

            return View(allBlogs);
        }

        
    }
}
