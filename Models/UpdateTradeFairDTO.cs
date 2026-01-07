namespace MARS_MELA_PROJECT.Models
{
    public class UpdateTradeFairDTO
    {
        public int FairId { get; set; }

      
        public string FairName { get; set; }

        public string? Division { get; set; }
        public string? District { get; set; }
        public string? Tehsil { get; set; }
        public string? City { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ApplyStartDate { get; set; }
        public DateTime? ApplyEndDate { get; set; }

        public string? ContactEmail { get; set; }
        public string? ContactMobile1 { get; set; }
        public string? ContactMobile2 { get; set; }

        public bool Status { get; set; }   // checkbox

        public string? ExistingLogoPath { get; set; } // preview
        public IFormFile? NewLogo { get; set; }       // optional


    }
}
