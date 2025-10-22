// In Models/Claim.cs
using ContractMonthlyClaimSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    public class Claim
    {
        public int Id { get; set; } 

        [Required(ErrorMessage = "Lecturer Name is required.")]
        public string LecturerName { get; set; } = "Dr. Eleanor Vance"; // Hardcoded for simulation

        [Required(ErrorMessage = "Hours Worked is required.")]
        [Range(1, 100, ErrorMessage = "Hours must be between 1 and 100.")]
        public double HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly Rate is required.")]
        [Range(1, 2000, ErrorMessage = "Hourly rate must be between 1 and 2000.")]
        public decimal HourlyRate { get; set; }

        public decimal TotalAmount => (decimal)HoursWorked * HourlyRate;

        public string? Notes { get; set; }

        public DateTime SubmissionDate { get; set; }

        public ClaimStatus Status { get; set; }

        // To store information about the uploaded document
        public string? DocumentFileName { get; set; }
        public string? DocumentFilePath { get; set; } // This will be the path to the ENCRYPTED file
    }
}