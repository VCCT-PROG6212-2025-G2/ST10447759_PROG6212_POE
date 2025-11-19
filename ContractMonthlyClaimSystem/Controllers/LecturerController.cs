using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class LecturerController : Controller
    {
        private readonly CMCSContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FileEncryptionService _fileEncryptionService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LecturerController(
            CMCSContext context,
            UserManager<ApplicationUser> userManager,
            FileEncryptionService fileEncryptionService,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _fileEncryptionService = fileEncryptionService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Lecturer/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // AUTOMATION: Fetch the current user to get their specific Hourly Rate
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Pass the rate to the view for the auto-calculator (Read-Only)
            ViewBag.HourlyRate = user.HourlyRate;
            return View();
        }

        // POST: Lecturer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Increase limit for file uploads if needed
        [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
        public async Task<IActionResult> Create(ClaimViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // 1. AUTOMATION & SECURITY: Pull official rate from DB. 
            // We do NOT trust the form input for money calculations.
            decimal officialRate = user.HourlyRate;

            // 2. VALIDATION FIX: 
            // The form doesn't send HourlyRate anymore (because it's auto-filled/readonly).
            // We remove it from validation so ModelState.IsValid doesn't fail.
            ModelState.Remove("HourlyRate");

            // 2. VALIDATION: Check Business Logic (Max 180 hours)
            if (model.HoursWorked > 180)
            {
                ModelState.AddModelError("HoursWorked", "You cannot claim more than 180 hours in a month.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.HourlyRate = officialRate; // Reset for view
                return View(model);
            }

            // 3. FILE PROCESSING (Existing logic adapted)
            string? originalFileName = null;
            string? encryptedFilePath = null;

            if (model.SupportingDocument != null && model.SupportingDocument.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
                var fileExtension = Path.GetExtension(model.SupportingDocument.FileName).ToLowerInvariant();
                var maxFileSize = 10 * 1024 * 1024; // 10 MB

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("SupportingDocument", "Invalid file type. Only PDF, DOCX, and XLSX are allowed.");
                    ViewBag.HourlyRate = officialRate;
                    return View(model);
                }

                if (model.SupportingDocument.Length > maxFileSize)
                {
                    ModelState.AddModelError("SupportingDocument", "File size cannot exceed 10 MB.");
                    ViewBag.HourlyRate = officialRate;
                    return View(model);
                }

                // Secure storage path
                originalFileName = model.SupportingDocument.FileName;
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);
                encryptedFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Encrypt
                await _fileEncryptionService.EncryptFileAsync(model.SupportingDocument, encryptedFilePath);
            }

            // 4. CREATE & SAVE CLAIM
            var claim = new Claim
            {
                // Ensure your Claim model has a LecturerID string property to link to AspNetUsers
                // LecturerID = user.Id, 
                LecturerName = $"{user.FirstName} {user.LastName}",
                HoursWorked = model.HoursWorked,
                HourlyRate = officialRate, // Automated Rate
                Notes = model.Notes,
                SubmissionDate = DateTime.Now,
                Status = ClaimStatus.Pending,
                DocumentFileName = originalFileName,
                DocumentFilePath = encryptedFilePath
            };

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyClaims));
        }

        // GET: Lecturer/MyClaims
        public async Task<IActionResult> MyClaims()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Fetch only claims belonging to this user
            // Assuming you added a 'LecturerName' or 'LecturerID' to filter by
            var claims = await _context.Claims
                                       .Where(c => c.LecturerName == $"{user.FirstName} {user.LastName}")
                                       .OrderByDescending(c => c.SubmissionDate)
                                       .ToListAsync();

            return View(claims);
        }
    }
}