using System.ComponentModel.DataAnnotations;

namespace User_Mangement.Models
{
    public class UserLoginDTO
    {
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.com$", ErrorMessage = "Email must be in the format user@domain.com")]
        public required string EmailId { get; set; }
        
        public required string Password { get; set; }

    }
}
