using System.Diagnostics;
using Blogenesis.Data;
using Blogenesis.DTO;
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
        
        [HttpGet]
        public async Task<IActionResult> FilterBlogs(string? subject)
        {
            IQueryable<BlogModel> blogsQuery = _context.Blogs
                .Include(b => b.User)
                .Where(b => b.IsPublished == true);

            // Apply filter if subject is provided
            if (!string.IsNullOrEmpty(subject))
            {
                blogsQuery = blogsQuery.Where(b => b.Subject == subject);
            }

            var filteredBlogs = await blogsQuery
                .OrderByDescending(b => b.DateCreated)
                .ToListAsync();

            return View("Index", filteredBlogs);
        }

        
    }
}
