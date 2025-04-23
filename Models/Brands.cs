namespace GCTWeb.Models;

[Table("brands")]
public class Brand
{
    [Key]
    [Column("brand_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public uint BrandId { get; set; }

    [Required]
    [Column("brand_name")]
    [StringLength(100)]
    public string BrandName { get; set; }
}