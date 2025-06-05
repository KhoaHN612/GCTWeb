using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GCTWeb.Data;
using GCTWeb.Models;

namespace GCTWeb.Controllers
{
    /*[Route("api/productapi")]*/
    [Route("api/[controller]")]
    [ApiController]
    public class ProductAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductAPIController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/productapi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // GET: api/productapi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }
        
        // GET: api/productapi/5
        [HttpGet("{id}/quickview")]
        public async Task<ActionResult<Product>> GetProductQuickViewDetails(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid product ID.");
            }
            
            var product = await _context.Products
                                        .Include(p => p.Brand)
                                        .Include(p => p.Category)
                                        .Include(p => p.ProductImages)
                                        .Where(p => p.ProductId == id && p.IsActive)
                                        .Select(p => new // DTO cho Quick View
                                        {
                                            p.ProductId,
                                            p.Name,
                                            Price = p.Price, 
                                            Description = p.Description, 
                                            // Thêm các thông tin cần thiết khác
                                            // BrandName = p.Brand != null ? p.Brand.BrandName : null,
                                            // CategoryName = p.Category != null ? p.Category.CategoryName : null,
                                            Images = p.ProductImages
                                                       .OrderByDescending(img => img.IsPrimary)
                                                       .ThenBy(img => img.CreatedAt)
                                                       .Select(img => new {
                                                           ImageUrl = img.ImageUrl, // Ví dụ: "/uploads/product-images/image.jpg"
                                                           AltText = img.AltText ?? p.Name,
                                                           IsPrimary = img.IsPrimary,
                                                           DataThumb = img.ImageUrl // Dùng ImageUrl làm data-thumb
                                                       }).ToList()
                                            // Ví dụ thêm các tùy chọn nếu có:
                                            // Sizes = p.ProductVariants.Select(v => v.Size).Distinct().ToList(),
                                            // Colors = p.ProductVariants.Select(v => v.Color).Distinct().ToList()
                                        })
                                        .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound($"Product with ID {id} not found or is not active.");
            }
            return Ok(product);
        }

        // PUT: api/productapi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(Guid id, Product product)
        {
            if (id != product.ProductId)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/productapi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        // DELETE: api/productapi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(Guid id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
