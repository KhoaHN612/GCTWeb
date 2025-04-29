namespace GCTWeb.Models;

[Table("order_details")]
[Index(nameof(ProductId))]
public class OrderDetail // Or OrderItem
{
    [Key]
    [Column("order_detail_id")]
    public Guid OrderDetailId { get; set; }

    [Required]
    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Required]
    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Required]
    [Column("quantity")]
    [Range(1, int.MaxValue)]
    public uint Quantity { get; set; }

    [Required]
    [Column("unit_price", TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; }

    [Required]
    [StringLength(255)]
    [Column("product_name")]
    public string ProductName { get; set; } = null!;

    [StringLength(100)]
    [Column("product_sku")]
    public string? ProductSku { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!; // ON DELETE RESTRICT -> Cấu hình Fluent API
}