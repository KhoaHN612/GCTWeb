using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace GCTWeb.Controllers;

public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrderController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
    // GET
    [HttpGet("order/{orderNumber}")]
    public async Task<IActionResult> OrderConfirmation(string orderNumber)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .ThenInclude(p => p!.PrimaryImage)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.UserId == GetCurrentUserId()); 

        if (order == null) return NotFound();
    
        return View("Details", order);
    }
}