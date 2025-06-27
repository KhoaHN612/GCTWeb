namespace GCTWeb.Models;

[Table("addresses")]
[Index(nameof(UserId))]
public class Address
{
    public Address()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsDefaultShipping = false;
        IsDefaultBilling = false;
        Country = "Vietnam";
    }

    [Key]
    [Column("address_id")]
    public Guid AddressId { get; set; }

    [Required]
    [StringLength(256)]
    [Column("user_id")]
    public string UserId { get; set; } = null!; // FK to ApplicationUser.Id

    [Required]
    [StringLength(100)]
    [Column("recipient_name")]
    public string RecipientName { get; set; } = null!;

    [Required]
    [StringLength(20)]
    [Phone]
    [Column("phone")]
    public string Phone { get; set; } = null!;

    [Required]
    [StringLength(255)]
    [Column("street")]
    public string Street { get; set; } = null!;

    [Required]
    [StringLength(100)]
    [Column("ward")]
    public string Ward { get; set; } = null!;

    [Required]
    [StringLength(100)]
    [Column("city")]
    public string City { get; set; } = null!;

    [Required]
    [StringLength(100)]
    [Column("country")]
    public string Country { get; set; } = null!;

    [Required]
    [Column("is_default_shipping")]
    public bool IsDefaultShipping { get; set; }

    [Required]
    [Column("is_default_billing")]
    public bool IsDefaultBilling { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!; // Đổi ApplicationUser nếu tên khác
    
}