using Microsoft.AspNetCore.Identity;

namespace ContractMonthlyClaimSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public decimal HourlyRate { get; set; } // HR sets this
    }
}