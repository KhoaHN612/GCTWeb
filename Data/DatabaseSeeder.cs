using GCTWeb.Models.Enums;

namespace GCTWeb.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Kiểm tra nếu đã có data thì không seed nữa
        if (context.Brands.Any() || context.Products.Any())
        {
            Console.WriteLine("[SEED] Database already has data. Skipping seed.");
            return;
        }

        Console.WriteLine("[SEED] Starting database seeding...");

        // 1. Seed Brands (Gunpla Manufacturers)
        var brands = new List<Brand>
        {
            new Brand { BrandId = 1, BrandName = "Bandai" },
            new Brand { BrandId = 2, BrandName = "Kotobukiya" },
            new Brand { BrandId = 3, BrandName = "Good Smile Company" },
            new Brand { BrandId = 4, BrandName = "Flame Toys" },
            new Brand { BrandId = 5, BrandName = "Daban Model" },
            new Brand { BrandId = 6, BrandName = "Dragon Momoko" },
            new Brand { BrandId = 7, BrandName = "Third Party" }
        };
        await context.Brands.AddRangeAsync(brands);
        await context.SaveChangesAsync();
        Console.WriteLine($"[SEED] Added {brands.Count} brands");

        // 2. Seed Categories (Gunpla Grades)
        var categories = new List<Category>
        {
            new Category { CategoryId = 1, CategoryName = "High Grade (HG)" },
            new Category { CategoryId = 2, CategoryName = "Master Grade (MG)" },
            new Category { CategoryId = 3, CategoryName = "Real Grade (RG)" },
            new Category { CategoryId = 4, CategoryName = "Perfect Grade (PG)" },
            new Category { CategoryId = 5, CategoryName = "SD Gundam" },
            new Category { CategoryId = 6, CategoryName = "Figure-rise Standard" },
            new Category { CategoryId = 7, CategoryName = "30 Minutes Missions" },
            new Category { CategoryId = 8, CategoryName = "Mega Size Model" }
        };
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
        Console.WriteLine($"[SEED] Added {categories.Count} categories");

        // 3. Seed Grades (Product Condition)
        var grades = new List<Grade>
        {
            new Grade { GradeId = 1, GradeName = "Chưa lắp ráp - Nguyên seal" },
            new Grade { GradeId = 2, GradeName = "Đã build - Chưa sơn" },
            new Grade { GradeId = 3, GradeName = "Đã build - Đã sơn" },
            new Grade { GradeId = 4, GradeName = "Customized" },
            new Grade { GradeId = 5, GradeName = "Pre-built" }
        };
        await context.Grades.AddRangeAsync(grades);
        await context.SaveChangesAsync();
        Console.WriteLine($"[SEED] Added {grades.Count} grades");

        // 4. Seed Products (Gunpla Models)
        var products = new List<Product>
        {
            // High Grade (HG) - Bandai
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "HG RX-78-2 Gundam (REVIVE)",
                Sku = "BAN-HG-RX78",
                BrandId = 1,
                CategoryId = 1,
                GradeId = 1,
                Price = 450000,
                Stock = 100,
                Description = "Mô hình Gundam gốc với thiết kế cải tiến, khớp linh hoạt, đi kèm nhiều vũ khí",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "HG Zaku II Char Custom",
                Sku = "BAN-HG-ZAKU2C",
                BrandId = 1,
                CategoryId = 1,
                GradeId = 1,
                Price = 420000,
                Stock = 85,
                Description = "Zaku II phiên bản đỏ của Char Aznable, tỷ lệ 1/144 với chi tiết sắc nét",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "HG Gundam Barbatos Lupus Rex",
                Sku = "BAN-HG-BARBLR",
                BrandId = 1,
                CategoryId = 1,
                GradeId = 1,
                Price = 550000,
                Stock = 70,
                Description = "Gundam từ IBO series, thiết kế hung tợn với mace và tail blade",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // Master Grade (MG) - Bandai
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "MG Nu Gundam Ver.Ka",
                Sku = "BAN-MG-NU",
                BrandId = 1,
                CategoryId = 2,
                GradeId = 1,
                Price = 2100000,
                Stock = 45,
                Description = "Master Grade Nu Gundam thiết kế bởi Katoki, đầy đủ Fin Funnel, khung nội tuyệt đẹp",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "MG Sazabi Ver.Ka",
                Sku = "BAN-MG-SAZABI",
                BrandId = 1,
                CategoryId = 2,
                GradeId = 1,
                Price = 2300000,
                Stock = 40,
                Description = "Mobile suit khổng lồ của Char, chi tiết cực kỳ phong phú, đầy đủ decal waterslide",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "MG Strike Freedom Gundam",
                Sku = "BAN-MG-SFREE",
                BrandId = 1,
                CategoryId = 2,
                GradeId = 1,
                Price = 1850000,
                Stock = 60,
                Description = "Gundam với hệ thống cánh vàng đặc trưng, trang bị đầy đủ vũ khí tầm xa",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // Real Grade (RG) - Bandai
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "RG Unicorn Gundam",
                Sku = "BAN-RG-UNICORN",
                BrandId = 1,
                CategoryId = 3,
                GradeId = 1,
                Price = 1100000,
                Stock = 75,
                Description = "Mô hình RG có thể biến hình giữa Unicorn mode và Destroy mode, LED kit tương thích",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "RG Wing Gundam EW",
                Sku = "BAN-RG-WING",
                BrandId = 1,
                CategoryId = 3,
                GradeId = 1,
                Price = 950000,
                Stock = 80,
                Description = "Wing Gundam phiên bản Endless Waltz, cánh thiên thần có thể gấp mở",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // Perfect Grade (PG) - Bandai
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "PG Unleashed RX-78-2 Gundam",
                Sku = "BAN-PG-RX78UL",
                BrandId = 1,
                CategoryId = 4,
                GradeId = 1,
                Price = 6500000,
                Stock = 15,
                Description = "Perfect Grade thế hệ mới với khung nội cực chi tiết, có thể tháo rời từng phần giáp",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "PG Unicorn Gundam LED",
                Sku = "BAN-PG-UNICLED",
                BrandId = 1,
                CategoryId = 4,
                GradeId = 1,
                Price = 7200000,
                Stock = 12,
                Description = "PG Unicorn với hệ thống LED tích hợp sẵn, biến hình hoàn toàn, cao 50cm",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // SD Gundam - Bandai
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "SD Cross Silhouette RX-78-2",
                Sku = "BAN-SD-RX78CS",
                BrandId = 1,
                CategoryId = 5,
                GradeId = 1,
                Price = 320000,
                Stock = 120,
                Description = "SD Gundam phong cách chibi với khớp linh hoạt, dễ build cho người mới",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "SD Sazabi",
                Sku = "BAN-SD-SAZABI",
                BrandId = 1,
                CategoryId = 5,
                GradeId = 1,
                Price = 380000,
                Stock = 90,
                Description = "SD Sazabi phong cách dễ thương nhưng vẫn giữ được sự hùng vĩ của bản gốc",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // Kotobukiya Models
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "Frame Arms Girl Hresvelgr",
                Sku = "KOTO-FAG-HRES",
                BrandId = 2,
                CategoryId = 6,
                GradeId = 1,
                Price = 1250000,
                Stock = 40,
                Description = "Frame Arms Girl với thiết kế cơ khí kết hợp nhân vật nữ, nhiều pose option",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "Megami Device Asra Ninja",
                Sku = "KOTO-MD-ASRA",
                BrandId = 2,
                CategoryId = 6,
                GradeId = 1,
                Price = 1150000,
                Stock = 50,
                Description = "Megami Device phong cách ninja với nhiều vũ khí và phụ kiện đi kèm",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // 30 Minutes Missions
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "30MM Alto (Green)",
                Sku = "BAN-30MM-ALTOG",
                BrandId = 1,
                CategoryId = 7,
                GradeId = 1,
                Price = 350000,
                Stock = 100,
                Description = "Mô hình 30 Minutes Missions có thể custom, tương thích với toàn bộ dòng 30MM",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // Third Party
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "Dragon Momoko MG Destiny Gundam",
                Sku = "DM-MG-DESTINY",
                BrandId = 6,
                CategoryId = 2,
                GradeId = 1,
                Price = 950000,
                Stock = 35,
                Description = "Third party MG Destiny với chất lượng tốt, chi tiết sắc nét, giá phải chăng",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
        Console.WriteLine($"[SEED] Added {products.Count} products");

        // 5. Seed Product Sales (một vài mô hình đang sale)
        var productList = await context.Products.ToListAsync();
        var productSales = new List<ProductSale>
        {
            new ProductSale
            {
                ProductSaleId = Guid.NewGuid(),
                ProductId = productList.First(p => p.Sku == "BAN-HG-RX78").ProductId,
                DiscountValue = 15,
                DiscountUnit = DiscountUnit.Percentage,
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow.AddDays(7)
            },
            new ProductSale
            {
                ProductSaleId = Guid.NewGuid(),
                ProductId = productList.First(p => p.Sku == "BAN-MG-NU").ProductId,
                DiscountValue = 200000,
                DiscountUnit = DiscountUnit.FixedAmount,
                StartDate = DateTime.UtcNow.AddDays(-5),
                EndDate = DateTime.UtcNow.AddDays(10)
            },
            new ProductSale
            {
                ProductSaleId = Guid.NewGuid(),
                ProductId = productList.First(p => p.Sku == "BAN-RG-UNICORN").ProductId,
                DiscountValue = 10,
                DiscountUnit = DiscountUnit.Percentage,
                StartDate = DateTime.UtcNow.AddDays(-3),
                EndDate = DateTime.UtcNow.AddDays(14)
            },
            new ProductSale
            {
                ProductSaleId = Guid.NewGuid(),
                ProductId = productList.First(p => p.Sku == "BAN-SD-RX78CS").ProductId,
                DiscountValue = 50000,
                DiscountUnit = DiscountUnit.FixedAmount,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(6)
            }
        };

        await context.ProductSales.AddRangeAsync(productSales);
        await context.SaveChangesAsync();
        Console.WriteLine($"[SEED] Added {productSales.Count} product sales");

        // 6. Seed Vouchers
        var vouchers = new List<Voucher>
        {
            new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "GUNPLA2024",
                Description = "Voucher chào mừng Gunpla Builder - Giảm 10%",
                DiscountValue = 10,
                DiscountUnit = DiscountUnit.Percentage,
                MaxDiscountAmount = 300000,
                MinPurchaseAmount = 500000,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(60),
                UsageLimit = 1000,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                AppliesToEntity = VoucherApplicabilityEntity.Order,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "MEGASALE200K",
                Description = "Mega Sale - Giảm 200K cho đơn từ 2 triệu",
                DiscountValue = 200000,
                DiscountUnit = DiscountUnit.FixedAmount,
                MaxDiscountAmount = null,
                MinPurchaseAmount = 2000000,
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow.AddDays(7),
                UsageLimit = 500,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                AppliesToEntity = VoucherApplicabilityEntity.Order,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "BANDAI25",
                Description = "Giảm 25% cho sản phẩm Bandai",
                DiscountValue = 25,
                DiscountUnit = DiscountUnit.Percentage,
                MaxDiscountAmount = 500000,
                MinPurchaseAmount = 1000000,
                StartDate = DateTime.UtcNow.AddDays(-5),
                EndDate = DateTime.UtcNow.AddDays(25),
                UsageLimit = 200,
                UsageLimitPerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true,
                AppliesToEntity = VoucherApplicabilityEntity.Brand,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "MGRADE20",
                Description = "Giảm 20% cho Master Grade",
                DiscountValue = 20,
                DiscountUnit = DiscountUnit.Percentage,
                MaxDiscountAmount = 400000,
                MinPurchaseAmount = 1500000,
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(20),
                UsageLimit = 150,
                UsageLimitPerUser = 2,
                CurrentUsageCount = 0,
                IsActive = true,
                AppliesToEntity = VoucherApplicabilityEntity.Category,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await context.Vouchers.AddRangeAsync(vouchers);
        await context.SaveChangesAsync();
        Console.WriteLine($"[SEED] Added {vouchers.Count} vouchers");

        // 7. Seed Voucher Applicability cho voucher Bandai và Master Grade
        var bandaiVoucher = vouchers.First(v => v.Code == "BANDAI25");
        var mgVoucher = vouchers.First(v => v.Code == "MGRADE20");
        var bandaiBrand = brands.First(b => b.BrandName == "Bandai");
        var mgCategory = categories.First(c => c.CategoryName == "Master Grade (MG)");

        var voucherApplicabilities = new List<VoucherApplicability>
        {
            new VoucherApplicability
            {
                VoucherId = bandaiVoucher.VoucherId,
                EntityType = VoucherApplicabilityEntity.Brand,
                EntityId = bandaiBrand.BrandId.ToString()
            },
            new VoucherApplicability
            {
                VoucherId = mgVoucher.VoucherId,
                EntityType = VoucherApplicabilityEntity.Category,
                EntityId = mgCategory.CategoryId.ToString()
            }
        };

        await context.VoucherApplicabilities.AddRangeAsync(voucherApplicabilities);
        await context.SaveChangesAsync();
        Console.WriteLine($"[SEED] Added {voucherApplicabilities.Count} voucher applicabilities");

        Console.WriteLine("[SEED] Database seeding completed successfully!");
    }
}
