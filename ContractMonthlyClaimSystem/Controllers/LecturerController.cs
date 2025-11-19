// In Controllers/LecturerController.cs
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Reflection.Metadata;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class LecturerController : Controller
    {
        private readonly InMemoryClaimService _claimService;
        private readonly FileEncryptionService _fileEncryptionService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LecturerController(
            InMemoryClaimService claimService,
            FileEncryptionService fileEncryptionService,
            IWebHostEnvironment webHostEnvironment)
        {
            _claimService = claimService;
            _fileEncryptionService = fileEncryptionService;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Create()
        {
            return View();
        }

        // --- THE FINAL FIX ---
        // We are increasing the limit to 100 MB. This ensures our C# code's
        // 10 MB validation is what the user sees.
        [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClaimViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? originalFileName = null;
            string? encryptedFilePath = null;

            if (model.SupportingDocument != null && model.SupportingDocument.Length > 0)
            {
                // 1. File Validation
                var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
                var fileExtension = Path.GetExtension(model.SupportingDocument.FileName).ToLowerInvariant();
                var maxFileSize = 10 * 1024 * 1024; // 10 MB

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("SupportingDocument", "Invalid file type. Only PDF, DOCX, and XLSX are allowed.");
                    return View(model);
                }

                if (model.SupportingDocument.Length > maxFileSize)
                {   
                    ModelState.AddModelError("SupportingDocument", "File size cannot exceed 10 MB.");
                    return View(model);
                }

                // 2. File Processing
                originalFileName = model.SupportingDocument.FileName;
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                Directory.CreateDirectory(uploadsFolder);

                encryptedFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 3. Encryption
                await _fileEncryptionService.EncryptFileAsync(model.SupportingDocument, encryptedFilePath);
            }

            var newClaim = new Claim
            {
                LecturerName = "Dr. Eleanor Vance",
                HoursWorked = model.HoursWorked,
                HourlyRate = model.HourlyRate,
                Notes = model.Notes,
                DocumentFileName = originalFileName,
                DocumentFilePath = encryptedFilePath
            };

            _claimService.AddClaim(newClaim);

            return RedirectToAction("MyClaims");
        }

        public IActionResult MyClaims()
        {
            var claims = _claimService.GetAllClaims();
            return View(claims);
        }
    }
}