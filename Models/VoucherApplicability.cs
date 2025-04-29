namespace GCTWeb.Models;

[Table("voucher_applicability")]
[Index(nameof(VoucherId), nameof(EntityType), nameof(EntityId), IsUnique = true, Name = "IX_voucher_applicability_unique")]
[Index(nameof(EntityType), nameof(EntityId))]
public class VoucherApplicability
{
    [Key]
    [Column("voucher_applicability_id")]
    public int VoucherApplicabilityId { get; set; }

    [Required]
    [Column("voucher_id")]
    public Guid VoucherId { get; set; }

    [Required]
    [Column("entity_type")]
    [EnumDataType(typeof(VoucherApplicabilityEntity))]
    public VoucherApplicabilityEntity EntityType { get; set; } // Chỉ nên là Product, Category, Brand

    [Required]
    [StringLength(36)]
    [Column("entity_id")]
    public string EntityId { get; set; } = null!;

    [ForeignKey(nameof(VoucherId))]
    public virtual Voucher Voucher { get; set; } = null!;
}