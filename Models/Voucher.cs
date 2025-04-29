namespace GCTWeb.Models;

[Table("vouchers")]
[Index(nameof(Code), IsUnique = true)]
[Index(nameof(StartDate), nameof(EndDate))]
[Index(nameof(IsActive))]
public class Voucher
{
    public Voucher()
    {
        VoucherApplicabilities = new HashSet<VoucherApplicability>();
        Orders = new HashSet<Order>();
        OrderVoucherUsages = new HashSet<OrderVoucherUsage>();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsActive = true;
        UsageLimitPerUser = 1;
        MinPurchaseAmount = 0;
        CurrentUsageCount = 0;
        AppliesToEntity = VoucherApplicabilityEntity.Order;
    }

    [Key]
    [Column("voucher_id")]
    public Guid VoucherId { get; set; }

    [Required]
    [StringLength(50)]
    [Column("code")]
    public string Code { get; set; } = null!;

    [StringLength(255)]
    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [Column("discount_value", TypeName = "decimal(10, 2)")]
    public decimal DiscountValue { get; set; }

    [Required]
    [Column("discount_unit")]
    [EnumDataType(typeof(DiscountUnit))]
    public DiscountUnit DiscountUnit { get; set; }

    [Column("max_discount_amount", TypeName = "decimal(10, 2)")]
    public decimal? MaxDiscountAmount { get; set; }

    [Required]
    [Column("min_purchase_amount", TypeName = "decimal(18, 2)")]
    public decimal MinPurchaseAmount { get; set; }

    [Required]
    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Required]
    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Column("usage_limit")]
    public uint? UsageLimit { get; set; }

    [Required]
    [Column("usage_limit_per_user")]
    public uint UsageLimitPerUser { get; set; }

    [Required]
    [Column("current_usage_count")]
    public uint CurrentUsageCount { get; set; }

    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; }

    [Required]
    [Column("applies_to_entity")]
    [EnumDataType(typeof(VoucherApplicabilityEntity))]
    public VoucherApplicabilityEntity AppliesToEntity { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<VoucherApplicability> VoucherApplicabilities { get; set; }
    public virtual ICollection<Order> Orders { get; set; }
    public virtual ICollection<OrderVoucherUsage> OrderVoucherUsages { get; set; }
}