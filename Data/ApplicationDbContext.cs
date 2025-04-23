using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace GCTWeb.Data;

public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Brand> Brands { get; set; }
}