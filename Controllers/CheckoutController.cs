using GCTWeb.Models.Enums;
using GCTWeb.Models.ViewModels;
using GCTWeb.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GCTWeb.Controllers;

public class CheckoutController : Controller
{
    private readonly CartService _cartService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(
        CartService cartService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<CheckoutController> logger)
    {
        _cartService = cartService;
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    // GET: /Checkout
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cart = await _cartService.GetCartAsync();
        if (cart.IsEmpty)
        {
            TempData["CartErrorMessage"] = "Your cart is empty. Please add items before checkout.";
            return RedirectToAction("Index", "Cart");
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var viewModel = new CheckoutViewModel
        {
            Cart = cart,
            BillingFullName = currentUser.Name,
            BillingEmail = currentUser.Email,
            BillingPhoneNumber = currentUser.PhoneNumber
        };

        return View(viewModel);
    }

    // POST: /Checkout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        var cart = await _cartService.GetCartAsync();
        model.Cart = cart;

        if (cart.IsEmpty) ModelState.AddModelError("", "Your cart is empty and cannot be processed.");

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Checkout ModelState is invalid for user.");
            return View(model);
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Tổ hợp địa chỉ thành một chuỗi duy nhất
            var fullShippingAddress =
                $"{model.BillingAddress1}, {model.BillingWard}, {model.BillingCity}, {model.BillingState}";

            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                UserId = currentUser.Id,
                OrderNumber = GenerateOrderNumber(),

                ShippingRecipientName = model.BillingFullName,
                ShippingPhone = model.BillingPhoneNumber,
                ShippingAddress = fullShippingAddress,

                Subtotal = cart.TotalPrice,
                ShippingFee = ParseShippingFee(model.ShippingMethod),
                DiscountAmount = 0, // Logic discount sẽ thêm sau
                TotalPrice = cart.TotalPrice + ParseShippingFee(model.ShippingMethod),
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = PaymentStatus.Pending,
                Status = OrderStatus.Pending,
                Notes = model.OrderNotes
            };

            foreach (var item in cart.Items)
            {
                var productInDb = await _context.Products.FindAsync(item.ProductId);
                if (productInDb == null || productInDb.Stock < item.Quantity)
                    throw new InvalidOperationException(
                        $"Product '{item.ProductName}' is no longer available or out of stock.");
                productInDb.Stock -= item.Quantity; // Giảm tồn kho

                var orderDetail = new OrderDetail
                {
                    OrderDetailId = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    ProductName = item.ProductName,
                    ProductSku = productInDb.Sku
                };
                order.OrderDetails.Add(orderDetail);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _cartService.ClearCartAsync();

            return RedirectToAction("OrderConfirmation", new { id = order.OrderId });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during checkout process for user {UserId}", currentUser.Id);
            ModelState.AddModelError("", "An error occurred while placing your order. Please try again.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> OrderConfirmation(Guid id)
    {
        if (id == Guid.Empty) return BadRequest();
        // Tải đơn hàng để hiển thị xác nhận
        var order = await _context.Orders.Include(o => o.OrderDetails).ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id);
        if (order == null) return NotFound();

        return View("OrderConfirmation", order);
    }

    private string GenerateOrderNumber()
    {
        return $"GCT-{DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    private decimal ParseShippingFee(string shippingMethodValue)
    {
        if (string.IsNullOrEmpty(shippingMethodValue) || !shippingMethodValue.StartsWith("standard_")) return 0;
        var feePart = shippingMethodValue.Split('_').Last();
        decimal.TryParse(feePart, out var fee);
        return fee;
    }
}