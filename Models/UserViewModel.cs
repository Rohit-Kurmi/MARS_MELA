using System.ComponentModel.DataAnnotations;
using System.Data;

namespace MARS_MELA_PROJECT.Models

{
    public class UserViewModel
    {

        [Required(ErrorMessage = "Role is required")]
        public int? RoleID { get; set; }

        public List<RolesdropDown> Rolesdropdowns { get; set; }

        public MelaAdmin MelaAdmin { get; set; }


    }
}
