using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GCTWeb.Models;

namespace GCTWeb.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger,ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Grade)
            .Include(p => p.PrimaryImage);
        return View(await applicationDbContext.ToListAsync());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}