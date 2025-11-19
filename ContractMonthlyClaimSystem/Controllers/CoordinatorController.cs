// In Controllers/CoordinatorController.cs
using ContractMonthlyClaimSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class CoordinatorController : Controller
    {
        private readonly InMemoryClaimService _claimService;

        public CoordinatorController(InMemoryClaimService claimService)
        {
            _claimService = claimService;
        }

        // GET: /Coordinator
        public IActionResult Index()
        {
            // Coordinator only sees claims that are pending initial review.
            var pendingClaims = _claimService.GetAllClaims()
                .Where(c => c.Status == Models.ClaimStatus.Pending)
                .ToList();
            return View(pendingClaims);
        }

        // POST: /Coordinator/Verify
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Verify(int id)
        {
            var claim = _claimService.GetClaimById(id);
            if (claim != null)
            {
                claim.Status = Models.ClaimStatus.CoordinatorApproved;
                _claimService.UpdateClaim(claim);
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Coordinator/Reject
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