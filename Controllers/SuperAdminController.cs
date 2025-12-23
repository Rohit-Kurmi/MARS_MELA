using MARS_MELA_PROJECT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


namespace MARS_MELA_PROJECT.Controllers
{
    public class SuperAdminController : Controller
    {


     

        public IActionResult SuperAdminDashboard()
        {
            string session = HttpContext.Session.GetString("SuperAdminMobileNo");

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


        public IActionResult TradeFair()
        {
            return View();
        }


        public IActionResult CreateMelaAdmin()
        {
            return View();
        }




    }
}
