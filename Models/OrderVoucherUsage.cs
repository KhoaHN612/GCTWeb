namespace GCTWeb.Models;

[Table("order_voucher_usage")]
[Index(nameof(OrderId), nameof(UserId), nameof(VoucherId), IsUnique = true, Name = "IX_order_voucher_usage_unique")]
[Index(nameof(UserId))]
[Index(nameof(VoucherId))]
public class OrderVoucherUsage
{
    public OrderVoucherUsage()
    {
        UsedAt = DateTime.UtcNow;
    }

    [Key]
    [Column("usage_id")]
    public Guid UsageId { get; set; }

    [Required]
    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Required]
    [Column("voucher_id")]
    public Guid VoucherId { get; set; }

    [Required]
    [StringLength(256)]
    [Column("user_id")]
    public string UserId { get; set; } = null!; // FK to ApplicationUser.Id

    [Required]
    [Column("used_at")]
    public DateTime UsedAt { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey(nameof(VoucherId))]
    public virtual Voucher Voucher { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!; // Đổi ApplicationUser nếu tên khác
}