using GCTWeb.Models.ViewModels;
using GCTWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace GCTWeb.Controllers;

public class CartController : Controller
{
    private readonly CartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(CartService cartService, ILogger<CartController> logger)
    {
        _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // GET: /Cart hoặc /Cart/Index
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var cartViewModel = await _cartService.GetCartAsync();
            return View(cartViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving cart for Cart page.");
            ViewData["ErrorMessage"] = "Could not load your cart at this time. Please try again later.";
            return View(new CartViewModel());
        }
    }

    // GET: Cart/Checkout'
    public async Task<IActionResult> Checkout()
    {
        var cart = await _cartService.GetCartAsync();
        if (cart.IsEmpty)
        {
            TempData["CartErrorMessage"] = "Your cart is empty. Please add items before proceeding to checkout.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index), "Checkout");
    }
}