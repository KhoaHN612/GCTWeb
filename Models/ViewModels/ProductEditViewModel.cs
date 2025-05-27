using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTWeb.Models.ViewModels;

public class ProductEditViewModel
{
    public Guid ProductId { get; set; }

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
    public bool IsActive { get; set; }

    // Ảnh hiện có
    public List<ProductImageViewModel> ExistingImages { get; set; } = new List<ProductImageViewModel>();

    // ID của các ảnh hiện có mà người dùng muốn xóa
    public List<Guid>? ImagesToDelete { get; set; } = new List<Guid>();

    // Ảnh mới tải lên
    [Display(Name = "Add New Images")]
    public List<IFormFile>? NewImages { get; set; }

    // Để chọn ảnh chính:
    // Cách 1: Dùng index của ảnh MỚI (nếu có ảnh mới và muốn chọn làm chính)
    [Display(Name = "Set New Primary Image (Index from New Images)")]
    public int? SelectedNewPrimaryImageIndex { get; set; }

    // Cách 2: Dùng ID của ảnh HIỆN CÓ để đặt làm chính
    [Display(Name = "Set Existing Image as Primary")]
    public Guid? MakeExistingImagePrimaryId { get; set; }


    // For dropdowns
    public IEnumerable<SelectListItem>? Brands { get; set; }
    public IEnumerable<SelectListItem>? Categories { get; set; }
    public IEnumerable<SelectListItem>? Grades { get; set; }
}