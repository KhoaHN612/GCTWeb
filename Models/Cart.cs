namespace GCTWeb.Models;

[Table("carts")]
[Index(nameof(UserId), IsUnique = true)]
[Index(nameof(SessionId), IsUnique = true)]
public class Cart
{
    public Cart()
    {
        CartItems = new HashSet<CartItem>();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    [Key]
    [Column("cart_id")]
    public Guid CartId { get; set; }

    [StringLength(256)]
    [Column("user_id")]
    public string? UserId { get; set; } // Nullable FK to ApplicationUser.Id

    [StringLength(128)]
    [Column("session_id")]
    public string? SessionId { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; } // Đổi ApplicationUser nếu tên khác

    public virtual ICollection<CartItem> CartItems { get; set; }
}