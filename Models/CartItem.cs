namespace GCTWeb.Models;

[Table("cart_items")]
[Index(nameof(CartId), nameof(ProductId), IsUnique = true, Name = "IX_cart_items_cart_product_unique")]
[Index(nameof(ProductId))]
public class CartItem
{
    public CartItem()
    {
        AddedAt = DateTime.UtcNow;
    }

    [Key]
    [Column("cart_item_id")]
    public Guid CartItemId { get; set; }

    [Required]
    [Column("cart_id")]
    public Guid CartId { get; set; }

    [Required]
    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Required]
    [Column("quantity")]
    [Range(1, int.MaxValue)]
    public uint Quantity { get; set; }

    [Required]
    [Column("added_at")]
    public DateTime AddedAt { get; set; }

    [ForeignKey(nameof(CartId))]
    public virtual Cart Cart { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;
}