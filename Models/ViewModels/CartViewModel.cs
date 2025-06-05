namespace GCTWeb.Models.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;

public class CartViewModel
{
    public Guid CartId { get; set; } // Có thể là CartId từ DB hoặc GuestSessionCartId
    public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
    public decimal TotalPrice { get; set; }
    public int TotalItems { get; set; }
    public bool IsEmpty => !Items.Any();
}