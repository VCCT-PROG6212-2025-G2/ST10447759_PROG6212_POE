// In Controllers/ManagerController.cs
using ContractMonthlyClaimSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class ManagerController : Controller
    {
        private readonly InMemoryClaimService _claimService;

        public ManagerController(InMemoryClaimService claimService)
        {
            _claimService = claimService;
        }

        // GET: /Manager
        public IActionResult Index()
        {
            // Manager only sees claims that have been verified by the coordinator.
            var claimsForApproval = _claimService.GetAllClaims()
                .Where(c => c.Status == Models.ClaimStatus.CoordinatorApproved)
                .ToList();
            return View(claimsForApproval);
        }

        // POST: /Manager/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var claim = _claimService.GetClaimById(id);
            if (claim != null)
            {
                claim.Status = Models.ClaimStatus.ManagerApproved;
                _claimService.UpdateClaim(claim);
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Manager/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id)
        {
            var claim = _claimService.GetClaimById(id);
            if (claim != null)
            {
                claim.Status = Models.ClaimStatus.Rejected;
                _claimService.UpdateClaim(claim);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}