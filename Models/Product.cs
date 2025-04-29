namespace GCTWeb.Models;

[Table("products")]
[Index(nameof(Name))]
[Index(nameof(Sku), IsUnique = true, Name = "IX_products_sku")]
public class Product
{
    public Product()
    {
        ProductImages = new HashSet<ProductImage>();
        ProductSales = new HashSet<ProductSale>();
        CartItems = new HashSet<CartItem>();
        OrderDetails = new HashSet<OrderDetail>();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsActive = true;
        Stock = 0;
        // PrimaryImageId và PrimaryImage sẽ là null theo mặc định
    }

    [Key]
    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Required]
    [StringLength(255)]
    [Column("name")]
    public string Name { get; set; } = null!;

    [StringLength(100)]
    [Column("sku")]
    public string? Sku { get; set; }

    [Required]
    [Column("brand_id")]
    public int BrandId { get; set; }

    [Required]
    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("grade_id")]
    public int? GradeId { get; set; }

    [Required]
    [Column("price", TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [Required]
    [Column("stock")]
    public uint Stock { get; set; }

    [Column("description", TypeName = "TEXT")]
    public string? Description { get; set; }

    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // --- Khóa ngoại mới cho ảnh chính ---
    [Column("primary_image_id")]
    public Guid? PrimaryImageId { get; set; } // Nullable Guid

    // --- Navigation properties ---
    [ForeignKey(nameof(BrandId))]
    public virtual Brand Brand { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    public virtual Category Category { get; set; } = null!;

    [ForeignKey(nameof(GradeId))]
    public virtual Grade? Grade { get; set; }

    // Navigation đến ảnh chính (mối quan hệ một-một/không)
    [ForeignKey(nameof(PrimaryImageId))]
    public virtual ProductImage? PrimaryImage { get; set; }

    // Navigation đến tất cả ảnh (mối quan hệ một-nhiều)
    public virtual ICollection<ProductImage> ProductImages { get; set; }

    // Các navigation khác
    public virtual ICollection<ProductSale> ProductSales { get; set; }
    public virtual ICollection<CartItem> CartItems { get; set; }
    public virtual ICollection<OrderDetail> OrderDetails { get; set; }
}