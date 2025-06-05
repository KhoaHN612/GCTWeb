using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GCTWeb.Data;
using GCTWeb.Models;
using GCTWeb.Models.ViewModels;

namespace GCTWeb.Controllers_
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Product
        public async Task<IActionResult> Index(int? categoryId, string searchTerm, string sortOption, string brandOption, string gradeOption)
        {
            var productsQuery = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Grade)
                .Include(p => p.PrimaryImage)
                .Where(p => p.IsActive)
                .AsQueryable();
            
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
            }
            
            switch (sortOption)
            {
                case "price_asc":
                    productsQuery = productsQuery.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Price);
                    break;
                case "newest":
                    productsQuery = productsQuery.OrderByDescending(p => p.CreatedAt);
                    break;
                default:
                    productsQuery = productsQuery.OrderBy(p => p.Name); // hoặc không sắp xếp gì cả
                    break;
            }
            
            if (!string.IsNullOrWhiteSpace(brandOption)) {
                productsQuery = productsQuery.Where(p => p.Brand.BrandName == brandOption);
            }
            
            if (!string.IsNullOrWhiteSpace(gradeOption)) {
                productsQuery = productsQuery.Where(p => p.Grade.GradeName == gradeOption);
            }
            
            var viewModel = new ProductCategoryViewModel
            {
                Products = await productsQuery.ToListAsync(),
                Categories = await _context.Categories.ToListAsync(),
                Brands = await _context.Brands.ToListAsync(),
                Grades = await _context.Grades.ToListAsync(),
                SelectedCategoryId = categoryId,
                SearchTerm = searchTerm,
                SortOption = sortOption,
                BrandOption = brandOption,
                GradeOption = gradeOption
            };
            
            return View(viewModel);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Detail(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Grade)
                .Include(p => p.ProductImages) 
                .FirstOrDefaultAsync(m => m.ProductId == id && m.IsActive); 

            if (product == null)
            {
                return NotFound();
            }
            
            if (product.ProductImages.Any() && product.PrimaryImageId.HasValue)
            {
                product.ProductImages = product.ProductImages
                    .OrderByDescending(img => img.ImageId == product.PrimaryImageId)
                    .ThenBy(img => img.CreatedAt) 
                    .ToList();
            }
            else if (product.ProductImages.Any())
            {
                product.ProductImages = product.ProductImages.OrderBy(img => img.CreatedAt).ToList();
            }
            
            return View(product);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandName");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            ViewData["GradeId"] = new SelectList(_context.Grades, "GradeId", "GradeName");
            ViewData["PrimaryImageId"] = new SelectList(_context.ProductImages, "ImageId", "ImageUrl");
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Name,Sku,BrandId,CategoryId,GradeId,Price,Stock,Description,IsActive,CreatedAt,UpdatedAt,PrimaryImageId")] Product product)
        {
            /*foreach (var kvp in ModelState)
            {
                Console.WriteLine($"{kvp.Key} => {(kvp.Value?.Errors.Count > 0 ? string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage)) : "OK")}");
            }*/
            if (ModelState.IsValid)
            {
                product.ProductId = Guid.NewGuid();
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandName", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["GradeId"] = new SelectList(_context.Grades, "GradeId", "GradeName", product.GradeId);
            ViewData["PrimaryImageId"] = new SelectList(_context.ProductImages, "ImageId", "ImageUrl", product.PrimaryImageId);
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandName", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["GradeId"] = new SelectList(_context.Grades, "GradeId", "GradeName", product.GradeId);
            ViewData["PrimaryImageId"] = new SelectList(_context.ProductImages, "ImageId", "ImageUrl", product.PrimaryImageId);
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ProductId,Name,Sku,BrandId,CategoryId,GradeId,Price,Stock,Description,IsActive,CreatedAt,UpdatedAt,PrimaryImageId")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandName", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["GradeId"] = new SelectList(_context.Grades, "GradeId", "GradeName", product.GradeId);
            ViewData["PrimaryImageId"] = new SelectList(_context.ProductImages, "ImageId", "ImageUrl", product.PrimaryImageId);
            return View(product);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Grade)
                .Include(p => p.PrimaryImage)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(Guid id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
