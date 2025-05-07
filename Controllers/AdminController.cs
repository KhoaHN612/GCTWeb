using Microsoft.AspNetCore.Mvc;

namespace GCTWeb.Controllers;

public class AdminController : Controller {
    // GET
    public IActionResult Index() {
        return View();
    }
    
    public IActionResult Users() {
        return View();
    }
    
    public IActionResult Orders() {
        return View();
    }
    
    public IActionResult Products() {
        return View();
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