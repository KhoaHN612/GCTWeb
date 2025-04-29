namespace GCTWeb.Models;

[Table("grades")]
[Index(nameof(GradeName), IsUnique = true)]
public class Grade
{
    public Grade()
    {
        Products = new HashSet<Product>();
    }

    [Key]
    [Column("grade_id")]
    public int GradeId { get; set; }

    [Required]
    [StringLength(100)]
    [Column("grade_name")]
    public string GradeName { get; set; } = null!;

    // Navigation property
    public virtual ICollection<Product> Products { get; set; }
}