using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTWeb.Models;

public class ProductCreateViewModel
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Sku { get; set; }

    [Required]
    public int BrandId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public int? GradeId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative number.")]
    public uint Stock { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    
    [Display(Name = "Product Images")]
    public List<IFormFile>? NewImages { get; set; } // Danh sách các file ảnh mới
    
    [Display(Name = "Primary Image Index")]
    public int? SelectedPrimaryImageIndex { get; set; }
    
    public IEnumerable<SelectListItem>? Brands { get; set; }
    public IEnumerable<SelectListItem>? Categories { get; set; }
    public IEnumerable<SelectListItem>? Grades { get; set; }
}