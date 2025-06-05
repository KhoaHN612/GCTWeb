namespace GCTWeb.Models.ViewModels;

using System;

public class CartItemViewModel
{
    public Guid CartItemId { get; set; } // ID của CartItem trong DB (nếu là user cart)
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public uint Quantity { get; set; }
    public decimal Price { get; set; } // Giá tại thời điểm lấy (hoặc thêm vào giỏ)
    public decimal ItemTotalPrice { get; set; }
    public string? ImageUrl { get; set; }
}