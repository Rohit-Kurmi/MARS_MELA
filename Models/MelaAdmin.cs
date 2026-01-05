using System.ComponentModel.DataAnnotations;

namespace MARS_MELA_PROJECT.Models
{
    public class MelaAdmin
    {
        public int RoleID { get; set; }

        [Required(ErrorMessage = "Mobile number required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile must be 10 digits")]
        public string MobileNo { get; set; }



        [Required(ErrorMessage = "Email ID is required")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Enter a valid email address")]
        public string EmailID { get; set; }


        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50, ErrorMessage = "First Name cannot be longer than 50 characters")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "First Name can contain only alphabets")]
        public string FirstName { get; set; }



        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, ErrorMessage = "Last Name cannot be longer than 50 characters")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Last Name can contain only alphabets")]
        public string LastName { get; set; }



        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]

        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
                       ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string PasswordHash { get; set; }



        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("PasswordHash", ErrorMessage = "Password and Confirm Password do not match")]
        public string ConfirmPassword { get; set; }


    }
}
