using System.ComponentModel.DataAnnotations;

namespace MARS_MELA_PROJECT.Models
{

    public class ForgotPassword
    {
        [Required(ErrorMessage = "Mobile number required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile must be 10 digits")]
        public string MobileNo { get; set; }


        [Required(ErrorMessage = "Email ID is required")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Enter a valid email address")]
        public string EmailID { get; set; }

    }


}
