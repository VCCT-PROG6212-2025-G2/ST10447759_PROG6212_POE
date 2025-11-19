using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Authorize(Roles = "HR")] // Security: Only HR users can access this controller
    public class HRController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CMCSContext _context;

        // Constructor: Inject both UserManager (for creating users) and CMCSContext (for reports)
        public HRController(UserManager<ApplicationUser> userManager, CMCSContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: /HR/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: /HR/AddLecturer
        [HttpGet]
        public IActionResult AddLecturer()
        {
            return View();
        }

        // POST: /HR/AddLecturer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLecturer(string firstName, string lastName, string email, string phoneNumber, decimal hourlyRate, string password)
        {
            // Basic Validation
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and Password are required.");
                return View();
            }

            // Create the new User object with custom fields
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                HourlyRate = hourlyRate, // HR sets this rate here
                EmailConfirmed = true    // Auto-confirm since an admin is creating it
            };

            // 1. Create User in the Database
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // 2. Assign the "Lecturer" Role
                await _userManager.AddToRoleAsync(user, "Lecturer");

                ViewBag.Message = $"Lecturer {firstName} {lastName} added successfully!";

                // Return the view to allow adding another lecturer easily
                // (ModelState is cleared automatically on a new request, but here we just stay on the page)
                ModelState.Clear();
                return View();
            }
            else
            {
                // Display errors (e.g., "Password too weak", "Email already taken")
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // If we got here, something failed, redisplay form
            return View();
        }

        // GET: /HR/Reports
        [HttpGet]
        public IActionResult Reports()
        {
            // Fetch claims that have been fully approved by the Manager
            var approvedClaims = _context.Claims
                .Where(c => c.Status == ClaimStatus.ManagerApproved)
                // We don't strictly need .Include(c => c.Lecturer) if we stored LecturerName as a string in the Claim model.
                // But if you added a navigation property, keep it. 
                // Based on your previous Claim model, you stored 'LecturerName' directly, so .Include isn't strictly necessary 
                // unless you added a 'public virtual ApplicationUser Lecturer { get; set; }' to your Claim model.
                .OrderByDescending(c => c.SubmissionDate)
                .ToList();

            return View(approvedClaims);
        }

        // GET: /HR/ManageLecturers
        [HttpGet]
        public async Task<IActionResult> ManageLecturers()
        {
            // Get all users in the 'Lecturer' role
            var lecturers = await _userManager.GetUsersInRoleAsync("Lecturer");
            return View(lecturers);
        }

        // GET: /HR/EditLecturer/{id}
        [HttpGet]
        public async Task<IActionResult> EditLecturer(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: /HR/EditLecturer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLecturer(string id, string firstName, string lastName, string email, decimal hourlyRate)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Update fields
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;
            user.UserName = email; // Keep username same as email
            user.HourlyRate = hourlyRate;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                ViewBag.Message = "Lecturer details updated successfully!";
                return View(user);
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(user);
        }
    }
}