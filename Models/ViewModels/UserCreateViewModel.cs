using System.ComponentModel.DataAnnotations;

namespace GCTWeb.Models.ViewModels
{
    public class AddUserViewModel
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        public string Role { get; set; } = null!;
    }
}