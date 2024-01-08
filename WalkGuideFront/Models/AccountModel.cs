using System.ComponentModel.DataAnnotations;

namespace WalkGuideFront.Models
{
    public class AccountModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email {  get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password length should be at least 6 characters")]
        public string Password { get; set; }
    }
}
