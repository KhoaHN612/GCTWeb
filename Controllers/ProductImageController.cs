using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GCTWeb.Data;
using GCTWeb.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GCTWeb.Controllers_
{
    public class ProductImageController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductImageController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: ProductImage
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ProductImages.Include(p => p.Product);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ProductImage/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productImage = await _context.ProductImages
                .Include(p => p.Product)
                .FirstOrDefaultAsync(m => m.ImageId == id);
            if (productImage == null)
            {
                return NotFound();
            }

            return View(productImage);
        }

        // GET: ProductImage/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name");
            return View();
        }

        // POST: ProductImage/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ImageId,ProductId,ImageFile,AltText,IsPrimary,CreatedAt")] ProductImage productImage)
        {
            if (productImage.ImageFile == null || productImage.ImageFile.Length == 0)
                return BadRequest("No file uploaded");
                
            productImage.ImageId = Guid.NewGuid();
            var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "product-images");
            var fileExtension = Path.GetExtension(productImage.ImageFile.FileName);
            var fileName = productImage.ImageId.ToString() + fileExtension;
            var fullPath = Path.Combine(folderPath, fileName);
            var relativePath = Path.Combine("uploads", "product-images", fileName);
            
                
            using var memoryStream = new MemoryStream();
            await productImage.ImageFile.CopyToAsync(memoryStream); //Temp save (limit <5mb)
            
            /*
             Directory.CreateDirectory(folderPath);
             using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await productImage.ImageFile.CopyToAsync(stream); // Save directly into server not thought anything (maybe not right) 
            }*/
                
            productImage.ImageUrl = relativePath;
            ModelState.Remove(nameof(productImage.ImageUrl)); 
            ModelState.SetModelValue("ImageUrl", new ValueProviderResult(relativePath));
            TryValidateModel(productImage);
            
            if (ModelState.IsValid)
            {
                _context.Add(productImage);
                await _context.SaveChangesAsync();
                
                Directory.CreateDirectory(folderPath);
                await System.IO.File.WriteAllBytesAsync(fullPath, memoryStream.ToArray()); // If valid save file to server
                
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", productImage.ProductId);
            return View(productImage);
        }

        // GET: ProductImage/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productImage = await _context.ProductImages.FindAsync(id);
            if (productImage == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", productImage.ProductId);
            return View(productImage);
        }

        // POST: ProductImage/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ImageId,ProductId,ImageUrl,AltText,IsPrimary,CreatedAt")] ProductImage productImage)
        {
            if (id != productImage.ImageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductImageExists(productImage.ImageId))
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
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", productImage.ProductId);
            return View(productImage);
        }

        // GET: ProductImage/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productImage = await _context.ProductImages
                .Include(p => p.Product)
                .FirstOrDefaultAsync(m => m.ImageId == id);
            if (productImage == null)
            {
                return NotFound();
            }

            return View(productImage);
        }

        // POST: ProductImage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var productImage = await _context.ProductImages.FindAsync(id);
            if (productImage != null)
            {
                _context.ProductImages.Remove(productImage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductImageExists(Guid id)
        {
            return _context.ProductImages.Any(e => e.ImageId == id);
        }
    }
}
