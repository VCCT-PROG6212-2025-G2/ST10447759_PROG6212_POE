// In Services/InMemoryClaimService.cs
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Services
{
    public class InMemoryClaimService
    {
        // Use a static list to ensure data persists across HTTP requests.
        private static readonly List<Claim> _claims = new List<Claim>();
        private static int _nextId = 1;

        // Static constructor to add sample data once
        static InMemoryClaimService()
        {
            InitializeData();
        }

        private static void InitializeData()
        {
            // Add some sample data to work with
            _claims.Add(new Claim
            {
                Id = _nextId++,
                LecturerName = "Dr. Eleanor Vance",
                HoursWorked = 5,
                HourlyRate = 850,
                SubmissionDate = new DateTime(2024, 7, 15),
                Status = ClaimStatus.ManagerApproved
            });
            _claims.Add(new Claim
            {
                Id = _nextId++,
                LecturerName = "Dr. Eleanor Vance",
                HoursWorked = 6,
                HourlyRate = 850,
                SubmissionDate = new DateTime(2024, 6, 20),
                Status = ClaimStatus.Rejected
            });
        }

        public List<Claim> GetAllClaims()
        {
            // Return claims ordered by the most recent submission date
            return _claims.OrderByDescending(c => c.SubmissionDate).ToList();
        }

        public Claim? GetClaimById(int id)
        {
            return _claims.FirstOrDefault(c => c.Id == id);
        }

        public void AddClaim(Claim claim)
        {
            claim.Id = _nextId++;
            claim.SubmissionDate = DateTime.Now;
            claim.Status = ClaimStatus.Pending; // All new claims start as Pending
            _claims.Add(claim);
        }

        public void UpdateClaim(Claim updatedClaim)
        {
            var existingClaim = _claims.FirstOrDefault(c => c.Id == updatedClaim.Id);
            if (existingClaim != null)
            {
                // Update the properties you want to be changeable, e.g., status
                existingClaim.Status = updatedClaim.Status;
            }
        }
    }
}