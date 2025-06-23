using System.ComponentModel.DataAnnotations;

namespace User_Mangement.Models
{
    public class PasswordDetailsDTO
    {
        public string? Token { get; set; }
        [Required]
        public string? Password { get; set; }
        [Required]
        public string? ConfirmPassword { get; set; }
    }
}
