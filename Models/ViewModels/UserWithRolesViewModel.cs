namespace GCTWeb.Models.ViewModels;

public class UserWithRolesViewModel
{
    public string Id { get; set; }
    public string? Name { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = new();
}