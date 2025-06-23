using System.ComponentModel.DataAnnotations;

namespace User_Mangement.Models
{
    public class RetrivePasswordDTO
    {

        [Required]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.com$", ErrorMessage = "Email must be in the format user@domain.com")]
        public required String EmailAddress { get; set; }
    }
}
