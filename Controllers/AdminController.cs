using Microsoft.AspNetCore.Mvc;

namespace GCTWeb.Controllers.Admin;

public class AdminController : Controller {
    
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index() {
        return View();
    }
    
    public IActionResult Users() {
        return View();
    }
    
    public IActionResult Orders() {
        return View();
    }
    
    [Route("Admin/Products")]
    public async Task<IActionResult> ProductIndex()
    {
        var applicationDbContext = _context.Products.Include(p => p.Brand).Include(p => p.Category).Include(p => p.Grade).Include(p => p.PrimaryImage);
        return View("../Admin/Product/Index",await applicationDbContext.ToListAsync());
    }
    
    public IActionResult Brands() {
        return View();
    }
    
    public IActionResult Grades() {
        return View();
    }
    
    public IActionResult Categories() {
        return View();
    }
    
    public IActionResult Vouchers() {
        return View();
    }
}