using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace GCTWeb.Data;

public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Brand> Brands { get; set; } = null!;
    public DbSet<Grade> Grades { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductImage> ProductImages { get; set; } = null!;
    public DbSet<ProductSale> ProductSales { get; set; } = null!;
    public DbSet<Voucher> Vouchers { get; set; } = null!;
    public DbSet<VoucherApplicability> VoucherApplicabilities { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<Cart> Carts { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
    public DbSet<OrderVoucherUsage> OrderVoucherUsages { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // LUÔN gọi base method đầu tiên cho Identity

        // --- Cấu hình Fluent API ---

        // Chuyển đổi Enum sang String (khuyến nghị)
        builder.Entity<ProductSale>().Property(ps => ps.DiscountUnit).HasConversion<string>().HasMaxLength(50); // Thêm MaxLength nếu cần
        builder.Entity<Voucher>().Property(v => v.DiscountUnit).HasConversion<string>().HasMaxLength(50);
        builder.Entity<Voucher>().Property(v => v.AppliesToEntity).HasConversion<string>().HasMaxLength(50);
        builder.Entity<VoucherApplicability>().Property(va => va.EntityType).HasConversion<string>().HasMaxLength(50);
        builder.Entity<Order>().Property(o => o.Status).HasConversion<string>().HasMaxLength(50);
        builder.Entity<Order>().Property(o => o.PaymentStatus).HasConversion<string>().HasMaxLength(50);

        // --- Cấu hình mối quan hệ Product <-> ProductImage (QUAN TRỌNG) ---

        // 1. Mối quan hệ Một-Nhiều (Product -> ProductImages) 
        builder.Entity<ProductImage>()
            .HasOne(pi => pi.Product)          // Một ProductImage thuộc về một Product
            .WithMany(p => p.ProductImages)    // Một Product có nhiều ProductImages (qua collection này)
            .HasForeignKey(pi => pi.ProductId) // Khóa ngoại trong ProductImage là ProductId
            .OnDelete(DeleteBehavior.Cascade); // Hoặc Restrict tùy yêu cầu khi xóa Product

        // 2. Mối quan hệ Một-Một/Không (Product -> PrimaryImage) 
        builder.Entity<Product>()
            .HasOne(p => p.PrimaryImage)       // Một Product có một (hoặc không) PrimaryImage
            .WithOne()                         // Phía ProductImage KHÔNG có navigation property ngược lại *dành riêng* cho mối quan hệ này
            .HasForeignKey<Product>(p => p.PrimaryImageId) // Khóa ngoại nằm trong Product là PrimaryImageId
            .IsRequired(false)                 // Mối quan hệ này là tùy chọn (FK nullable)
            .OnDelete(DeleteBehavior.SetNull); // Khi ProductImage bị xóa, set Product.PrimaryImageId = NULL

        // --- Cấu hình ON DELETE khác (nếu cần ghi đè mặc định) ---
        builder.Entity<OrderDetail>()
           .HasOne(od => od.Product)
           .WithMany(p => p.OrderDetails)
           .HasForeignKey(od => od.ProductId)
           .OnDelete(DeleteBehavior.Restrict); // Khớp với SQL

         builder.Entity<Order>()
            .HasOne(o => o.ShippingAddress)
            .WithMany(a => a.OrdersShippedTo)
            .HasForeignKey(o => o.ShippingAddressId)
            .OnDelete(DeleteBehavior.Restrict); // VD: Không cho xóa địa chỉ đang được dùng để ship

         builder.Entity<Order>()
            .HasOne(o => o.BillingAddress)
            .WithMany(a => a.OrdersBilledTo)
            .HasForeignKey(o => o.BillingAddressId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict); // VD: Không cho xóa địa chỉ đang được dùng để bill

         // Foreign key từ Address đến User (Identity)
         builder.Entity<Address>()
            .HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Khi User bị xóa, xóa luôn Address của họ

         // Foreign key từ Cart đến User (Identity) - Optional
         builder.Entity<Cart>()
             .HasOne(c => c.User)          // Cart có thể có 1 User
             .WithOne(u => u.Cart)         // User có thể có 1 Cart (qua property Cart?)
             .HasForeignKey<Cart>(c => c.UserId) // Chỉ định FK UserId nằm trên Cart
             .IsRequired(false)            // Quan hệ là tùy chọn (UserId nullable)
             .OnDelete(DeleteBehavior.Cascade); // Khi User bị xóa, xóa Cart liên quan

         // Foreign key từ Order đến User (Identity)
         builder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict); // VD: Không cho xóa User nếu họ có Order

         // Foreign key từ OrderVoucherUsage đến User (Identity)
         builder.Entity<OrderVoucherUsage>()
            .HasOne(ovu => ovu.User)
            .WithMany(u => u.OrderVoucherUsages)
            .HasForeignKey(ovu => ovu.UserId)
            .OnDelete(DeleteBehavior.Restrict); // VD: Không cho xóa User nếu họ đã dùng Voucher

        // --- Cấu hình khác nếu cần (CHECK constraints, default values...) ---
        // Ví dụ CHECK constraint (cú pháp có thể khác nhau giữa các DB Provider)
        // builder.Entity<Product>().ToTable(tb => tb.HasCheckConstraint("CK_Product_Price", "[Price] >= 0")); // SQL Server
        // builder.Entity<CartItem>().ToTable(tb => tb.HasCheckConstraint("CK_CartItem_Quantity", "[Quantity] > 0")); // SQL Server

        // Ví dụ Default Value (nếu không muốn set trong constructor C#)
        // builder.Entity<Product>().Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()"); // SQL Server
        // builder.Entity<Product>().Property(p => p.ProductId).HasDefaultValueSql("NEWID()"); // SQL Server

    }
}