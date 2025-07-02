using System.Security.Claims;
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
    private readonly SignInManager<ApplicationUser> _signInManager;

    public CheckoutController(
        CartService cartService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<CheckoutController> logger,
        SignInManager<ApplicationUser> signInManager)
    {
        _cartService = cartService;
        _userManager = userManager;
        _context = context;
        _logger = logger;
        _signInManager = signInManager;
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

        var viewModel = new CheckoutViewModel
        {
            Cart = cart
        };
        
        if (_signInManager.IsSignedIn(User))
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                viewModel.BillingFullName = currentUser.Name;
                viewModel.BillingEmail = currentUser.Email;
                viewModel.BillingPhoneNumber = currentUser.PhoneNumber;
                // TODO: Tải địa chỉ mặc định của user từ DB (nếu có) và điền vào các trường địa chỉ
            }
        }

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

        if (!_signInManager.IsSignedIn(User) && string.IsNullOrWhiteSpace(model.BillingEmail))
        {
            ModelState.AddModelError("BillingEmail", "Email address is required to receive order confirmation.");
        }
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Checkout ModelState is invalid.");
            return View(model);
        }

        string? currentUserId = null; 
        if (_signInManager.IsSignedIn(User))
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                currentUserId = currentUser.Id;
            }
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Tổ hợp địa chỉ thành một chuỗi duy nhất
            var fullShippingAddress =
                $"{model.BillingAddress1}, {model.BillingWard}, {model.BillingCity}, {model.BillingState}";
            
            var newOrderNumber = await GenerateOrderNumberAsync();

            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                UserId = currentUserId,
                OrderNumber = newOrderNumber,

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
                productInDb.Stock -= item.Quantity; 

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

            return RedirectToAction("OrderConfirmation", new { orderNumber = order.OrderNumber });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during checkout process for user/guest. UserId: {UserId}", currentUserId ?? "Guest");
            ModelState.AddModelError("", "An error occurred while placing your order. Please try again.");
            return View(model);
        }
    }
    
    private async Task<string> GenerateOrderNumberAsync()
    {
        string orderNumber;
        var random = new Random();
        do
        {
            // Lấy ngày hiện tại theo giờ Việt Nam
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);

            // Phần ngày tháng: GCT-YYMMDD-
            var datePart = $"GCT{today:yyMMdd}";

            // Phần ngẫu nhiên: 8 ký tự đầu từ một GUID, chuyển thành chữ hoa
            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            orderNumber = datePart + randomPart;

        } while (await _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber)); 

        return orderNumber;
    }

    private decimal ParseShippingFee(string shippingMethodValue)
    {
        if (string.IsNullOrEmpty(shippingMethodValue) || !shippingMethodValue.StartsWith("standard_")) return 0;
        var feePart = shippingMethodValue.Split('_').Last();
        decimal.TryParse(feePart, out var fee);
        return fee;
    }
}