using MARS_MELA_PROJECT.Models;
using MARS_MELA_PROJECT.Repository;
using MARS_MELA_PROJECT.Repository_Implementation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using PortalLib.Framework.Utilities;


//using PortalLib.Framework.Utilities;
using System.Diagnostics;

namespace MARS_MELA_PROJECT.Controllers
{
    public class AccountController : Controller
    {
        private readonly User _use;
        //private readonly SignInCheckcs _sign;
        private readonly EmailHelper _emailHelper;



        public AccountController(User use , EmailHelper emailHelper)
        {
            _use = use;
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
                    TempData["UserAlready"] = "User Already Exists";

                    // Redirect user to SignIN page
                    return RedirectToAction("SignIN", "Account");
                }

                // CASE 2: New user successfully created
                else if (res == 1)
                {

                    try
                    {
                        _emailHelper.SendVerificationMail(sign.EmailID,sign.MobileNo);
                    }
                    catch
                    {
                        _emailHelper.ClearEmailToken(sign.EmailID,sign.MobileNo);
                    }


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
        public IActionResult EmailVerify(string token, string email,string mobile)
        {
            
            if(string.IsNullOrEmpty(token))
            {
                TempData["invalidtoken"] = "Invalid token";
                return RedirectToAction("SignIn", "Account");
            }

            email = PortalEncryption.DecryptPasswordNew(email);

            int result=_use.emailverificationcheck(token, email);

            if(result==1)
            {
                _use.updateEmailVerified(email);
                TempData["emailupdate"] = "Email Verified Successfully!";
                return RedirectToAction("Mobileverification","Account");
            }

            else if (result == -1)
            {
                email = PortalEncryption.EncryptPasswordNew(email);

                return RedirectToAction("VerificationExpired","Account", new { email = email,mobile=mobile });
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


        [HttpGet]
        public IActionResult VerificationExpired(string email,string mobile)
        {


            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(mobile))
            {
                return NotFound();
            }
            email = PortalEncryption.DecryptPasswordNew(email);
            mobile = PortalEncryption.DecryptPasswordNew(mobile);
           
           
            var model = new ForgotPassword()
            {
                EmailID = email,
                MobileNo = mobile
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult VerificationExpired(ForgotPassword model)
        {
            if (string.IsNullOrEmpty(model.EmailID))
            {
                TempData["error"] = "Invalid request.";
                return RedirectToAction("SignIn");
            }

            // 👉 New token generate + mail send
            _emailHelper.SendVerificationMail(model.EmailID, model.MobileNo);

            TempData["resendvarifymail"] =
                "Verification email has been resent. Please check your inbox.";

            return RedirectToAction("Index","Account");
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

                if (otp == "WAIT_1_MIN")
                {
                    TempData["Error"] = "Please wait 1 minute before resending OTP.";
                }
                else if (otp == "BLOCK_1_HOUR")
                {
                    TempData["Error"] = "OTP limit reached. Try again after 1 hour.";
                }
                else if (!string.IsNullOrEmpty(otp))
                {
                    TempData["OTPMessage"] = $"Your OTP is {otp}";
                    TempData["StartOTPTimer"] = "true"; // Flag for JS timer
                }

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
                // Step 1: Hash the password using SHA512
                // This ensures the password is stored securely in the database



                string hasspassword = PortalEncryption.GetSHA512(creatpass.PasswordHash);

                // Step 2: Save the hashed password in the database
                // DAL.SavePassword returns the number of rows affected
                int result = _use.SavePassword(creatpass.MobileNo, hasspassword);

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
                string result = _use.CheckOnSignIN(sign);


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
                    return RedirectToAction("Mobileverification", "Account");
                }


                // CASE 3: User verified AND password already exists
                // Take user to EnterPassword page
                else if (result == "LOGIN_ALLOWED")
                {
                    TempData["Mobile"] = sign.MobileNo;
                    return RedirectToAction("EnterPassword","Account");
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
        }



        public IActionResult ForgotPassword()
        {

            
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPassword fogpass)
        {
            if (!ModelState.IsValid)
            {
                return View(fogpass);
            }

            try
            {
                // 1️⃣ Check user exists
                bool isUserExist = _use.IsUserExist(fogpass.EmailID, fogpass.MobileNo);

                if (!isUserExist)
                {
                    TempData["frogetpassmessage"] =
                        "No account found with this email or mobile number.";
                    return RedirectToAction("ForgotPassword", "Account");
                }

                // 2️⃣ Send mail first
                _emailHelper.SendForgotpasswordMail(fogpass.EmailID,fogpass.MobileNo);

                // 3️⃣ Mail success → mark unverified
                _use.MarkUserUnverifiedForForgot(fogpass.EmailID, fogpass.MobileNo);

                TempData["frogetpassmessage"] =
                    "Password reset link has been sent to your email.";

                return RedirectToAction("Index", "Account");
            }
            catch
            {
                TempData["frogetpassmessage"] =
                    "Unable to send reset email. Please try again later.";
                return RedirectToAction("ForgotPassword", "Account");
            }
        }





        public IActionResult EnterPassword()
        {
            var model = new EnterPassword()
            {
                MobileNo = TempData["Mobile"]?.ToString()
            };
            return View(model);
        }


        [HttpPost]
        public IActionResult EnterPassword(EnterPassword pass)
        {
            if(!ModelState.IsValid)
            {
                return View(pass);
            }

            int result =_use.SignIn(pass);

            if (result == 0)
            {
                TempData["loginnotfound"] = " User Not Found";
                return View(pass);

            }

            if (result == -1)
            {
                TempData["wrongpassword"] = "Wrong Password";
                return View(pass);

            }

            if (result == 1)
            {
                HttpContext.Session.SetString("CitizenMobileNo", pass.MobileNo);
                return RedirectToAction("CitizenDashboard", "Clitizencompanydashboard");

            }

            if (result == 2)
            {
                HttpContext.Session.SetString("SuperAdminMobileNo", pass.MobileNo);
                return RedirectToAction("SuperAdminDashboard", "SuperAdmin");

            }

            return View(pass);
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






        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
