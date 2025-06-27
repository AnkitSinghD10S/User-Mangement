using System.ComponentModel.DataAnnotations;

namespace User_Mangement.Models
{
    public class UserDetails
    {
        [Key]
        public required int UserID { get; set; }
            
        public required string FirstName { get; set; }

        public required String LastName { get; set; }

        [Required(ErrorMessage ="Date of Birth is required")]
        public  DateOnly DOB { get; set; }

        public  required String Gender { get; set; }

        [Required]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.com$",ErrorMessage = "Email must be in the format user@domain.com")]
        public required String EmailAddress { get; set; }

    }
}
