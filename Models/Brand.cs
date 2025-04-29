namespace GCTWeb.Models;

[Table("brands")]
[Index(nameof(BrandName), IsUnique = true)]
public class Brand
{
    public Brand()
    {
        Products = new HashSet<Product>();
    }

    [Key]
    [Column("brand_id")]
    public int BrandId { get; set; }

    [Required]
    [StringLength(100)]
    [Column("brand_name")]
    public string BrandName { get; set; } = null!;

    // Navigation property
    public virtual ICollection<Product> Products { get; set; }
}