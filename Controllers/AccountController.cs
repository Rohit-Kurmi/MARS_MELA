using MARS_MELA_PROJECT.Models;
using MARS_MELA_PROJECT.Repository_Implementation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace MARS_MELA_PROJECT.Controllers
{
    public class AccountController : Controller
    {
        private readonly User _use;
        private readonly SignInCheckcs _sign;
        private readonly EmailHelper _emailHelper;


        public AccountController(User use, SignInCheckcs sign , EmailHelper emailHelper)
        {
            _use = use;
            _sign = sign;
            _emailHelper = emailHelper;
            
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
                    return RedirectToAction("SignIN", "Account");
                }

                // CASE 2: New user successfully created
                else if (res == 1)
                {

                    _emailHelper.SendVerificationMail(sign.EmailID);

                    TempData["message"] = "Registration successful! Please check your email for verification.";
                    // Redirect user to OTP verification page
                    return RedirectToAction("Index", "Account");
                }



                // CASE 3: Unexpected result or failure
                return View();
            }
            catch (Exception ex)
            {
                // If any exception occurs, stay on SignUP page
                return View();
            }
        }


        [HttpGet]
        public IActionResult EmailVerify(string token, string email)
        {
            if(string.IsNullOrEmpty(token))
            {
                TempData["invalidtoken"] = "Invalid token";
                return RedirectToAction("SignIn", "Account");
            }

            int result=_use.emailverificationcheck(token, email);

            if(result==1)
            {
                _use.updateEmailVerified(email);
                TempData["emailupdate"] = "Email Verified Successfully!";
                return RedirectToAction("Mobileverification","Account");
            }

            else if (result == -1)
            {

                return Content("Token expired! Please resend verification email.");
            }
            else if (result == 2)
            {
                return Content("Email already verified.");
            }
            else
            {
                return Content("Invalid verification link.");
            }
        }


        



        public IActionResult Mobileverification()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Mobileverification(Mobileverification mob, string actiontype)
        {
            if (actiontype == "Send_OTP")
            {
                ModelState.Remove("OTPCode");

                if (!ModelState.IsValid)
                    return View(mob);

                string otp = _use.GenerateAndSaveOTP(mob);

                if (!string.IsNullOrEmpty(otp))
                    TempData["OTPMessage"] = $"Your OTP is {otp}";

                return View(mob);
            }

            if (actiontype == "Submit")
            {
                if (!ModelState.IsValid)
                    return View(mob);

                int result = _use.verification(mob);

                if (result == 1)
                {
                    TempData["Success"] = "Mobile verified!";
                    return RedirectToAction("CreatePassword");
                }
                else if (result == -1)
                {
                    TempData["Error"] = "OTP expired! Please request a new OTP.";
                    return RedirectToAction("Mobileverification");
                }
                else
                {
                    TempData["Error"] = "Invalid OTP!";
                    return View(mob);
                }
            }

            return View(mob);
        }


        public IActionResult CreatePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreatePassword(CreatePassword creatpass)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(creatpass);
                }
                // Step 1: Hash the password using SHA256
                // This ensures the password is stored securely in the database
                

                // Step 2: Save the hashed password in the database
                // DAL.SavePassword returns the number of rows affected
                int result = _use.SavePassword(creatpass.MobileNo, creatpass.PasswordHash);

                // Step 3: Check if the password was saved successfully
                if (result > 0)
                {
                    // Password saved successfully
                    TempData["Success1"] = "Password created successfully!";

                    // Redirect user to SignIN page after password creation
                    return RedirectToAction("SignIn", "Account");
                }

                // Step 4: Password save failed
                // Show error message and stay on CreatePassword page
                else
                {
                    TempData["Error1"] = "Something went wrong!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }



        public IActionResult EnterPassword()
        {
            return View();
        }




        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult SignIn(SignIn sign)
        {
            if (!ModelState.IsValid)
            {
                return View(sign);
            }


            try
            {


                string result = _sign.CheckOnSignIN(sign);


                // CASE 1: User exists but NOT verified → Send to OTP page
                if (result == "NEED_EMAIL_VERIFICATION")
                {
                    TempData["Mobile"] = "You Have Recived An Email For Email Verification ";
                    return View();
                }



                else if (result == "NEED_MOBILE_VERIFICATION")
                {

                    return RedirectToAction("Mobileverification", "Account");
                }


                // CASE 2: User verified but has NO password yet
                // Redirect user to OTP page so that they can verify
                // and then create a new password
                else if (result == "CREATE_PASSWORD")
                {
                    return RedirectToAction("Verification", "Account");
                }


                // CASE 3: User verified AND password already exists
                // Take user to EnterPassword page
                else if (result == "LOGIN_ALLOWED")
                {
                    TempData["Mobile"] = sign.MobileNo;
                    return RedirectToAction("EnterPassword");
                }


                // CASE 4: User does not exist in database
                else
                {
                    ViewBag.msg = "User not found!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                return View();
            }



            return View();
        }










        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
