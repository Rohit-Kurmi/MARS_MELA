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

                string token = Guid.NewGuid().ToString();
                // Call DAL to insert user into database
                // Return values:
                //  -1 = User already exists
                //   1 = User successfully registered
                int res = _use.AddUser(sign,token);

                // CASE 1: User already exists
                if (res == -1)
                {
                    TempData["message"] = "User Already Exists";

                    // Redirect user to SignIN page
                    return RedirectToAction("SignIN", "Account");
                }

                // CASE 2: New user successfully created
                else if (res == 1)
                {
                    

                    string verifyurl = Url.Action("EmailVerify","Account",new {token=token},Request.Scheme);

                    string body = $"<h3>Welcome {sign.FirstName} </h3>" + $"<h3>Welcome {sign.LastName} </h3><br/>"
                        + $"Click here to verify your email: <a href='{verifyurl}'>Verify Email</a>\";";




                    TempData["message"] = "Registration successful! Please check your email for verification.";
                    // Redirect user to OTP verification page
                    return RedirectToAction("SignIn", "Account");
                }



                // CASE 3: Unexpected result or failure
                return View();
            }
            catch(Exception ex)
            {
                // If any exception occurs, stay on SignUP page
                return View();
            }
        }



        public IActionResult EmailVerify()
        {
            return View();
        }

        public IActionResult SignIn()
        {
            return View();
        }










        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
