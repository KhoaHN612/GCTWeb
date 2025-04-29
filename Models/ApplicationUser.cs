using Microsoft.AspNetCore.Identity;

namespace GCTWeb.Models;

public class ApplicationUser: IdentityUser
{
    public ApplicationUser()
    {
        // Khởi tạo các collection
        Addresses = new HashSet<Address>();
        Orders = new HashSet<Order>();
        OrderVoucherUsages = new HashSet<OrderVoucherUsage>();
    }
    
    public string Name { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<Address> Addresses { get; set; }
    public virtual ICollection<Order> Orders { get; set; }
    public virtual Cart? Cart { get; set; } 
    public virtual ICollection<OrderVoucherUsage> OrderVoucherUsages { get; set; }
}