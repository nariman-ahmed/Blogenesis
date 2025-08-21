using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
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
        private readonly IConfiguration _configuration;

        public BlogController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyBlogs()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(loggedInUserId))
            {
                // If the user is not logged in, redirect to the login page or handle accordingly
                return RedirectToAction("Login", "Account");
            }

            // Get current user information for profile picture
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == loggedInUserId);
            ViewBag.CurrentUserProfilePic = currentUser?.ProfilePicUrl;
            ViewBag.CurrentUserName = currentUser?.FullName ?? currentUser?.UserName;

            //return all said user's blogs in his personal page
            var allBlogs = await _context.Blogs
            .Where(b => b.UserId == loggedInUserId)
            .OrderByDescending(b => b.DateCreated)
            .ToListAsync();

            return View(allBlogs);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create(string generatedTitle, string generatedContent, string generatedSubject)
        {
            // If we have generated content, pass it to the view
            if (!string.IsNullOrEmpty(generatedContent))
            {
                ViewBag.GeneratedTitle = generatedTitle;
                ViewBag.GeneratedContent = generatedContent;
                ViewBag.GeneratedSubject = generatedSubject;
                ViewBag.SuccessMessage = "AI content generated successfully! You can now edit it below.";
            }
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


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var blog = await _context.Blogs
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);
            
            if (blog == null)
            {
                return NotFound();
            }

            // Check if the current user owns this blog or if it's published
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (blog.UserId != loggedInUserId && !blog.IsPublished)
            {
                return Forbid(); // User can't view unpublished blogs of others
            }

            return View(blog);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            
            if (blog == null)
            {
                return NotFound();
            }

            // Check if the current user owns this blog
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (blog.UserId != loggedInUserId)
            {
                return Forbid(); // User can't edit blogs they don't own
            }

            // Pass the blog data to the Create view for editing
            ViewBag.EditMode = true;
            ViewBag.BlogId = blog.Id;
            ViewBag.GeneratedTitle = blog.Title;
            ViewBag.GeneratedContent = blog.Content;
            ViewBag.GeneratedSubject = blog.Subject;
            ViewBag.ReadTimeMinutes = blog.ReadTimeMinutes;
            ViewBag.IsPublished = blog.IsPublished;

            return View("Create");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Update(int id, CreateBlogDto updateDto)
        {
            var blog = await _context.Blogs.FindAsync(id);
            
            if (blog == null)
            {
                return NotFound();
            }

            // Check if the current user owns this blog
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (blog.UserId != loggedInUserId)
            {
                return Forbid(); // User can't edit blogs they don't own
            }

            // Update the blog properties
            blog.Title = updateDto.Title;
            blog.Content = updateDto.Content;
            blog.ReadTimeMinutes = updateDto.ReadTimeMinutes;
            blog.Subject = updateDto.Subject;
            blog.IsPublished = updateDto.IsPublished;
            blog.DateCreated = DateTime.UtcNow; // Update the modified date

            _context.Blogs.Update(blog);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = blog.Id });
        }


        [Authorize]
        [HttpPost]
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateBlogWithAi(CreateWithAiDto createDto)
        {
            try
            {
                // Get API key from appsettings.json
                var apiKey = _configuration["OpenRouter:ApiKey"];
                var endpoint = "https://openrouter.ai/api/v1/chat/completions";

                var ai_prompt = $"Generate a blog post about {createDto.Subject} with the tone: {createDto.Tone}. I want it to be {createDto.Length} length. Return ONLY the HTML body content (no <html>, <head>, or <body> tags) with headings, subheadings, bullet points, and bold where appropriate. Start directly with the content like <h1>Title</h1><p>Content...</p>";

                var requestBody = new
                {
                    model = "meta-llama/llama-3.1-8b-instruct",
                    messages = new[]
                    {
                        new { role = "user", content = ai_prompt }
                    },
                    max_tokens = 4000,
                    temperature = 0.7
                };

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:5161");
                client.DefaultRequestHeaders.Add("X-Title", "Blogenesis");

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Parse the AI response
                    var jsonResponse = JsonDocument.Parse(responseString);
                    var generatedContent = jsonResponse.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    // Check if content was generated
                    if (string.IsNullOrEmpty(generatedContent))
                    {
                        TempData["ErrorMessage"] = "AI service returned empty content.";
                        return RedirectToAction("Create");
                    }

                    // Debug: Log the full generated content
                    Console.WriteLine($"Full AI Generated Content: {generatedContent}");

                    // Extract title from the generated content (assuming it starts with <h1>)
                    string generatedTitle = "AI Generated Blog Post";
                    string generatedBody = generatedContent;

                    if (generatedContent.StartsWith("<h1>"))
                    {
                        var titleEnd = generatedContent.IndexOf("</h1>");
                        if (titleEnd > 0)
                        {
                            generatedTitle = generatedContent.Substring(4, titleEnd - 4); // Remove <h1> tags
                            generatedBody = generatedContent.Substring(titleEnd + 5); // Content after </h1>
                        }
                    }

                    // Debug: Log the parsed content
                    Console.WriteLine($"Parsed Title: {generatedTitle}");
                    Console.WriteLine($"Parsed Body: {generatedBody}");

                    // Store the generated content in TempData to pass to the Create view
                    TempData["GeneratedTitle"] = generatedTitle;
                    TempData["GeneratedContent"] = generatedBody;
                    TempData["GeneratedSubject"] = createDto.Subject;
                    TempData["SuccessMessage"] = "AI content generated successfully! You can now edit it below.";

                    var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(loggedInUserId))
                    {
                        // If the user is not logged in, redirect to the login page or handle accordingly
                        return RedirectToAction("Login", "Account");
                    }

                    //save to database
                    var newBlog = new BlogModel
                    {
                        Title = generatedTitle,
                        Content = generatedBody,
                        Subject = createDto.Subject,
                        DateCreated = DateTime.UtcNow,
                        UserId = loggedInUserId
                    };

                    _context.Blogs.Add(newBlog);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Create", new
                    {
                        generatedTitle = generatedTitle,
                        generatedContent = generatedBody,
                        generatedSubject = createDto.Subject
                    });
                }
                else
                {
                    TempData["ErrorMessage"] = $"AI service error: {response.StatusCode} - {responseString}";
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating AI content: {ex.Message}";
                return RedirectToAction("Create");
            }
        }
        
    }
}
