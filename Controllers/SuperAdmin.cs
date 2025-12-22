using MARS_MELA_PROJECT.Models;
using Microsoft.AspNetCore.Mvc;


namespace MARS_MELA_PROJECT.Controllers
{
    public class SuperAdmin : Controller
    {


        public IActionResult Dashbord()
        {
            return View();
        }





        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            // Cache prevent (back button issue)
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return RedirectToAction("SignIn", "Account"); // Login page
        }


    }
}
