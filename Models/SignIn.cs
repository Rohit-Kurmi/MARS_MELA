using System.ComponentModel.DataAnnotations;

namespace MARS_MELA_PROJECT.Models
{
    public class SignIn
    {

        [Required(ErrorMessage = "Mobile number required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile must be 10 digits")]
        public string MobileNo { get; set; }

    }
}
