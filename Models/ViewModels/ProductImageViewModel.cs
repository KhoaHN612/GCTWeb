namespace GCTWeb.Models.ViewModels;

public class ProductImageViewModel
{
    public Guid ImageId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
}