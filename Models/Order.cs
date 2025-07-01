using GCTWeb.Models.Enums;

namespace GCTWeb.Models;

[Table("orders")]
[Index(nameof(OrderNumber), IsUnique = true)]
[Index(nameof(UserId))]
[Index(nameof(Status))]
[Index(nameof(CreatedAt))]
public class Order
{
    public Order()
    {
        OrderDetails = new HashSet<OrderDetail>();
        OrderVoucherUsages = new HashSet<OrderVoucherUsage>();
        OrderId = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        PaymentStatus = PaymentStatus.Pending;
        DiscountAmount = 0;
        ShippingFee = 0;
    }

    [Key]
    [Column("order_id")]
    public Guid OrderId { get; set; }

    [StringLength(20)]
    [Column("order_number")]
    public string? OrderNumber { get; set; }
    
    [StringLength(256)]
    [Column("user_id")]
    public string? UserId { get; set; }

    [Required(ErrorMessage = "Recipient name is required.")]
    [StringLength(100)]
    public string ShippingRecipientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [StringLength(20)]
    public string ShippingPhone { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Address is required.")]
    [StringLength(500)] // Tăng độ dài để chứa toàn bộ địa chỉ
    public string ShippingAddress { get; set; } = string.Empty;

    [Required]
    [Column("status")]
    [EnumDataType(typeof(OrderStatus))]
    public OrderStatus Status { get; set; }

    [Required]
    [Column("subtotal", TypeName = "decimal(18, 2)")]
    public decimal Subtotal { get; set; }

    [Required]
    [Column("shipping_fee", TypeName = "decimal(10, 2)")]
    public decimal ShippingFee { get; set; }

    [Column("applied_voucher_id")]
    public Guid? AppliedVoucherId { get; set; }

    [Required]
    [Column("discount_amount", TypeName = "decimal(18, 2)")]
    public decimal DiscountAmount { get; set; }

    [Required]
    [Column("total_price", TypeName = "decimal(18, 2)")]
    public decimal TotalPrice { get; set; }

    [StringLength(50)]
    [Column("payment_method")]
    public string? PaymentMethod { get; set; }

    [Required]
    [Column("payment_status")]
    [EnumDataType(typeof(PaymentStatus))]
    public PaymentStatus PaymentStatus { get; set; }

    [StringLength(100)]
    [Column("shipping_method")]
    public string? ShippingMethod { get; set; }

    [StringLength(100)]
    [Column("tracking_number")]
    public string? TrackingNumber { get; set; }

    [Column("notes", TypeName = "TEXT")]
    public string? Notes { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; } = null!; 

    [ForeignKey(nameof(AppliedVoucherId))]
    public virtual Voucher? AppliedVoucher { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    public virtual ICollection<OrderVoucherUsage> OrderVoucherUsages { get; set; }
}