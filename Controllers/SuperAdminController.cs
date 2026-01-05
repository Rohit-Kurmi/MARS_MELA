using MARS_MELA_PROJECT.Models;
using MARS_MELA_PROJECT.Repository_Implementation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


namespace MARS_MELA_PROJECT.Controllers
{
    public class SuperAdminController : Controller
    {


        private readonly SuperAdmin _supad;
        //private readonly SignInCheckcs _sign;
        private readonly EmailHelper _emailHelper;



        public SuperAdminController(SuperAdmin supad, EmailHelper emailHelper)
        {
            _supad = supad;
            _emailHelper = emailHelper;

        }



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


        public IActionResult CreateTradeFair()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateTradeFair(CreateTradeFair CTF)
        {
            
            if (!ModelState.IsValid)
            {
                return View(CTF);
            }

            string session = HttpContext.Session.GetString("SuperAdminMobileNo");
            _supad.AddTraid(CTF, session);

            return RedirectToAction("SuperAdminDashboard","SuperAdmin");
        }




        public IActionResult MelaAdmin()
        {
            UserViewModel vm = new UserViewModel();
            vm.Rolesdropdowns = _supad.GetRoles(); // repository call
            vm.MelaAdmin = new MelaAdmin();


            return View(vm);
        }

        
        [HttpPost]
        public IActionResult MelaAdmin(UserViewModel model)
        {
            // 🔴 IMPORTANT: dropdown data always reload
            model.Rolesdropdowns = _supad.GetRoles();

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"KEY: {state.Key}, ERROR: {error.ErrorMessage}");
                    }
                }

            }

            // ✅ Safe assignment
            model.MelaAdmin.RoleID = model.RoleID.Value;

            string session = HttpContext.Session.GetString("SuperAdminMobileNo");

            try
            {
                _supad.MelaAdmin(model.MelaAdmin, session);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.Rolesdropdowns = _supad.GetRoles();
                return View(model);
            }

            

            return RedirectToAction("SuperAdminDashboard", "SuperAdmin");
        }





    }
}
