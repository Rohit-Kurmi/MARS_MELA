using Azure;
using Microsoft.AspNetCore.Mvc;

namespace MARS_MELA_PROJECT.Controllers
{
    public class ClitizencompanydashboardController:Controller
    {




        public IActionResult CitizenDashboard()
        {
            string session = HttpContext.Session.GetString("CitizenMobileNo");

            if (session == null)
            {
                return NotFound();
            }

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


       


        public IActionResult MyProfile()
        {
            return View();
        }





    }
}
