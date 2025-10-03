using GCTWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GCTWeb.ViewComponents
{
    public class OrderNotificationsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public OrderNotificationsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var pendingOrders = await _context.Orders
                .Where(o => o.Status == Models.Enums.OrderStatus.Pending)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var totalPending = pendingOrders.Count;

            ViewBag.TotalPending = totalPending;

            return View(pendingOrders);
        }
    }
}