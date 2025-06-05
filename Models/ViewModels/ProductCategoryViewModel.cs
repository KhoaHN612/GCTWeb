namespace GCTWeb.Models.ViewModels;

public class ProductCategoryViewModel {
    public IEnumerable<Product> Products { get; set; }
    public IEnumerable<Category> Categories { get; set; }
    public IEnumerable<Brand> Brands { get; set; }
    public IEnumerable<Grade> Grades { get; set; }
    
    public int? SelectedCategoryId { get; set; }
    public string SearchTerm { get; set; }
    public string SortOption { get; set; }
    public string BrandOption { get; set; }
    public string GradeOption { get; set; }
}