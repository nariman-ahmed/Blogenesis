using System.Diagnostics;
using Blogenesis.Data;
using Blogenesis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Index()
        {
            return View();
        }

        
    }
}
