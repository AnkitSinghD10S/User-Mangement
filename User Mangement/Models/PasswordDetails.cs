using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace User_Mangement.Models
{
    public class PasswordDetails
    {
        [Key]
        public int Id { set; get; }

        [ForeignKey("UserDetails")]
        public required int UserId { get; set; }

        public required string Password { get; set; }


    }
}
