using System.ComponentModel.DataAnnotations;

namespace MARS_MELA_PROJECT.Models
{
    public class Mobileverification
    {



        [Required(ErrorMessage = "Mobile number required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile must be 10 digits")]
        public string MobileNo { get; set; }



        

        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, ErrorMessage = "OTP must be 6 digits")]
        public string OTPCode { get; set; }

    }
}
