using GCTWeb.Models.Enums;

namespace GCTWeb.Models;

[Table("product_sales")]
[Index(nameof(ProductId))]
[Index(nameof(StartDate), nameof(EndDate))]
public class ProductSale
{
    [Key]
    [Column("product_sale_id")]
    public Guid ProductSaleId { get; set; }

    [Required]
    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Required]
    [Column("discount_value", TypeName = "decimal(10, 2)")]
    public decimal DiscountValue { get; set; }

    [Required]
    [Column("discount_unit")]
    [EnumDataType(typeof(DiscountUnit))]
    public DiscountUnit DiscountUnit { get; set; }

    [Required]
    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Required]
    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;

    [NotMapped]
    public bool IsActive => StartDate <= DateTime.UtcNow && EndDate >= DateTime.UtcNow;
}