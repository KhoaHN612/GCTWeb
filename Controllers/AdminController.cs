using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTWeb.Controllers.Admin;

public class AdminController : Controller {
    
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }
    
    public IActionResult Index() {
        return View();
    }
    
    public IActionResult Users() {
        return View();
    }
    
    public IActionResult Orders() {
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
    
    [Route("Admin/Brand")]
    public async Task<IActionResult> BrandIndex() {
        var applicationDbContext = _context.Brands.ToListAsync();
        return View("../Admin/Brand/Index", await applicationDbContext);
    }
    
    [HttpPost, ActionName("CreateBrand")]
    [ValidateAntiForgeryToken]
    [Route("Admin/Brand/Create")]
    public async Task<IActionResult> Create([Bind("BrandId,BrandName")] Brand brand) {
        if (ModelState.IsValid) {
            try {
                _context.Add(brand);
                await _context.SaveChangesAsync();
                TempData["success"] = $"Brand '{brand.BrandName}' added successfully!";
                return RedirectToAction(nameof(BrandIndex)); // Redirect back to the list
            }
            catch (Exception ex) {
                TempData["error"] = "An error occurred while saving the brand.";
                return RedirectToAction(nameof(BrandIndex));
            }
        }
        
        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        TempData["error"] = "Failed to add brand: " + string.Join(" ", errors);
        return RedirectToAction(nameof(BrandIndex));
    }
    
    [HttpPost, ActionName("DeleteBrand")] 
    [ValidateAntiForgeryToken]       
    [Route("Admin/Brand/Delete/{id}")] 
    public async Task<IActionResult> DeleteBrandConfirmed(int id) {
        if (_context.Brands == null) {
            TempData["error"] = "Brand data context not found.";
            return RedirectToAction(nameof(BrandIndex)); // Redirect to Brand list
        }
        
        var brand = await _context.Brands.FindAsync(id);
        if (brand != null) {
            try {
                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
                TempData["success"] = $"Brand '{brand.BrandName}' deleted successfully!"; // Use TempData for feedback
            }
            catch (DbUpdateException) {
                TempData["error"] = $"Error deleting brand '{brand.BrandName}'. It might be associated with existing products.";
            }
        }
        else {
            TempData["error"] = "Brand not found.";
        }
        
        return RedirectToAction(nameof(BrandIndex));
    }
    
    // --- End Brand Actions ---
    
    [Route("Admin/Category")]
    public async Task<IActionResult> CategoryIndex() {
        var applicationDbContext = _context.Categories.ToListAsync();
        return View("../Admin/Category/Index", await applicationDbContext);
    }
    
    [HttpPost, ActionName("CreateCategory")]
    [ValidateAntiForgeryToken]
    [Route("Admin/Category/Create")]
    public async Task<IActionResult> Create([Bind("CategoryID,CategoryName")] Category category) {
        if (ModelState.IsValid) {
            try {
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["success"] = $"Category '{category.CategoryName}' added successfully!";
                return RedirectToAction(nameof(CategoryIndex)); // Redirect back to the list
            }
            catch (Exception ex) {
                TempData["error"] = "An error occurred while saving the category.";
                return RedirectToAction(nameof(CategoryIndex));
            }
        }
        
        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        TempData["error"] = "Failed to add category: " + string.Join(" ", errors);
        return RedirectToAction(nameof(CategoryIndex));
    }
    
    [HttpPost, ActionName("DeleteCategory")]
    [ValidateAntiForgeryToken]
    [Route("Admin/Category/Delete/{id}")]
    public async Task<IActionResult> DeleteCategoryConfirmed(int id) {
        if (_context.Categories == null) {
            TempData["error"] = "Category data context not found.";
            return RedirectToAction(nameof(CategoryIndex)); 
        }
        
        var category = await _context.Categories.FindAsync(id);
        if (category != null) {
            try {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["success"] = $"Category '{category.CategoryName}' deleted successfully!"; // Use TempData for feedback
            }
            catch (DbUpdateException) {
                TempData["error"] = $"Error deleting category '{category.CategoryName}'. It might be associated with existing products.";
            }
        }
        else {
            TempData["error"] = "Category not found.";
        }
        
        return RedirectToAction(nameof(CategoryIndex));
    }
    
    [Route("Admin/Grade")]
    public async Task<IActionResult> GradeIndex() {
        var applicationDbContext = _context.Grades.ToListAsync();
        return View("../Admin/Grade/Index", await applicationDbContext);
    }
    
    [HttpPost, ActionName("CreateGrade")]
    [ValidateAntiForgeryToken]
    [Route("Admin/Grade/Create")]
    public async Task<IActionResult> Create([Bind("GradeID,GradeName")] Grade grade) {
        if (ModelState.IsValid) {
            try {
                _context.Add(grade);
                await _context.SaveChangesAsync();
                TempData["success"] = $"Grade '{grade.GradeName}' added successfully!";
                return RedirectToAction(nameof(GradeIndex)); // Redirect back to the list
            }
            catch (Exception ex) {
                TempData["error"] = "An error occurred while saving the grade.";
                return RedirectToAction(nameof(GradeIndex));
            }
        }
        
        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        TempData["error"] = "Failed to add grade: " + string.Join(" ", errors);
        return RedirectToAction(nameof(GradeIndex));
    }
    
    [HttpPost, ActionName("DeleteGrade")]
    [ValidateAntiForgeryToken]
    [Route("Admin/Grade/Delete/{id}")]
    public async Task<IActionResult> DeleteGradeConfirmed(int id) {
        if (_context.Grades == null) {
            TempData["error"] = "Grade data context not found.";
            return RedirectToAction(nameof(GradeIndex)); 
        }
        
        var grade = await _context.Grades.FindAsync(id);
        if (grade != null) {
            try {
                _context.Grades.Remove(grade);
                await _context.SaveChangesAsync();
                TempData["success"] = $"Grade '{grade.GradeName}' deleted successfully!"; // Use TempData for feedback
            }
            catch (DbUpdateException) {
                TempData["error"] = $"Error deleting grade '{grade.GradeName}'. It might be associated with existing products.";
            }
        }
        else {
            TempData["error"] = "Grade not found.";
        }
        
        return RedirectToAction(nameof(GradeIndex));
    }
    
    public IActionResult Vouchers() {
        return View();
    }
}