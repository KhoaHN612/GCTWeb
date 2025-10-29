namespace GCTWeb.Models.ViewModels;

using System;

public class UserDetailViewModel
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public List<AddressViewModel> Addresses { get; set; } = new();
}