using System.Security.Claims;
using System.Threading.Tasks;
using Blogenesis.Data;
using Blogenesis.DTO;
using Blogenesis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blogenesis.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateBlogDto createDto)
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(loggedInUserId))
            {
                // If the user is not logged in, redirect to the login page or handle accordingly
                return RedirectToAction("Login", "Account");
            }

            var newBlog = new BlogModel()
            {
                Title = createDto.Title,
                Content = createDto.Content,
                ReadTimeMinutes = createDto.ReadTimeMinutes,
                Subject = createDto.Subject,
                DateCreated = DateTime.UtcNow,
                IsPublished = createDto.IsPublished,
                UserId = loggedInUserId // Link the blog to the logged-in user
            };

            await _context.Blogs.AddAsync(newBlog);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyBlogs");
        }

        [Authorize]
        public async Task<IActionResult> MyBlogs()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(loggedInUserId))
            {
                // If the user is not logged in, redirect to the login page or handle accordingly
                return RedirectToAction("Login", "Account");
            }

            //return all said user's blogs in his personal page
            var allBlogs = await _context.Blogs
            .Where(b => b.UserId == loggedInUserId)
            .OrderByDescending(b => b.DateCreated)
            .ToListAsync();

            return View(allBlogs);
        }

        public async Task<IActionResult> DeleteBlog(DeleteBlogDto deleteDto)
        {
            //ana ayza el blog men el passed blog id
            var toBeDeleted = await _context.Blogs.FindAsync(deleteDto.BlogId);

            if (toBeDeleted == null)
            {
                // Handle the case where the blog post doesn't exist
                return NotFound();
            }

            _context.Blogs.Remove(toBeDeleted);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyBlogs");
        }
        
    }
}
