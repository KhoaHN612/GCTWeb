// File: Controllers/Api/CartApiController.cs
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using GCTWeb.Services; 
using GCTWeb.Models.ViewModels; 

namespace GCTWeb.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    public class CartApiController : ControllerBase
    {
        private readonly CartService _cartService;
        private readonly ILogger<CartApiController> _logger; 

        public CartApiController(CartService cartService, ILogger<CartApiController> logger)
        {
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/cartapi
        // Lấy thông tin chi tiết giỏ hàng để hiển thị trong panel cart
        [HttpGet]
        public async Task<ActionResult<CartViewModel>> GetCart()
        {
            try
            {
                var cartViewModel = await _cartService.GetCartAsync();
                return Ok(cartViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving the cart.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the cart.");
            }
        }

        // GET: api/cartapi/summary
        // Lấy thông tin tóm tắt (số lượng, tổng tiền) để cập nhật icon giỏ hàng
        [HttpGet("summary")]
        public async Task<ActionResult<object>> GetCartSummary()
        {
            try
            {
                var cartViewModel = await _cartService.GetCartAsync();
                return Ok(new { 
                    totalItems = cartViewModel.TotalItems, 
                    totalPriceString = cartViewModel.TotalPrice.ToString("N0") + " VND" // Định dạng tiền tệ
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving cart summary.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving cart summary.");
            }
        }

        // DTO (Data Transfer Object) cho request thêm vào giỏ hàng
        public class AddToCartRequestDto
        {
            [Required(ErrorMessage = "Product ID is required.")]
            public Guid ProductId { get; set; }
            [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100.")]
            public uint Quantity { get; set; } = 1; // Mặc định số lượng là 1
        }

        // POST: api/cartapi/add
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _cartService.AddToCartAsync(request.ProductId, request.Quantity);
                var updatedCart = await _cartService.GetCartAsync(); // Lấy lại cart sau khi thêm
                return Ok(new { 
                    message = "Product added to cart successfully.", 
                    totalItems = updatedCart.TotalItems, 
                    totalPriceString = updatedCart.TotalPrice.ToString("N0") + " VND",
                    cart = updatedCart // Trả về cả cart đã cập nhật
                });
            }
            catch (InvalidOperationException ex) // Lỗi nghiệp vụ (ví dụ: hết hàng, sản phẩm không tồn tại)
            {
                _logger.LogWarning(ex, "Business logic error while adding to cart.");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) // Lỗi không mong muốn khác
            {
                _logger.LogError(ex, "Unexpected error occurred while adding to cart.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred. Please try again." });
            }
        }

        public class UpdateQuantityRequestDto
        {
            [Required(ErrorMessage = "Product ID is required.")]
            public Guid ProductId { get; set; }
            [Range(0, 100, ErrorMessage = "Quantity must be between 0 and 100.")] // Cho phép quantity = 0 để xóa
            public uint Quantity { get; set; }
        }

        // POST: api/cartapi/updatequantity
        [HttpPost("updatequantity")]
        public async Task<ActionResult<CartViewModel>> UpdateQuantity([FromBody] UpdateQuantityRequestDto request)
        {
             if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                // Nếu Quantity = 0, RemoveFromCartAsync sẽ được gọi bên trong UpdateQuantityAsync
                var updatedCart = await _cartService.UpdateQuantityAsync(request.ProductId, request.Quantity);
                return Ok(updatedCart);
            }
            catch (InvalidOperationException ex) 
            { 
                _logger.LogWarning(ex, "Business logic error while updating cart quantity.");
                return BadRequest(new { message = ex.Message }); 
            }
            catch (Exception ex) { 
                _logger.LogError(ex, "Error updating cart quantity.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating cart quantity."});
            }
        }

        // POST: api/cartapi/remove/{productId}
        // Hoặc bạn có thể dùng [HttpDelete("{productId:guid}")] nếu client hỗ trợ DELETE verb dễ dàng
        [HttpPost("remove/{productId:guid}")]
        public async Task<ActionResult<CartViewModel>> RemoveItem(Guid productId)
        {
            if (productId == Guid.Empty) 
            {
                return BadRequest(new { message = "Invalid Product ID." });
            }
            try
            {
                var updatedCart = await _cartService.RemoveFromCartAsync(productId);
                return Ok(updatedCart);
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"Error removing item {productId} from cart.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while removing the item from cart."});
            }
        }

        // POST: api/cartapi/clear
        [HttpPost("clear")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                await _cartService.ClearCartAsync();
                var emptyCart = await _cartService.GetCartAsync(); // Lấy cart rỗng để trả về
                return Ok(new { message = "Cart cleared successfully.", cart = emptyCart });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while clearing the cart." });
            }
        }
    }
}