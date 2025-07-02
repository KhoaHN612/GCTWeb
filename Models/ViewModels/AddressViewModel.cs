namespace GCTWeb.Models.ViewModels;

using System;

public class AddressViewModel {
    public Guid AddressId { get; set; }
    
    [Required]
    public string RecipientName { get; set; } = null!;
    
    [Required]
    [Phone]
    public string Phone { get; set; } = null!;
    
    [Required]
    public string Street { get; set; } = null!;
    
    [Required]
    public string Ward { get; set; } = null!;
    
    [Required]
    public string City { get; set; } = null!;
    
    public string Country { get; set; } = "Vietnam";
    
    public bool IsDefaultShipping { get; set; }
    
}