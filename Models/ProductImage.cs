namespace GCTWeb.Models;

[Table("product_images")]
[Index(nameof(ProductId))]
public class ProductImage
{
    public ProductImage()
    {
        CreatedAt = DateTime.UtcNow;
        IsPrimary = false;
    }

    [Key]
    [Column("image_id")]
    public Guid ImageId { get; set; }

    [Required]
    [Column("product_id")]
    public Guid ProductId { get; set; }

    [NotMapped]
    public IFormFile? ImageFile { get; set; }
    
    [Required]
    [StringLength(512)]
    [Column("image_url")]
    public string ImageUrl { get; set; } = null!;

    [StringLength(255)]
    [Column("alt_text")]
    public string? AltText { get; set; }

    [Required]
    [Column("is_primary")]
    public bool IsPrimary { get; set; } // Logic để set cái này cần cập nhật PrimaryImageId trong Product

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // Navigation property (cho mối quan hệ một-nhiều)
    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; } = null!;
    
}