using MARS_MELA_PROJECT.Models;
using MARS_MELA_PROJECT.Repository_Implementation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using NuGet.Protocol.Core.Types;


namespace MARS_MELA_PROJECT.Controllers
{
    public class SuperAdminController : Controller
    {
        private readonly IConfiguration _config;


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

        public IActionResult UpdateTradeFair()
        {
            return View();
        }

        // ================= GetTradeFair (AJAX) =================
        [HttpGet]
        public IActionResult GetTradeFair(int? id, string? email)
        {
            if (id == null && string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Please provide Fair ID or Email" });
            }

            try
            {
                var fair = _supad.GetTradeFair(id, email);

                if (fair == null)
                {
                    return Json(new { success = false, message = "Trade Fair not found" });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        fairId = fair.FairId,
                        fairName = fair.FairName,
                        division = fair.Division,
                        district = fair.District,
                        tehsil = fair.Tehsil,
                        city = fair.City,
                        contactEmail = fair.ContactEmail,
                        contactMobile1 = fair.ContactMobile1,
                        contactMobile2 = fair.ContactMobile2,

                        startDate = fair.StartDate?.ToString("yyyy-MM-dd"),
                        endDate = fair.EndDate?.ToString("yyyy-MM-dd"),
                        applyStartDate = fair.ApplyStartDate?.ToString("yyyy-MM-dd"),
                        applyEndDate = fair.ApplyEndDate?.ToString("yyyy-MM-dd"),

                        status = fair.Status,
                        existingLogoPath = fair.ExistingLogoPath
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        

        // ================= UpdateTradeFair (POST) =================
       [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateTradeFair(TradeFairUpdateVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                bool updated = _supad.UpdateTradeFair(model.Fair.FairId, model.Fair);

                if (updated)
                    TempData["Success"] = "Trade Fair updated successfully!";
                else
                    TempData["Error"] = "Trade Fair not found or not updated!";

                return RedirectToAction("UpdateTradeFair");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("UpdateTradeFair");
            }
        }


    
        [HttpPost]
        public IActionResult MelaAdmin(UserViewModel model)
        {
            // 🔴 Dropdown hamesha reload
            model.Rolesdropdowns = _supad.GetRoles();

            if (!ModelState.IsValid)
            {
                return View(model); // 🔥 MOST IMPORTANT LINE
            }

            // ✅ Ab safe hai
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


        public IActionResult UpdateFairAdmin ()
        {
            return View();
        }





    }
}
