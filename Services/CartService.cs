namespace GCTWeb.Services;

// File: Services/CartService.cs
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json; // Cần cài đặt gói NuGet Newtonsoft.Json
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GCTWeb.Data; 
using GCTWeb.Models;
using GCTWeb.Models.ViewModels; 

public class CartService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CartItemsSessionKey = "App_SessionCartItems_v2"; // Đổi key nếu cần reset session cũ
    private const string GuestCartIdSessionKey = "App_GuestCartId_v2";

    public CartService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    private ISession Session => _httpContextAccessor.HttpContext?.Session
                                ?? throw new InvalidOperationException(
                                    "Session is not available in the current context. Ensure IHttpContextAccessor is registered and HttpContext is available.");

    private ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User
                                    ?? new ClaimsPrincipal(
                                        new ClaimsIdentity()); // Trả về một principal rỗng nếu user null

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private List<CartItemViewModel> GetGuestCartItemsFromSession()
    {
        var sessionCartJson = Session.GetString(CartItemsSessionKey);
        if (string.IsNullOrEmpty(sessionCartJson)) return new List<CartItemViewModel>();
        try
        {
            return JsonConvert.DeserializeObject<List<CartItemViewModel>>(sessionCartJson) ??
                   new List<CartItemViewModel>();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing guest cart from session: {ex.Message}"); // Ghi log lỗi
            Session.Remove(CartItemsSessionKey); // Xóa dữ liệu session hỏng
            return new List<CartItemViewModel>();
        }
    }

    private void SaveGuestCartItemsToSession(List<CartItemViewModel> items)
    {
        if (items == null || !items.Any())
        {
            Session.Remove(CartItemsSessionKey);
            Session.Remove(GuestCartIdSessionKey); // Cũng xóa ID nếu giỏ hàng rỗng
        }
        else
        {
            Session.SetString(CartItemsSessionKey, JsonConvert.SerializeObject(items));
            // GuestCartId được tạo và lưu khi GetOrCreateGuestCartIdFromSession được gọi
        }
    }

    private Guid GetOrCreateGuestCartIdFromSession()
    {
        var cartIdStr = Session.GetString(GuestCartIdSessionKey);
        if (Guid.TryParse(cartIdStr, out var cartId)) return cartId;
        cartId = Guid.NewGuid();
        Session.SetString(GuestCartIdSessionKey, cartId.ToString());
        return cartId;
    }

    public async Task<CartViewModel> GetCartAsync()
    {
        var userId = GetCurrentUserId();
        var cartViewModel = new CartViewModel();

        if (!string.IsNullOrEmpty(userId)) // Người dùng đã đăng nhập
        {
            var userCart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p!.PrimaryImage) // p! để báo EF Core rằng Product không null sau ThenInclude
                .AsNoTracking() // Dùng AsNoTracking cho truy vấn chỉ đọc
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null)
            {
                // Không tự động tạo cart ở đây nữa, để AddToCart hoặc Merge xử lý
                // Hoặc nếu muốn, bạn có thể tạo cart rỗng cho user mới ở đây
                cartViewModel.CartId = Guid.NewGuid(); // ID tạm thời nếu cart rỗng
            }
            else
            {
                cartViewModel.CartId = userCart.CartId;
                cartViewModel.Items = userCart.CartItems.Select(ci => new CartItemViewModel
                {
                    CartItemId = ci.CartItemId,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product?.Name ?? "N/A",
                    Quantity = ci.Quantity,
                    Price = ci.Product?.Price ?? 0,
                    ItemTotalPrice = ci.Quantity * (ci.Product?.Price ?? 0),
                    ImageUrl = ci.Product?.PrimaryImage?.ImageUrl
                }).ToList();
            }
        }
        else // Khách (Guest)
        {
            var sessionItems = GetGuestCartItemsFromSession();
            cartViewModel.CartId = GetOrCreateGuestCartIdFromSession(); // ID cho giỏ hàng session

            if (sessionItems.Any())
            {
                var productIds = sessionItems.Select(s => s.ProductId).Distinct().ToList();
                // Tải thông tin sản phẩm một lần để tối ưu
                var productsFromDb = await _context.Products
                    .Where(p => productIds.Contains(p.ProductId) && p.IsActive)
                    .Include(p => p.PrimaryImage)
                    .AsNoTracking()
                    .ToDictionaryAsync(p => p.ProductId, p => p);

                foreach (var sessionItem in sessionItems)
                    if (productsFromDb.TryGetValue(sessionItem.ProductId, out var product))
                        cartViewModel.Items.Add(new CartItemViewModel
                        {
                            ProductId = product.ProductId, // Dùng ProductId từ DB object
                            ProductName = product.Name,
                            Quantity = sessionItem.Quantity,
                            Price = product.Price, // Luôn lấy giá mới nhất từ DB
                            ItemTotalPrice = sessionItem.Quantity * product.Price,
                            ImageUrl = product.PrimaryImage?.ImageUrl
                        });
                // Nếu sản phẩm không còn (bị xóa, không active), nó sẽ không được thêm vào cartViewModel
            }
        }

        cartViewModel.TotalItems = (int)cartViewModel.Items.Sum(i => i.Quantity);
        cartViewModel.TotalPrice = cartViewModel.Items.Sum(i => i.ItemTotalPrice);
        return cartViewModel;
    }

    public async Task AddToCartAsync(Guid productId, uint quantity = 1)
    {
        if (quantity == 0) quantity = 1; // Số lượng tối thiểu là 1

        var product = await _context.Products
            .Include(p => p.PrimaryImage) // Để lấy ImageUrl cho session cart
            .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsActive);

        if (product == null) throw new InvalidOperationException("Product not found or is not available.");

        var userId = GetCurrentUserId();

        if (!string.IsNullOrEmpty(userId)) // Người dùng đã đăng nhập
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { CartId = Guid.NewGuid(), UserId = userId };
                _context.Carts.Add(cart);
                // Sẽ SaveChanges sau khi thêm item
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                if (product.Stock < cartItem.Quantity + quantity)
                    throw new InvalidOperationException(
                        $"Not enough stock for {product.Name}. Available: {product.Stock}.");
                cartItem.Quantity += quantity;
            }
            else
            {
                if (product.Stock < quantity)
                    throw new InvalidOperationException(
                        $"Not enough stock for {product.Name}. Available: {product.Stock}.");
                cart.CartItems.Add(new CartItem
                    { CartItemId = Guid.NewGuid(), ProductId = productId, Quantity = quantity, CartId = cart.CartId });
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        else // Khách
        {
            var sessionItems = GetGuestCartItemsFromSession();
            var existingItem = sessionItems.FirstOrDefault(item => item.ProductId == productId);
            if (existingItem != null)
            {
                if (product.Stock < existingItem.Quantity + quantity)
                    throw new InvalidOperationException(
                        $"Not enough stock for {product.Name}. Available: {product.Stock}.");
                existingItem.Quantity += quantity;
            }
            else
            {
                if (product.Stock < quantity)
                    throw new InvalidOperationException(
                        $"Not enough stock for {product.Name}. Available: {product.Stock}.");
                sessionItems.Add(new CartItemViewModel // Lưu ViewModel vào session cho đơn giản
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    Quantity = quantity,
                    Price = product.Price, // Lưu giá tại thời điểm thêm (có thể khác khi checkout)
                    ImageUrl = product.PrimaryImage?.ImageUrl
                    // ItemTotalPrice sẽ được tính lại khi GetCartAsync
                });
            }

            SaveGuestCartItemsToSession(sessionItems);
        }
    }

    public async Task<CartViewModel> UpdateQuantityAsync(Guid productId, uint newQuantity)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null) throw new InvalidOperationException("Product not found.");

        if (newQuantity == 0) return await RemoveFromCartAsync(productId); // Nếu số lượng mới là 0 thì xóa item
        if (product.Stock < newQuantity)
            throw new InvalidOperationException(
                $"Not enough stock for {product.Name}. Available: {product.Stock}, requested: {newQuantity}.");

        var userId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart?.CartItems != null)
            {
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                if (cartItem != null)
                {
                    cartItem.Quantity = newQuantity;
                    cart.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Nếu item không có trong cart DB, có thể người dùng đang cố update session cart cũ
                    // hoặc có lỗi logic. Cân nhắc throw exception hoặc bỏ qua.
                    throw new InvalidOperationException("Item not found in user's cart.");
                }
            }
        }
        else
        {
            var sessionItems = GetGuestCartItemsFromSession();
            var itemToUpdate = sessionItems.FirstOrDefault(item => item.ProductId == productId);
            if (itemToUpdate != null)
            {
                itemToUpdate.Quantity = newQuantity;
                SaveGuestCartItemsToSession(sessionItems);
            }
            else
            {
                throw new InvalidOperationException("Item not found in guest's cart.");
            }
        }

        return await GetCartAsync(); // Trả về cart đã cập nhật
    }

    public async Task<CartViewModel> RemoveFromCartAsync(Guid productId)
    {
        var userId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart?.CartItems != null)
            {
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                if (cartItem != null)
                {
                    _context.CartItems.Remove(cartItem);
                    cart.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
        }
        else
        {
            var sessionItems = GetGuestCartItemsFromSession();
            var itemToRemove = sessionItems.FirstOrDefault(item => item.ProductId == productId);
            if (itemToRemove != null)
            {
                sessionItems.Remove(itemToRemove);
                SaveGuestCartItemsToSession(sessionItems);
            }
        }

        return await GetCartAsync(); // Trả về cart đã cập nhật
    }

    public async Task ClearCartAsync()
    {
        var userId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart != null)
            {
                if (cart.CartItems.Any()) _context.CartItems.RemoveRange(cart.CartItems); // Xóa tất cả items
                // Không xóa cart object, chỉ làm rỗng nó
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        else
        {
            SaveGuestCartItemsToSession(new List<CartItemViewModel>()); // Lưu danh sách rỗng để xóa session
            Session.Remove(GuestCartIdSessionKey); // Xóa luôn ID của giỏ hàng khách
        }
    }

    public async Task MergeSessionCartToUserCartAsync(string userId)
    {
        var sessionItems = GetGuestCartItemsFromSession();
        if (!sessionItems.Any())
        {
            Session.Remove(CartItemsSessionKey); // Dọn dẹp session
            Session.Remove(GuestCartIdSessionKey);
            return;
        }

        var userCart = await _context.Carts
            .Include(c => c.CartItems) // Cần để kiểm tra existing items
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (userCart == null)
        {
            userCart = new Cart { CartId = Guid.NewGuid(), UserId = userId };
            _context.Carts.Add(userCart);
            // Không cần SaveChanges ngay nếu CartId là client-generated GUID
        }

        var cartWasModified = false;
        foreach (var sessionItem in sessionItems)
        {
            // Quan trọng: Luôn kiểm tra lại sản phẩm và tồn kho từ DB khi merge
            var product = await _context.Products.FindAsync(sessionItem.ProductId);
            if (product == null || !product.IsActive || product.Stock == 0) continue; // Bỏ qua sản phẩm không hợp lệ

            var existingDbItem = userCart.CartItems.FirstOrDefault(ci => ci.ProductId == sessionItem.ProductId);
            uint quantityToProcess = sessionItem.Quantity;

            if (existingDbItem != null)
            {
                var newTotalQuantity = existingDbItem.Quantity + quantityToProcess;
                existingDbItem.Quantity = product.Stock < newTotalQuantity ? product.Stock : newTotalQuantity;
            }
            else
            {
                userCart.CartItems.Add(new CartItem
                {
                    CartItemId = Guid.NewGuid(),
                    CartId = userCart.CartId, // Gán CartId của userCart
                    ProductId = product.ProductId,
                    Quantity = product.Stock < quantityToProcess ? product.Stock : quantityToProcess
                });
            }

            cartWasModified = true;
        }

        if (cartWasModified)
        {
            userCart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        SaveGuestCartItemsToSession(new List<CartItemViewModel>()); // Xóa session cart của khách
        Session.Remove(GuestCartIdSessionKey);
    }
}