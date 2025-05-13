using Microsoft.AspNetCore.Mvc;

namespace GCTWeb.Controllers.Admin;

public class AdminController : Controller {
    
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context) {
        _context = context;
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
    
    [Route("Admin/Product")]
    public async Task<IActionResult> ProductIndex() {
        var applicationDbContext = _context.Products.Include(p => p.Brand).Include(p => p.Category).Include(p => p.Grade).Include(p => p.PrimaryImage);
        return View("../Admin/Product/Index",await applicationDbContext.ToListAsync());
    }
    
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