using System.Diagnostics;
using MARS_MELA_PROJECT.Models;
using MARS_MELA_PROJECT.Repository_Implementation;
using Microsoft.AspNetCore.Mvc;

namespace MARS_MELA_PROJECT.Controllers
{
    public class AccountController : Controller
    {
        private readonly User _use;

        public AccountController(User use)
        {
            _use = use;
        }






        public IActionResult Index()
        {
            return View();
        }





        public IActionResult Privacy()
        {
            return View();
        }


        //========================================================
        // SIGN UP - GET
        //========================================================
        public IActionResult SignUP()
        {
            return View();
        }


        //========================================================
        // SIGN UP - POST
        //========================================================
        [HttpPost]
        public IActionResult SignUP(SignUP sign)
        {
            if (!ModelState.IsValid)
            {
                return View(sign);
            }

            try
            {
                // Call DAL to insert user into database
                // Return values:
                //  -1 = User already exists
                //   1 = User successfully registered
                int res = _use.AddUser(sign);

                // CASE 1: User already exists
                if (res == -1)
                {
                    TempData["message"] = "User Already Exists";

                    // Redirect user to SignIN page
                    return RedirectToAction("SignIN", "Home");
                }

                // CASE 2: New user successfully created
                else if (res == 1)
                {
                    TempData["message"] = "User Registered Successfully";

                    // Pass mobile and email to next page using TempData
                    TempData["Mobile"] = sign.MobileNo;
                    TempData["Email"] = sign.EmailID;

                    // Redirect user to OTP verification page
                    return RedirectToAction("Verification", "Home");
                }

                // CASE 3: Unexpected result or failure
                return View();
            }
            catch
            {
                // If any exception occurs, stay on SignUP page
                return View();
            }
        }












        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
