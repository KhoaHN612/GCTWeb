namespace GCTWeb.Models;

[Table("categories")]
[Index(nameof(CategoryName), IsUnique = true)]
public class Category
{
    public Category()
    {
        Products = new HashSet<Product>();
    }

    [Key]
    [Column("category_id")]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(100)]
    [Column("category_name")]
    public string CategoryName { get; set; } = null!;

    // Navigation property
    public virtual ICollection<Product> Products { get; set; }
}