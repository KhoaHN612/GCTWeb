using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTWeb.Controllers.Admin;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Users()
    {
        return View();
    }

    public IActionResult Orders()
    {
        return View();
    }

    [Route("Admin/Products")]
    public async Task<IActionResult> ProductIndex()
    {
        var applicationDbContext = _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Grade)
            .Include(p => p.PrimaryImage);
        return View("../Admin/Product/Index", await applicationDbContext.ToListAsync());
    }

    [Route("Admin/Products/Create")]
    public IActionResult ProductCreate()
    {
        var viewModel = new ProductCreateViewModel
        {
            Brands = new SelectList(_context.Brands.OrderBy(b => b.BrandName), "BrandId", "BrandName"),
            Categories = new SelectList(_context.Categories.OrderBy(c => c.CategoryName), "CategoryId", "CategoryName"),
            Grades = new SelectList(_context.Grades.OrderBy(g => g.GradeName), "GradeId", "GradeName"),
            IsActive = true
        };
        return View("../Admin/Product/Create", viewModel);
    }

    // POST: Product/Create
    [Route("Admin/Products/Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductCreate(ProductCreateViewModel viewModel)
    {
        ModelState.Remove("Brands");
        ModelState.Remove("Categories");
        ModelState.Remove("Grades");

        if (ModelState.IsValid)
        {
            // --- STAGE 1: Create and Save the Product WITHOUT PrimaryImageId ---
            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                Name = viewModel.Name,
                Sku = viewModel.Sku,
                BrandId = viewModel.BrandId,
                CategoryId = viewModel.CategoryId,
                GradeId = viewModel.GradeId,
                Price = viewModel.Price,
                Stock = viewModel.Stock,
                Description = viewModel.Description,
                IsActive = viewModel.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PrimaryImageId = null // Explicitly null initially
            };

            _context.Products.Add(product);

            try
            {
                // Save the product first to ensure its ProductId is fixed in the database context
                // and to break the immediate circular dependency on ProductImage for PrimaryImageId
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error saving initial product: {ex.ToString()}");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the product information.");
                await PrepareViewModelForCreateError(viewModel);
                return View("../Admin/Product/Create", viewModel);
            }


            // --- STAGE 2: Create ProductImages, link them to the saved Product, and then update Product's PrimaryImageId ---
            var createdProductImages = new List<ProductImage>();
            ProductImage? designatedPrimaryImage = null; // To hold the image that will be primary

            if (viewModel.NewImages != null && viewModel.NewImages.Count > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "product-images");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                for (var i = 0; i < viewModel.NewImages.Count; i++)
                {
                    var imageFile = viewModel.NewImages[i];
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                        string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                        if (!permittedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("NewImages",
                                $"File {imageFile.FileName} has an invalid extension.");
                            // If there's an error here, the product is already saved.
                            // You might want to consider deleting the product or providing a way to edit it.
                            // For simplicity now, we'll just return.
                            await PrepareViewModelForCreateError(viewModel);
                            return View("../Admin/Product/Create", viewModel);
                        }

                        var serverFileName = Guid.NewGuid().ToString() + fileExtension;
                        var filePath = Path.Combine(uploadsFolder, serverFileName);

                        try
                        {
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(fileStream);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error uploading file {imageFile.FileName}: {ex.ToString()}");
                            ModelState.AddModelError("NewImages", $"Error uploading file {imageFile.FileName}.");
                            await PrepareViewModelForCreateError(viewModel);
                            return View("../Admin/Product/Create", viewModel);
                        }

                        var newProductImage = new ProductImage
                        {
                            ImageId = Guid.NewGuid(),
                            ProductId = product.ProductId, // Use the ProductId from the saved product
                            ImageUrl = "/uploads/product-images/" + serverFileName,
                            AltText = viewModel.Name,
                            IsPrimary = false,
                            CreatedAt = DateTime.UtcNow
                        };

                        // Determine if this is the primary image
                        if (viewModel.SelectedPrimaryImageIndex.HasValue &&
                            viewModel.SelectedPrimaryImageIndex.Value == i)
                        {
                            newProductImage.IsPrimary = true;
                            designatedPrimaryImage = newProductImage;
                        }

                        createdProductImages.Add(newProductImage);
                        _context.ProductImages.Add(newProductImage);
                    }
                }

                // If no primary image was explicitly selected, but images were uploaded, make the first one primary.
                if (designatedPrimaryImage == null && createdProductImages.Any())
                {
                    designatedPrimaryImage = createdProductImages.First();
                    designatedPrimaryImage.IsPrimary = true; // Mark it as primary
                }
            }

            // If there are images and a primary one was determined, update the Product
            if (designatedPrimaryImage != null)
            {
                product.PrimaryImageId = designatedPrimaryImage.ImageId;
                _context.Products.Update(product); // Tell EF Core that the product entity has been modified
                // Or, more specifically: _context.Entry(product).Property(p => p.PrimaryImageId).IsModified = true;
            }
            else if (viewModel.NewImages == null || viewModel.NewImages.Count == 0)
            {
                // No new images, ensure PrimaryImageId remains null if it was
                product.PrimaryImageId = null;
                _context.Products.Update(product);
            }


            // --- STAGE 3: Save ProductImages and the update to Product's PrimaryImageId ---
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product and images created successfully!";
                return RedirectToAction(nameof(ProductIndex));
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error saving product images or updating product: {ex.ToString()}");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the product images.");
                // At this point, product is saved, but images/primary image link might not be.
                // Rollback logic for uploaded files could be added here.
            }
        }

        await PrepareViewModelForCreateError(viewModel);
        return View("../Admin/Product/Create", viewModel);
    }


    private async Task PrepareViewModelForCreateError(ProductCreateViewModel viewModel)
    {
        viewModel.Brands = new SelectList(await _context.Brands.OrderBy(b => b.BrandName).ToListAsync(), "BrandId",
            "BrandName", viewModel.BrandId);
        viewModel.Categories = new SelectList(await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync(),
            "CategoryId", "CategoryName", viewModel.CategoryId);
        viewModel.Grades = new SelectList(await _context.Grades.OrderBy(g => g.GradeName).ToListAsync(), "GradeId",
            "GradeName", viewModel.GradeId);
    }


    public IActionResult Brands()
    {
        return View();
    }

    public IActionResult Grades()
    {
        return View();
    }

    public IActionResult Categories()
    {
        return View();
    }

    public IActionResult Vouchers()
    {
        return View();
    }
}