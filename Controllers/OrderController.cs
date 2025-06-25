using Microsoft.AspNetCore.Mvc;

namespace GCTWeb.Controllers;

public class OrderController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}