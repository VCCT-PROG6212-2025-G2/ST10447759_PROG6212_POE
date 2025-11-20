using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    public class ClaimViewModel
    {
        [Required(ErrorMessage = "Hours Worked is required.")]
        // Update range to 200 so it doesn't block the 180 limit check in the controller
        [Range(1, 200, ErrorMessage = "Hours must be between 1 and 200.")]
        [Display(Name = "Hours Worked (Monthly)")]
        public double HoursWorked { get; set; }

        // We will handle the "Required" check for this in the Controller manually now
        [Display(Name = "Hourly Rate (ZAR)")]
        public decimal HourlyRate { get; set; }

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }

        public IFormFile? SupportingDocument { get; set; }
    }
}