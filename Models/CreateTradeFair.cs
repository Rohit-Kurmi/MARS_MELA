using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using static System.Collections.Specialized.BitVector32;

namespace MARS_MELA_PROJECT.Models
{
    public class CreateTradeFair
    {

        [Required(ErrorMessage = "Fair Name is required")]
        [StringLength(300, ErrorMessage = "Fair Name cannot exceed 300 characters")]
        public string FairName { get; set; } = null!;

        [Required(ErrorMessage = "Division is required")]
        [StringLength(200)]
        public string Division { get; set; } = null!;

        [Required(ErrorMessage = "District is required")]
        [StringLength(200)]
        public string District { get; set; } = null!;

        [Required(ErrorMessage = "Tehsil is required")]
        [StringLength(200)]
        public string Tehsil { get; set; } = null!;

        [Required(ErrorMessage = "City is required")]
        [StringLength(200)]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Apply Start Date is required")]
        [DataType(DataType.Date)]
        public DateTime ApplyStartDate { get; set; }

        [Required(ErrorMessage = "Apply End Date is required")]
        [DataType(DataType.Date)]
        public DateTime ApplyEndDate { get; set; }

        //[Required(ErrorMessage = "Fair Logo Path is required")]
        //[StringLength(500)]
        //public string FairLogoPath { get; set; } = null!;


        [Required(ErrorMessage = "Fair logo is required")]
        public IFormFile FairLogo { get; set; } = null!;


        [Required(ErrorMessage = "Contact Mobile 1 is required")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter valid 10 digit mobile number")]
        public string ContactMobile1 { get; set; } = null!;

        [Required(ErrorMessage = "Contact Mobile 2 is required")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter valid 10 digit mobile number")]
        public string ContactMobile2 { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter valid email address")]
        public string ContactEmail { get; set; } = null!;

       
        public int Status { get; set; }

        
        public long CreatedBy { get; set; }

 
        public DateTime CreatedAt { get; set; }




    }
}



