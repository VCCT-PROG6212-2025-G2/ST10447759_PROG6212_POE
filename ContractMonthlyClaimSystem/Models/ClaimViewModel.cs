// In Models/ClaimViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    public class ClaimViewModel
    {
        [Required(ErrorMessage = "Hours Worked is required.")]
        [Range(1, 100, ErrorMessage = "Hours must be between 1 and 100.")]
        public double HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly Rate is required.")]
        [Range(1, 2000, ErrorMessage = "Hourly rate must be between 1 and 2000.")]
        public decimal HourlyRate { get; set; }

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }

        // This property will hold the uploaded file from the form
        public IFormFile? SupportingDocument { get; set; }
    }
}